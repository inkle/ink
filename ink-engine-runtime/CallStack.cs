using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Ink.Runtime
{
    internal class CallStack
    {
        internal class Element
        {
            public Container currentContainer;
            public int currentContentIndex;

            public bool inExpressionEvaluation;
            public Dictionary<string, Runtime.Object> temporaryVariables;
            public PushPopType type;

            public Runtime.Object currentObject {
                get {
                    if (currentContainer && currentContentIndex < currentContainer.content.Count) {
                        return currentContainer.content [currentContentIndex];
                    }

                    return null;
                }
                set {
                    var currentObj = value;
                    if (currentObj == null) {
                        currentContainer = null;
                        currentContentIndex = 0;
                        return;
                    }

                    currentContainer = currentObj.parent as Container;
                    if (currentContainer != null)
                        currentContentIndex = currentContainer.content.IndexOf (currentObj);

                    // Two reasons why the above operation might not work:
                    //  - currentObj is already the root container
                    //  - currentObj is a named container rather than being an object at an index
                    if (currentContainer == null || currentContentIndex == -1) {
                        currentContainer = currentObj as Container;
                        currentContentIndex = 0;
                    }
                }
            }

            public Element(PushPopType type, Container container, int contentIndex, bool inExpressionEvaluation = false) {
                this.currentContainer = container;
                this.currentContentIndex = contentIndex;
                this.inExpressionEvaluation = inExpressionEvaluation;
                this.temporaryVariables = new Dictionary<string, Object>();
                this.type = type;
            }

            public Element Copy()
            {
                var copy = new Element (this.type, this.currentContainer, this.currentContentIndex, this.inExpressionEvaluation);
                copy.temporaryVariables = new Dictionary<string,Object>(this.temporaryVariables);
                return copy;
            }
        }

        internal class Thread
        {
            public List<Element> callstack;
            public int threadIndex;
            public Runtime.Object previousContentObject;

            public Thread() {
                callstack = new List<Element>();
            }

			public Thread(Dictionary<string, object> jThreadObj, Story storyContext) : this() {
                threadIndex = (int) jThreadObj ["threadIndex"];

				List<object> jThreadCallstack = (List<object>) jThreadObj ["callstack"];
				foreach (object jElTok in jThreadCallstack) {

					var jElementObj = (Dictionary<string, object>)jElTok;

                    PushPopType pushPopType = (PushPopType)(int)jElementObj ["type"];

					Container currentContainer = null;
					int contentIndex = 0;

					string currentContainerPathStr = null;
					object currentContainerPathStrToken;
					if (jElementObj.TryGetValue ("cPath", out currentContainerPathStrToken)) {
						currentContainerPathStr = currentContainerPathStrToken.ToString ();
						currentContainer = storyContext.ContentAtPath (new Path(currentContainerPathStr)) as Container;
                        contentIndex = (int) jElementObj ["idx"];
					}

                    bool inExpressionEvaluation = (bool)jElementObj ["exp"];

					var el = new Element (pushPopType, currentContainer, contentIndex, inExpressionEvaluation);

					var jObjTemps = (Dictionary<string, object>) jElementObj ["temp"];
					el.temporaryVariables = Json.JObjectToDictionaryRuntimeObjs (jObjTemps);

					callstack.Add (el);
				}

				object prevContentObjPath;
				if( jThreadObj.TryGetValue("previousContentObject", out prevContentObjPath) ) {
					var prevPath = new Path((string)prevContentObjPath);
                    previousContentObject = storyContext.ContentAtPath(prevPath);
                }
			}

            public Thread Copy() {
                var copy = new Thread ();
                copy.threadIndex = threadIndex;
                foreach(var e in callstack) {
                    copy.callstack.Add(e.Copy());
                }
                copy.previousContentObject = previousContentObject;
                return copy;
            }

			public Dictionary<string, object> jsonToken {
				get {
					var threadJObj = new Dictionary<string, object> ();

					var jThreadCallstack = new List<object> ();
					foreach (CallStack.Element el in callstack) {
						var jObj = new Dictionary<string, object> ();
						if (el.currentContainer) {
							jObj ["cPath"] = el.currentContainer.path.componentsString;
							jObj ["idx"] = el.currentContentIndex;
						}
						jObj ["exp"] = el.inExpressionEvaluation;
						jObj ["type"] = (int) el.type;
						jObj ["temp"] = Json.DictionaryRuntimeObjsToJObject (el.temporaryVariables);
						jThreadCallstack.Add (jObj);
					}

					threadJObj ["callstack"] = jThreadCallstack;
					threadJObj ["threadIndex"] = threadIndex;

                    if (previousContentObject != null)
                        threadJObj ["previousContentObject"] = previousContentObject.path.ToString();

					return threadJObj;
				}
			}
        }

        public List<Element> elements {
            get {
                return callStack;
            }
        }

		public int depth {
			get {
				return elements.Count;
			}
		}

        public Element currentElement { 
            get { 
                return callStack [callStack.Count - 1];
            } 
        }

        public int currentElementIndex {
            get {
                return callStack.Count - 1;
            }
        }

        public Thread currentThread
        {
            get {
                return _threads [_threads.Count - 1];
            }
            set {
                Debug.Assert (_threads.Count == 1, "Shouldn't be directly setting the current thread when we have a stack of them");
                _threads.Clear ();
                _threads.Add (value);
            }
        }

        public bool canPop {
            get {
                return callStack.Count > 1;
            }
        }

        public CallStack (Container rootContentContainer)
        {
            _threads = new List<Thread> ();
            _threads.Add (new Thread ());

            _threads [0].callstack.Add (new Element (PushPopType.Tunnel, rootContentContainer, 0));
        }

        public CallStack(CallStack toCopy)
        {
            _threads = new List<Thread> ();
            foreach (var otherThread in toCopy._threads) {
                _threads.Add (otherThread.Copy ());
            }
        }
            
        // Unfortunately it's not possible to implement jsonToken since
        // the setter needs to take a Story as a context in order to
        // look up objects from paths for currentContainer within elements.
        public void SetJsonToken(Dictionary<string, object> jObject, Story storyContext)
        {
            _threads.Clear ();

            var jThreads = (List<object>) jObject ["threads"];

            foreach (object jThreadTok in jThreads) {
                var jThreadObj = (Dictionary<string, object>)jThreadTok;
                var thread = new Thread (jThreadObj, storyContext);
                _threads.Add (thread);
            }

            _threadCounter = (int)jObject ["threadCounter"];
        }
            
        // See above for why we can't implement jsonToken
        public Dictionary<string, object> GetJsonToken() {

            var jObject = new Dictionary<string, object> ();

            var jThreads = new List<object> ();
            foreach (CallStack.Thread thread in _threads) {
				jThreads.Add (thread.jsonToken);
            }

            jObject ["threads"] = jThreads;
            jObject ["threadCounter"] = _threadCounter;

            return jObject;
        }

        public void PushThread()
        {
            var newThread = currentThread.Copy ();
            _threadCounter++;
            newThread.threadIndex = _threadCounter;
            _threads.Add (newThread);
        }

        public void PopThread()
        {
            if (canPopThread) {
                _threads.Remove (currentThread);
            } else {
				throw new System.Exception("Can't pop thread");
            }
        }

        public bool canPopThread
        {
            get {
                return _threads.Count > 1;
            }
        }

        public void Push(PushPopType type)
        {
            // When pushing to callstack, maintain the current content path, but jump out of expressions by default
            callStack.Add (new Element(type, currentElement.currentContainer, currentElement.currentContentIndex, inExpressionEvaluation: false));
        }

        public bool CanPop(PushPopType? type = null) {

            if (!canPop)
                return false;
            
            if (type == null)
                return true;
            
            return currentElement.type == type;
        }
            
        public void Pop(PushPopType? type = null)
        {
            if (CanPop (type)) {
                callStack.RemoveAt (callStack.Count - 1);
                return;
            } else {
				throw new System.Exception("Mismatched push/pop in Callstack");
            }
        }

        // Get variable value, dereferencing a variable pointer if necessary
        public Runtime.Object GetTemporaryVariableWithName(string name, int contextIndex = -1)
        {
            if (contextIndex == -1)
                contextIndex = currentElementIndex+1;
            
            Runtime.Object varValue = null;

            var contextElement = callStack [contextIndex-1];

            if (contextElement.temporaryVariables.TryGetValue (name, out varValue)) {
                return varValue;
            } else {
                return null;
            }
        }
            
        public void SetTemporaryVariable(string name, Runtime.Object value, bool declareNew, int contextIndex = -1)
        {
            if (contextIndex == -1)
                contextIndex = currentElementIndex+1;

            var contextElement = callStack [contextIndex-1];
            
            if (!declareNew && !contextElement.temporaryVariables.ContainsKey(name)) {
                throw new StoryException ("Could not find temporary variable to set: " + name);
            }

            Runtime.Object oldValue;
            if( contextElement.temporaryVariables.TryGetValue(name, out oldValue) )
                ListValue.RetainListOriginsForAssignment (oldValue, value);

            contextElement.temporaryVariables [name] = value;
        }

        // Find the most appropriate context for this variable.
        // Are we referencing a temporary or global variable?
        // Note that the compiler will have warned us about possible conflicts,
        // so anything that happens here should be safe!
        public int ContextForVariableNamed(string name)
        {
            // Current temporary context?
            // (Shouldn't attempt to access contexts higher in the callstack.)
            if (currentElement.temporaryVariables.ContainsKey (name)) {
                return currentElementIndex+1;
            } 

            // Global
            else {
                return 0;
            }
        }
            
        public Thread ThreadWithIndex(int index)
        {
            return _threads.Find (t => t.threadIndex == index);
        }

        private List<Element> callStack
        {
            get {
                return currentThread.callstack;
            }
        }

        List<Thread> _threads;
        int _threadCounter;
    }
}

