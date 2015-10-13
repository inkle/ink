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
        }

        public List<Element> elements {
            get {
                return _callStack;
            }
        }

        public Element currentElement { 
            get { 
                return _callStack.Last (); 
            } 
        }

        public bool CanPop(PushPop.Type type) {
            return canPop && currentElement.type == type;
        }

        public bool canPop {
            get {
                return _callStack.Count > 1;
            }
        }

        public CallStack ()
        {
            _callStack = new List<Element> ();
            _callStack.Add (new Element (PushPop.Type.Tunnel));
        }

        public void Push(PushPop.Type type)
        {
            // When pushing to callstack, maintain the current content path, but jump out of expressions by default
            _callStack.Add (new Element(type, initialPath: currentElement.path, inExpressionEvaluation: false));
        }

        public void Pop()
        {
            Debug.Assert (canPop);
            if (canPop) {
                _callStack.RemoveAt (_callStack.Count - 1);
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
                var variablePointerContextEl = _callStack [varPointer.resolvedCallstackElementIndex];
                varValue = variablePointerContextEl.variables [varPointer.variableName];
            }

            return varValue;
        }

        // Raw, in that it could be a variable pointer, in which case it doesn't de-reference it
        Runtime.Object GetRawVariableWithName(string name, out int foundInStackElIdx)
        {
            Runtime.Object varValue = null;

            // Search down the scope stack for a variable with this value
            for (int elIdx = _callStack.Count - 1; elIdx >= 0; --elIdx) {
                var element = _callStack [elIdx];

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
                    el = _callStack.First ();
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

            new List<Element> (_callStack);

            // Search down the scope stack for the variable to assign to
            for (int elIdx = _callStack.Count - 1; elIdx >= 0; --elIdx) {
                var element = _callStack [elIdx];

                Runtime.Object existingValue = null;
                if (element.variables.TryGetValue (name, out existingValue)) {

                    // Resolve variable pointer to assign to
                    while (existingValue is LiteralVariablePointer) {
                        var varPointer = (LiteralVariablePointer) existingValue;
                        name = varPointer.variableName;
                        element = _callStack [varPointer.resolvedCallstackElementIndex];
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

        private List<Element> _callStack;
    }
}

