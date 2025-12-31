using System.Collections.Generic;

namespace Ink.Parsed
{
    public class Stack : Parsed.Expression
    {
        public List<Expression> contents;

        public Stack (List<Expression> expressions)
        {
            this.contents = expressions;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            if (contents != null) {
                foreach (var valueExpression in contents) {
                    valueExpression.GenerateIntoContainer(container);
                }
            }

            var count = contents == null ? 0 : contents.Count;
            container.AddContent(new Runtime.IntValue(count));

            container.AddContent(Runtime.ControlCommand.StackLiteralEnd());
        }
    }
}
