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
            if ( !context.variableDeclarations.ContainsKey (this.name) ) {
                context.Error ("variable not found: " + this.name, this);
            }
        }
    }
}

