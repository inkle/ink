using System.Collections.Generic;

namespace Ink.Parsed
{
    internal class VariableReference : Expression
    {
        // - Normal variables have a single item in their "path"
        // - Knot/stitch names for read counts are actual dot-separated paths
        //   (though this isn't actually used at time of writing)
        // - Set names are dot separated: setName.itemName (or just itemName)
        public string name { 
            get {
                return string.Join (".", path);
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

            // If it's a constant reference, just generate the literal expression value
            // TODO: Write a comment about why it's okay to access the constants at
            // code generation time (it is, right?)
            if ( story.constants.TryGetValue (name, out constantValue) ) {
                constantValue.GenerateConstantIntoContainer (container);
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

            // Path might be to a set (setName.setItemName or just setItemName)
            if (path.Count == 1 || path.Count == 2) {
                string setItemName = null;
                string setName = null;

                if (path.Count == 1) setItemName = path [0];
                else {
                    setName = path [0];
                    setItemName = path [1];
                }

                var setItem = story.ResolveSetItem (setName, setItemName, this);
                if (setItem) {
                    _runtimeVarRef.name = name;
                    return;
                }
            }

            // Couldn't find this multi-part path at all?
            if (path.Count > 1) {
                var errorMsg = "Could not find target for read count: " + parsedPath;
                if (path.Count <= 2)
                    errorMsg += ", or couldn't find set item with the name " + string.Join (",", path);
                Error (errorMsg);
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

