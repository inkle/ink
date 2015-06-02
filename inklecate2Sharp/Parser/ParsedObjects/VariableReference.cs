using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class VariableReference : Expression
    {
        public string name { get; protected set; }

        public VariableReference (string name)
        {
            this.name = name;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            _runtimeVarRef = new Runtime.VariableReference (name);
            container.AddContent(_runtimeVarRef);
        }

        public override void ResolveReferences (Story context)
        {
            bool resolved = false;

            List<string> searchedLocations = new List<string> ();

            var ancestor = this.parent;
            while (ancestor != null) {

                if (ancestor is FlowBase) {
                    var ancestorFlow = (FlowBase)ancestor;
                    if (ancestorFlow.name != null) {
                        searchedLocations.Add ("'"+ancestorFlow.name+"'");
                    }
                    if( ancestorFlow.HasVariableWithName(this.name, true, false) ) {
                        resolved = true;
                        break;
                    }
                    var content = ancestorFlow.ContentWithNameAtLevel (this.name);
                    if (content != null) {
                        var namedContent = (FlowBase)content;

                        // Update the name using the *full* name of the content
                        this.name = namedContent.dotSeparatedFullName;
                        _runtimeVarRef.name = this.name;

                        resolved = true;
                        break;
                    }
                }

                ancestor = ancestor.parent;
            }

            if ( !resolved ) {
                var locationsStr = "";
                if (searchedLocations.Count > 0) {
                    var locationsListStr = string.Join (", ", searchedLocations);
                    locationsStr = " in " + locationsListStr + " or globally";
                }
                string.Join (", ", searchedLocations);
                context.Error ("variable '" + this.name + "' not found"+locationsStr, this);
                ResolveReferences (context);
            }
        }

        Runtime.VariableReference _runtimeVarRef;
    }
}

