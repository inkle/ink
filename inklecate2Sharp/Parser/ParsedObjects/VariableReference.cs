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
            container.AddContent(new Runtime.VariableReference(name));
        }

        public override void ResolveReferences (Story context)
        {
            bool resolved = false;

            var ancestor = this.parent;
            while (ancestor != null) {

                if (ancestor is FlowBase) {
                    var ancestorFlow = (FlowBase)ancestor;
                    if( ancestorFlow.HasVariableWithName(this.name) ) {
                        resolved = true;
                        break;
                    }
                }

                ancestor = ancestor.parent;
            }

            if ( !resolved ) {
                context.Error ("variable not found: " + this.name, this);
            }
        }
    }
}

