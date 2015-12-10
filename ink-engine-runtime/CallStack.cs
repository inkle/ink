using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Ink.Runtime
{
    internal class CallStack
    {
        internal class Element
        {
            public Path path;
            public bool inExpressionEvaluation;
            public Dictionary<string, Runtime.Object> temporaryVariables;
            public PushPop.Type type;

            public Element(PushPop.Type type, Path initialPath = null, bool inExpressionEvaluation = false) {
                if( initialPath == null ) {
                    initialPath = Path.ToFirstElement ();
                }

                this.path = initialPath;
                this.inExpressionEvaluation = inExpressionEvaluation;
                this.temporaryVariables = new Dictionary<string, Object>();
                this.type = type;
            }

            public Element Copy()
            {
                var copy = new Element (this.type, this.path, this.inExpressionEvaluation);
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
                return callStack.Last (); 
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

        public CallStack ()
        {
            _threads = new List<Thread> ();
            _threads.Add (new Thread ());

            _threads [0].callstack.Add (new Element (PushPop.Type.Tunnel));
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

        public void Push(PushPop.Type type)
        {
            // When pushing to callstack, maintain the current content path, but jump out of expressions by default
            callStack.Add (new Element(type, initialPath: currentElement.path, inExpressionEvaluation: false));
        }

        public bool CanPop(PushPop.Type? type = null) {

            if (!canPop)
                return false;
            
            if (type == null)
                return true;
            
            return currentElement.type == type;
        }
            
        public void Pop(PushPop.Type? type = null)
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

        private List<Element> callStack
        {
            get {
                return currentThread.callstack;
            }
        }

        private List<Thread> _threads;
    }
}

