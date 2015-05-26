using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class FunctionCall : Expression
    {
        public string name { get; protected set; }
        public List<Expression> arguments { get; protected set; }
        public Runtime.Divert runtimeDivert { get; protected set; }

        public FunctionCall (string functionName, List<Expression> arguments)
        {
            this.name = functionName;
            this.arguments = arguments;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            runtimeDivert = new Runtime.Divert ();

            // TODO: Pass arguments
            container.AddContent (new Runtime.StackPush ());
            container.AddContent (runtimeDivert);
        }

        public override void ResolveReferences (Story context)
        {
            Path ambiguousPath = Path.To (name);
            var targetObject = ResolvePath (ambiguousPath);

            if (targetObject == null) {
                context.Error ("Function (knot) not found: '" + name + "'", this);
            } else {
                runtimeDivert.targetPath = targetObject.runtimeObject.path;
            }
        }
    }
}

