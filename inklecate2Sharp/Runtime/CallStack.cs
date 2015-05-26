using System;
using System.Collections.Generic;
using System.Linq;

namespace Inklewriter.Runtime
{
    public class CallStack
    {
        public class Element
        {
            public Path path;
            public bool inExpressionEvaluation;

            public Element(Path initialPath = null, bool inExpressionEvaluation = false) {
                if( initialPath == null ) {
                    initialPath = Path.ToFirstElement ();
                }

                this.path = initialPath;
                this.inExpressionEvaluation = inExpressionEvaluation;
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
            _callStack.RemoveAt (_callStack.Count - 1);
        }

        private List<Element> _callStack;
    }
}

