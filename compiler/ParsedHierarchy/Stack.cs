using System.Collections.Generic;

namespace Ink.Parsed
{
    public class Stack : Parsed.Expression
    {
        public List<Expression> contents;

        public Stack (List<Expression> expressions)
        {
            this.contents = expressions ?? new List<Expression>();
        }

        public override void ResolveReferences(Story context)
        {
            base.ResolveReferences(context);
            foreach(var content in contents) {
                content.ResolveReferences(context);
            }
        }

        // Only known after GenerateIntoContainer has run
        public bool isValidGlobalStackLiteral;

        public override void GenerateIntoContainer(Runtime.Container container)
        {
            container.AddContent(GenerateRuntimeObject());
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container();
            // Assume true until we find a counter
            isValidGlobalStackLiteral = true;

            if (contents != null) {
                foreach (var valueExpression in contents) {
                    valueExpression.parent = this;
                    valueExpression.GenerateIntoContainer(container);
                    var variableReference = valueExpression as VariableReference;
                    if(variableReference && !variableReference.isConstantReference && !variableReference.isListItemReference) {
                        isValidGlobalStackLiteral = false;
                    }
                }
            }

            var count = contents == null ? 0 : contents.Count;
            container.AddContent(new Runtime.IntValue(count));

            container.AddContent(Runtime.ControlCommand.StackLiteralEnd());
            return container;
        }
    }
}
