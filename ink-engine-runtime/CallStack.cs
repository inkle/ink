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
                        return;
                    }

                    currentContainer = currentObj.parent as Container;

                    // Two reasons why the above operation might not work:
                    //  - currentObj is already the root container
                    //  - currentObj is a named container rather than being an object at an index
                    if (currentContainer != null)
                        currentContentIndex = currentContainer.content.IndexOf (currentObj);

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

            public Thread() {
                callstack = new List<Element>();
            }

            public Thread Copy() {
                var copy = new Thread ();
                foreach(var e in callstack) {
                    copy.callstack.Add(e.Copy());
                }
                if (_openContainers != null) {
                    copy._openContainers = new HashSet<Container> (_openContainers);
                }
                return copy;
            }

            public void ResetOpenContainers()
            {
                _openContainers = null;
            }

            // Story tells CallStack when the set of containers changes for this thread.
            // CallStack passes back which ones are new, for incrementing of read and turn counts.
            public HashSet<Container> UpdateOpenContainers(HashSet<Container> openContainers)
            {
                var newlyOpenContainers = new HashSet<Container> (openContainers);
                if (_openContainers != null) {
                    foreach (var c in _openContainers) {
                        newlyOpenContainers.Remove (c);
                    }
                }

                _openContainers = openContainers;

                return newlyOpenContainers;
            }

            // For tracking of read counts and turn counts:
            // Keep track of which containers (runtime equivalent of knots and stitches)
            // are "open" right now - which containers is the runtime currently inside.
            // e.g. can be currently inside a stitch, within a knot.
            // As these change, the story increments a counter for them.
            HashSet<Container> _openContainers;
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

        public void PushThread()
        {
            _threads.Add (currentThread.Copy());
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

        private List<Element> callStack
        {
            get {
                return currentThread.callstack;
            }
        }

        private List<Thread> _threads;
    }
}

