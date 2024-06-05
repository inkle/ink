using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Ink.Runtime
{
    public class CallStack
    {
        public class Element
        {
            public Pointer currentPointer;

            public bool inExpressionEvaluation;
            public Dictionary<string, Runtime.Object> temporaryVariables;
            public PushPopType type;

            // When this callstack element is actually a function evaluation called from the game,
            // we need to keep track of the size of the evaluation stack when it was called
            // so that we know whether there was any return value.
            public int evaluationStackHeightWhenPushed;

            // When functions are called, we trim whitespace from the start and end of what
            // they generate, so we make sure know where the function's start and end are.
            public int functionStartInOuputStream;

            public Element(PushPopType type, Pointer pointer, bool inExpressionEvaluation = false) {
                this.currentPointer = pointer;
                this.inExpressionEvaluation = inExpressionEvaluation;
                this.temporaryVariables = new Dictionary<string, Object>();
                this.type = type;
            }

            public Element Copy()
            {
                var copy = new Element (this.type, currentPointer, this.inExpressionEvaluation);
                copy.temporaryVariables = new Dictionary<string,Object>(this.temporaryVariables);
                copy.evaluationStackHeightWhenPushed = evaluationStackHeightWhenPushed;
                copy.functionStartInOuputStream = functionStartInOuputStream;
                return copy;
            }
        }

        public class Thread
        {
            public List<Element> callstack;
            public int threadIndex;
            public Pointer previousPointer;

            public Thread() {
                callstack = new List<Element>();
            }

			public Thread(Dictionary<string, object> jThreadObj, Story storyContext) : this() {
                threadIndex = (int) jThreadObj ["threadIndex"];

				List<object> jThreadCallstack = (List<object>) jThreadObj ["callstack"];
				foreach (object jElTok in jThreadCallstack) {

					var jElementObj = (Dictionary<string, object>)jElTok;

                    PushPopType pushPopType = (PushPopType)(int)jElementObj ["type"];

                    Pointer pointer = Pointer.Null;

					string currentContainerPathStr = null;
					object currentContainerPathStrToken;
					if (jElementObj.TryGetValue ("cPath", out currentContainerPathStrToken)) {
						currentContainerPathStr = currentContainerPathStrToken.ToString ();

                        var threadPointerResult = storyContext.ContentAtPath (new Path (currentContainerPathStr));
                        pointer.container = threadPointerResult.container;
                        pointer.index = (int)jElementObj ["idx"];

                        if (threadPointerResult.obj == null) {
                            throw new System.Exception ("When loading state, internal story location couldn't be found: " + currentContainerPathStr + ". Has the story changed since this save data was created?");
                        } else if (threadPointerResult.approximate) {
                            if (pointer.container != null) {
                                storyContext.Warning ("When loading state, exact internal story location couldn't be found: '" + currentContainerPathStr + "', so it was approximated to '" + pointer.container.path.ToString() + "' to recover. Has the story changed since this save data was created?");
                            } else {
                                storyContext.Warning ("When loading state, exact internal story location couldn't be found: '" + currentContainerPathStr + "' and it may not be recoverable. Has the story changed since this save data was created?");
                            }
                        }
					}

                    bool inExpressionEvaluation = (bool)jElementObj ["exp"];

					var el = new Element (pushPopType, pointer, inExpressionEvaluation);

                    object temps;
                    if ( jElementObj.TryGetValue("temp", out temps) ) {
                        el.temporaryVariables = Json.JObjectToDictionaryRuntimeObjs((Dictionary<string, object>)temps);
                    } else {
                        el.temporaryVariables.Clear();
                    }					

					callstack.Add (el);
				}

				object prevContentObjPath;
				if( jThreadObj.TryGetValue("previousContentObject", out prevContentObjPath) ) {
					var prevPath = new Path((string)prevContentObjPath);
                    previousPointer = storyContext.PointerAtPath(prevPath);
                }
			}

            public Thread Copy() {
                var copy = new Thread ();
                copy.threadIndex = threadIndex;
                foreach(var e in callstack) {
                    copy.callstack.Add(e.Copy());
                }
                copy.previousPointer = previousPointer;
                return copy;
            }

            public void WriteJson(SimpleJson.Writer writer)
            {
                writer.WriteObjectStart();

                // callstack
                writer.WritePropertyStart("callstack");
                writer.WriteArrayStart();
                foreach (CallStack.Element el in callstack)
                {
                    writer.WriteObjectStart();
                    if(!el.currentPointer.isNull) {
                        writer.WriteProperty("cPath", el.currentPointer.container.path.componentsString);
                        writer.WriteProperty("idx", el.currentPointer.index);
                    }

                    writer.WriteProperty("exp", el.inExpressionEvaluation);
                    writer.WriteProperty("type", (int)el.type);

                    if(el.temporaryVariables.Count > 0) {
                        writer.WritePropertyStart("temp");
                        Json.WriteDictionaryRuntimeObjs(writer, el.temporaryVariables);
                        writer.WritePropertyEnd();
                    }

                    writer.WriteObjectEnd();
                }
                writer.WriteArrayEnd();
                writer.WritePropertyEnd();

                // threadIndex
                writer.WriteProperty("threadIndex", threadIndex);

                if (!previousPointer.isNull)
                {
                    writer.WriteProperty("previousContentObject", previousPointer.Resolve().path.ToString());
                }

                writer.WriteObjectEnd();
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
                var thread = _threads [_threads.Count - 1];
                var cs = thread.callstack;
                return cs [cs.Count - 1];
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

        public CallStack (Story storyContext)
        {
            _startOfRoot = Pointer.StartOf(storyContext.rootContentContainer);
            Reset();
        }


        public CallStack(CallStack toCopy)
        {
            _threads = new List<Thread> ();
            foreach (var otherThread in toCopy._threads) {
                _threads.Add (otherThread.Copy ());
            }
            _threadCounter = toCopy._threadCounter;
            _startOfRoot = toCopy._startOfRoot;
        }

        public void Reset() 
        {
            _threads = new List<Thread>();
            _threads.Add(new Thread());

            _threads[0].callstack.Add(new Element(PushPopType.Tunnel, _startOfRoot));
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
            _startOfRoot = Pointer.StartOf(storyContext.rootContentContainer);
        }

        public void WriteJson(SimpleJson.Writer w)
        {
            w.WriteObject(writer =>
            {
                writer.WritePropertyStart("threads");
                {
                    writer.WriteArrayStart();

                    foreach (CallStack.Thread thread in _threads)
                    {
                        thread.WriteJson(writer);
                    }

                    writer.WriteArrayEnd();
                }
                writer.WritePropertyEnd();

                writer.WritePropertyStart("threadCounter");
                {
                    writer.Write(_threadCounter);
                }
                writer.WritePropertyEnd();
            });
        
        }

        public void PushThread()
        {
            var newThread = currentThread.Copy ();
            _threadCounter++;
            newThread.threadIndex = _threadCounter;
            _threads.Add (newThread);
        }

        public Thread ForkThread()
        {
            var forkedThread = currentThread.Copy();
            _threadCounter++;
            forkedThread.threadIndex = _threadCounter;
            return forkedThread;
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
				return _threads.Count > 1 && !elementIsEvaluateFromGame;
            }
        }

		public bool elementIsEvaluateFromGame
		{
			get {
				return currentElement.type == PushPopType.FunctionEvaluationFromGame;
			}
		}

        public void Push(PushPopType type, int externalEvaluationStackHeight = 0, int outputStreamLengthWithPushed = 0)
        {
            // When pushing to callstack, maintain the current content path, but jump out of expressions by default
            var element = new Element (
                type, 
                currentElement.currentPointer,
                inExpressionEvaluation: false
            );

            element.evaluationStackHeightWhenPushed = externalEvaluationStackHeight;
            element.functionStartInOuputStream = outputStreamLengthWithPushed;

            callStack.Add (element);
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
            // contextIndex 0 means global, so index is actually 1-based
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
                throw new System.Exception ("Could not find temporary variable to set: " + name);
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

		public string callStackTrace {
			get {
				var sb = new System.Text.StringBuilder();

				for(int t=0; t<_threads.Count; t++) {

					var thread = _threads[t];
					var isCurrent = (t == _threads.Count-1);
					sb.AppendFormat("=== THREAD {0}/{1} {2}===\n", (t+1), _threads.Count, (isCurrent ? "(current) ":""));

					for(int i=0; i<thread.callstack.Count; i++) {

						if( thread.callstack[i].type == PushPopType.Function )
							sb.Append("  [FUNCTION] ");
						else
							sb.Append("  [TUNNEL] ");

						var pointer = thread.callstack[i].currentPointer;
						if( !pointer.isNull ) {
							sb.Append("<SOMEWHERE IN ");
							sb.Append(pointer.container.path.ToString());
							sb.AppendLine(">");
						}
					}
				}


				return sb.ToString();
			}
		}

        List<Thread> _threads;
        int _threadCounter;
        Pointer _startOfRoot;
    }
}

