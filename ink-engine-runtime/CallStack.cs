using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

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
                copy.temporaryVariables = this.temporaryVariables;
                return copy;
            }
        }

        internal class Thread
        {
            public List<Element> callstack;
            public int threadIndex;

            public Thread() {
                callstack = new List<Element>();
            }

			public Thread(JToken jsonToken, Story storyContext) : this() {
				JObject jThreadObj = (JObject) jsonToken;
				threadIndex = jThreadObj ["threadIndex"].ToObject<int> ();

				JArray jThreadCallstack = (JArray) jThreadObj ["callstack"];
				foreach (JToken jElTok in jThreadCallstack) {

					JObject jElementObj = (JObject)jElTok;

					PushPopType pushPopType = (PushPopType) jElementObj ["type"].ToObject<int>();

					Container currentContainer = null;
					int contentIndex = 0;

					string currentContainerPathStr = null;
					JToken currentContainerPathStrToken;
					if (jElementObj.TryGetValue ("cPath", out currentContainerPathStrToken)) {
						currentContainerPathStr = currentContainerPathStrToken.ToString ();
						currentContainer = storyContext.ContentAtPath (new Path(currentContainerPathStr)) as Container;
						contentIndex = jElementObj ["idx"].ToObject<int> ();
					}

					bool inExpressionEvaluation = jElementObj ["exp"].ToObject<bool> ();

					var el = new Element (pushPopType, currentContainer, contentIndex, inExpressionEvaluation);

					var jObjTemps = (JObject) jElementObj ["temp"];
					el.temporaryVariables = Json.JObjectToDictionaryRuntimeObjs (jObjTemps);

					callstack.Add (el);
				}
			}

            public Thread Copy() {
                var copy = new Thread ();
                copy.threadIndex = threadIndex;
                foreach(var e in callstack) {
                    copy.callstack.Add(e.Copy());
                }
                return copy;
            }

			public JToken jsonToken {
				get {
					var threadJObj = new JObject ();

					var jThreadCallstack = new JArray ();
					foreach (CallStack.Element el in callstack) {
						var jObj = new JObject ();
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

					return threadJObj;
				}
			}
        }

        public List<Element> elements {
            get {
                return callStack;
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
        public void SetJsonToken(JToken token, Story storyContext)
        {
            _threads.Clear ();

            var jObject = (JObject)token;

            var jThreads = (JArray) jObject ["threads"];

            foreach (JToken jThreadTok in jThreads) {

                var thread = new Thread (jThreadTok, storyContext);
                _threads.Add (thread);
            }

            _threadCounter = jObject ["threadCounter"].ToObject<int> ();
        }
            
        // See above for why we can't implement jsonToken
        public JToken GetJsonToken() {

            var jObject = new JObject ();

            var jThreads = new JArray ();
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
            newThread.threadIndex = _threadCounter;
            _threadCounter++;
            _threads.Add (newThread);
        }

        public void PopThread()
        {
            if (canPopThread) {
                _threads.Remove (currentThread);
            } else {
                Debug.Fail ("Can't pop thread");
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
                Debug.Fail ("Mismatched push/pop in Callstack");
            }
        }

        // Get variable value, dereferencing a variable pointer if necessary
        public Runtime.Object GetTemporaryVariableWithName(string name, int contextIndex = -1)
        {
            if (contextIndex == -1)
                contextIndex = currentElementIndex;
            
            Runtime.Object varValue = null;

            var contextElement = callStack [contextIndex];

            if (contextElement.temporaryVariables.TryGetValue (name, out varValue)) {
                return varValue;
            } else {
                return null;
            }
        }
            
        public void SetTemporaryVariable(string name, Runtime.Object value, bool declareNew, int contextIndex = -1)
        {
            if (contextIndex == -1)
                contextIndex = currentElementIndex;

            var contextElement = callStack [contextIndex];
            
            if (!declareNew && !contextElement.temporaryVariables.ContainsKey(name)) {
                throw new StoryException ("Could not find temporary variable to set: " + name);
            }

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
                return currentElementIndex;
            } 

            // Global
            else {
                return -1;
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

        private List<Thread> _threads;
        public int _threadCounter;
    }
}

