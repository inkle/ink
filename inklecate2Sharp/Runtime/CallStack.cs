using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Runtime
{
    public class CallStack
    {
        public class Element
        {
            public Path path;
            public bool inExpressionEvaluation;
            public Dictionary<string, Runtime.Object> variables;

            public Element(Path initialPath = null, bool inExpressionEvaluation = false) {
                if( initialPath == null ) {
                    initialPath = Path.ToFirstElement ();
                }

                this.path = initialPath;
                this.inExpressionEvaluation = inExpressionEvaluation;
                this.variables = new Dictionary<string, Object>();
            }
        }

        public Element currentElement { 
            get { 
                return _callStack.Last (); 
            } 
        }

        public bool canPop {
            get {
                return _callStack.Count > 1;
            }
        }

        public CallStack ()
        {
            _callStack = new List<Element> ();
            _callStack.Add (new Element ());
        }

        public void Push()
        {
            // When pushing to callstack, maintain the current content path, but jump out of expressions by default
            _callStack.Add (new Element(initialPath: currentElement.path, inExpressionEvaluation: false));
        }

        public void Pop()
        {
            Debug.Assert (canPop);
            _callStack.RemoveAt (_callStack.Count - 1);
        }

        public Runtime.Object GetVariableWithName(string name)
        {
            Runtime.Object varValue = null;

            // Search down the scope stack for a variable with this value
            for (int elIdx = _callStack.Count - 1; elIdx >= 0; --elIdx) {
                var element = _callStack [elIdx];

                if (element.variables.TryGetValue (name, out varValue)) {
                    return varValue;
                }
            }

            return null;
        }

        private List<Element> _callStack;
    }
}

