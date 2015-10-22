using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Runtime
{
    internal class CallStack
    {
        internal class Element
        {
            public Path path;
            public bool inExpressionEvaluation;
            public Dictionary<string, Runtime.Object> variables;
            public PushPop.Type type;

            public Element(PushPop.Type type, Path initialPath = null, bool inExpressionEvaluation = false) {
                if( initialPath == null ) {
                    initialPath = Path.ToFirstElement ();
                }

                this.path = initialPath;
                this.inExpressionEvaluation = inExpressionEvaluation;
                this.variables = new Dictionary<string, Object>();
                this.type = type;
            }

            public Element Copy()
            {
                var copy = new Element (this.type, this.path, this.inExpressionEvaluation);
                copy.variables = this.variables;
                return copy;
            }
        }

        internal class Thread
        {
            public List<Element> callstack;

            public Thread() {
                callstack = new List<Element>();
            }

            public Thread(Thread threadToCopy) : this() {
                foreach(var e in threadToCopy.callstack) {
                    callstack.Add(e.Copy());
                }
            }

            public Thread Copy() {
                return new Thread (this);
            }
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

        public Thread currentThread
        {
            get {
                return _allCallStackThreads [_allCallStackThreads.Count - 1];
            }
            set {
                Debug.Assert (_allCallStackThreads.Count == 1, "Shouldn't be directly setting the current thread when we have a stack of them");
                _allCallStackThreads.Clear ();
                _allCallStackThreads.Add (value);
            }
        }

        public bool canPop {
            get {
                return callStack.Count > 1;
            }
        }

        public CallStack ()
        {
            _allCallStackThreads = new List<Thread> ();
            _allCallStackThreads.Add (new Thread ());

            callStack.Add (new Element (PushPop.Type.Tunnel));
        }

        public void PushThread()
        {
            _allCallStackThreads.Add (new Thread (currentThread));
        }

        public void PopThread()
        {
            if (canPopThread) {
                _allCallStackThreads.Remove (currentThread);
            } else {
                Debug.Fail ("Can't pop thread");
            }
        }

        public bool canPopThread
        {
            get {
                return _allCallStackThreads.Count > 1;
            }
        }

        public void Push(PushPop.Type type)
        {
            Debug.Assert (type != PushPop.Type.Paste);

            // When pushing to callstack, maintain the current content path, but jump out of expressions by default
            callStack.Add (new Element(type, initialPath: currentElement.path, inExpressionEvaluation: false));
        }

        public bool CanPop(PushPop.Type? type = null) {

            Debug.Assert (type != PushPop.Type.Paste);

            if (!canPop)
                return false;
            
            if (type == null)
                return true;
            
            return currentElement.type == type;
        }
            
        public void Pop(PushPop.Type? type = null)
        {
            Debug.Assert (type != PushPop.Type.Paste);

            if (CanPop (type)) {
                callStack.RemoveAt (callStack.Count - 1);
                return;
            } else {
                Debug.Fail ("Mismatched push/pop in Callstack");
            }
        }

        // Get variable value, dereferencing a variable pointer if necessary
        public Runtime.Object GetVariableWithName(string name)
        {
            int unusedStackIdx;
            var varValue = GetRawVariableWithName (name, out unusedStackIdx);

            // Get value from pointer?
            var varPointer = varValue as LiteralVariablePointer;
            if (varPointer) {
                var variablePointerContextEl = callStack [varPointer.resolvedCallstackElementIndex];
                varValue = variablePointerContextEl.variables [varPointer.variableName];
            }

            return varValue;
        }

        // Raw, in that it could be a variable pointer, in which case it doesn't de-reference it
        Runtime.Object GetRawVariableWithName(string name, out int foundInStackElIdx)
        {
            Runtime.Object varValue = null;

            // Search down the scope stack for a variable with this value
            for (int elIdx = callStack.Count - 1; elIdx >= 0; --elIdx) {
                var element = callStack [elIdx];

                if (element.variables.TryGetValue (name, out varValue)) {
                    foundInStackElIdx = elIdx;
                    return varValue;
                }
            }

            foundInStackElIdx = -1;
            return null;
        }

        public void SetVariable(string name, Runtime.Object value, bool declareNew, bool prioritiseHigherInCallStack = false)
        {
            if (declareNew) {
                Element el;
                if (prioritiseHigherInCallStack) {
                    el = callStack.First ();
                } else {
                    el = currentElement;
                }

                if (declareNew && value is LiteralVariablePointer) {
                    var varPointer = value as LiteralVariablePointer;
                    if (varPointer) {
                        value = ResolveVariablePointer (varPointer);
                    }
                }

                el.variables [name] = value;
                return;
            }

            // Search down the scope stack for the variable to assign to
            for (int elIdx = callStack.Count - 1; elIdx >= 0; --elIdx) {
                var element = callStack [elIdx];

                Runtime.Object existingValue = null;
                if (element.variables.TryGetValue (name, out existingValue)) {

                    // Resolve variable pointer to assign to
                    while (existingValue is LiteralVariablePointer) {
                        var varPointer = (LiteralVariablePointer) existingValue;
                        name = varPointer.variableName;
                        element = callStack [varPointer.resolvedCallstackElementIndex];
                        existingValue = element.variables [name];
                    }

                    element.variables [name] = value;

                    return;
                }

            }

            throw new StoryException ("Could not find variable to set: " + name);
        }

        // Takes:   variable pointer where only the name of the variable is known
        // Returns: variable pointer with additional information to pinpoint the
        //          precise instance of a variable with that name on the callstack
        LiteralVariablePointer ResolveVariablePointer(LiteralVariablePointer varPointer)
        {
            int stackIdx;
            var varValue = GetRawVariableWithName (varPointer.variableName, out stackIdx);

            // Extra layer of indirection:
            // When accessing a pointer to a pointer (e.g. when calling nested or 
            // recursive functions that take a variable references, ensure we don't create
            // a chain of indirection by just returning the final target.
            var existingPointer = varValue as LiteralVariablePointer;
            if (existingPointer && existingPointer.resolvedCallstackElementIndex >= 0) {
                return existingPointer;
            }

            // Return clone so we're not attempting to modify the source runtime
            var clone = new LiteralVariablePointer (varPointer.variableName);
            clone.resolvedCallstackElementIndex = stackIdx;
            return clone;
        }

        private List<Element> callStack
        {
            get {
                return currentThread.callstack;
            }
        }

        private List<Thread> _allCallStackThreads;
    }
}

