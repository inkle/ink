using System.Collections.Generic;

namespace Ink.Parsed
{
    internal class VariableReference : Expression
    {
        public string name { 
            get {
                if (path != null && path.Count == 1)
                    return path [0];
                else
                    return null;
            } 
        }
        
        public List<string> path;

        // Only known after GenerateIntoContainer has run
        public bool isConstantReference;

        public Runtime.VariableReference runtimeVarRef { get { return _runtimeVarRef; } }

        public VariableReference (List<string> path)
        {
            this.path = path;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            Expression constantValue = null;

            // Name can be null if it's actually a path to a knot/stitch etc for a read count
            var varName = name;

            // If it's a constant reference, just generate the literal expression value
            if ( varName != null && story.constants.TryGetValue (varName, out constantValue) ) {
                constantValue.GenerateIntoContainer (container);
                isConstantReference = true;
            } else {
                _runtimeVarRef = new Runtime.VariableReference (name);
                container.AddContent(_runtimeVarRef);
            }
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            // Work is already done if it's a constant reference
            if (isConstantReference) {
                return;
            }
                
            // Is it a read count?
            var parsedPath = new Path (path);
            Parsed.Object targetForCount = parsedPath.ResolveFromContext (this);
            if (targetForCount) {

                targetForCount.containerForCounting.visitsShouldBeCounted = true;

                _runtimeVarRef.pathForCount = targetForCount.runtimePath;
                _runtimeVarRef.name = null;

                // Check for very specific writer error: getting read count and
                // printing it as content rather than as a piece of logic
                // e.g. Writing {myFunc} instead of {myFunc()}
                var targetFlow = targetForCount as FlowBase;
                if (targetFlow && targetFlow.isFunction) {

                    // Is parent context content rather than logic?
                    if ( parent is Weave || parent is ContentList || parent is FlowBase) {
                        Warning ("'" + targetFlow.name + "' being used as read count rather than being called as function. Perhaps you intended to write " + targetFlow.name + "()");
                    }
                }

                return;
            } 

            // Definitely a read count, but wasn't found?
            else if (path.Count > 1) {
                Error ("Could not find target for read count: " + parsedPath);
                return;
            }

            if (!context.ResolveVariableWithName (this.name, fromNode: this).found) {
                Error("Unresolved variable: "+this.ToString(), this);
            }
        }

        public override string ToString ()
        {
            return string.Join(".", path);
        }

        Runtime.VariableReference _runtimeVarRef;
    }
}

