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
                return copy;
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
        public Runtime.Object GetTemporaryVariableWithName(string name)
        {
            int unusedStackIdx;
            var varValue = GetRawTemporaryVariableWithName (name, out unusedStackIdx);

            // Get value from pointer?
            var varPointer = varValue as LiteralVariablePointer;
            if (varPointer) {
                var variablePointerContextEl = callStack [varPointer.resolvedCallstackElementIndex];
                varValue = variablePointerContextEl.temporaryVariables [varPointer.variableName];
            }

            return varValue;
        }

        // Raw, in that it could be a variable pointer, in which case it doesn't de-reference it
        Runtime.Object GetRawTemporaryVariableWithName(string name, out int foundInStackElIdx)
        {
            Runtime.Object varValue = null;

            // Search down the scope stack for a variable with this value
            for (int elIdx = callStack.Count - 1; elIdx >= 0; --elIdx) {
                var element = callStack [elIdx];

                if (element.temporaryVariables.TryGetValue (name, out varValue)) {
                    foundInStackElIdx = elIdx;
                    return varValue;
                }
            }

            foundInStackElIdx = -1;
            return null;
        }

        public void SetTemporaryVariable(string name, Runtime.Object value, bool declareNew, bool prioritiseHigherInCallStack = false)
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
                        value = ResolveTemporaryVariablePointer (varPointer);
                    }
                }

                el.temporaryVariables [name] = value;
                return;
            }

            // Search down the scope stack for the variable to assign to
            for (int elIdx = callStack.Count - 1; elIdx >= 0; --elIdx) {
                var element = callStack [elIdx];

                Runtime.Object existingValue = null;
                if (element.temporaryVariables.TryGetValue (name, out existingValue)) {

                    // Resolve variable pointer to assign to
                    while (existingValue is LiteralVariablePointer) {
                        var varPointer = (LiteralVariablePointer) existingValue;
                        name = varPointer.variableName;
                        element = callStack [varPointer.resolvedCallstackElementIndex];
                        existingValue = element.temporaryVariables [name];
                    }

                    element.temporaryVariables [name] = value;

                    return;
                }

            }

            throw new StoryException ("Could not find variable to set: " + name);
        }

        // Takes:   variable pointer where only the name of the variable is known
        // Returns: variable pointer with additional information to pinpoint the
        //          precise instance of a variable with that name on the callstack
        LiteralVariablePointer ResolveTemporaryVariablePointer(LiteralVariablePointer varPointer)
        {
            int stackIdx;
            var varValue = GetRawTemporaryVariableWithName (varPointer.variableName, out stackIdx);

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

        private List<Thread> _threads;
    }
}

