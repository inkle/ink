﻿using System.Collections.Generic;

namespace Ink.Parsed
{
    internal class VariableReference : Expression
    {
        // - Normal variables have a single item in their "path"
        // - Knot/stitch names for read counts are actual dot-separated paths
        //   (though this isn't actually used at time of writing)
        // - List names are dot separated: listName.itemName (or just itemName)
        public string name { 
            get {
                return string.Join (".", path.ToArray());
            } 
        }
        
        public List<string> path;

        // Only known after GenerateIntoContainer has run
        public bool isConstantReference;
        public bool isListItemReference;

        public Runtime.VariableReference runtimeVarRef { get { return _runtimeVarRef; } }

        public VariableReference (List<string> path)
        {
            this.path = path;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            Expression constantValue = null;

            // If it's a constant reference, just generate the literal expression value
            // It's okay to access the constants at code generation time, since the
            // first thing the ExportRuntime function does it search for all the constants
            // in the story hierarchy, so they're all available.
            if ( story.constants.TryGetValue (name, out constantValue) ) {
                constantValue.GenerateConstantIntoContainer (container);
                isConstantReference = true;
                return;
            }

            _runtimeVarRef = new Runtime.VariableReference (name);

            // List item reference?
            // Path might be to a list (listName.listItemName or just listItemName)
            if (path.Count == 1 || path.Count == 2) {
                string listItemName = null;
                string listName = null;

                if (path.Count == 1) listItemName = path [0];
                else {
                    listName = path [0];
                    listItemName = path [1];
                }

                var listItem = story.ResolveListItem (listName, listItemName, this);
                if (listItem) {
                    isListItemReference = true;
                }
            }

            container.AddContent (_runtimeVarRef);
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            // Work is already done if it's a constant or list item reference
            if (isConstantReference || isListItemReference) {
                return;
            }
                
            // Is it a read count?
            var parsedPath = new Path (path);
            Parsed.Object targetForCount = parsedPath.ResolveFromContext (this);
            if (targetForCount) {

                targetForCount.containerForCounting.visitsShouldBeCounted = true;

                // If this is an argument to a function that wants a variable to be
                // passed by reference, then the Parsed.Divert will have generated a
                // Runtime.VariablePointerValue instead of allowing this object
                // to generate its RuntimeVariableReference. This only happens under
                // error condition since we shouldn't be passing a read count by
                // reference, but we don't want it to crash!
                if (_runtimeVarRef == null) return;

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

            // Couldn't find this multi-part path at all, whether as a divert
            // target or as a list item reference.
            if (path.Count > 1) {
                var errorMsg = "Could not find target for read count: " + parsedPath;
                if (path.Count <= 2)
                    errorMsg += ", or couldn't find list item with the name " + string.Join (",", path.ToArray());
                Error (errorMsg);
                return;
            }

            if (!context.ResolveVariableWithName (this.name, fromNode: this).found) {
                Error("Unresolved variable: "+this.ToString(), this);
            }
        }

        public override string ToString ()
        {
            return string.Join(".", path.ToArray());
        }

        Runtime.VariableReference _runtimeVarRef;
    }
}

