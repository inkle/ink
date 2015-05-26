using System;

namespace Inklewriter.Parsed
{
    public class Return : Parsed.Object
    {
        public Expression returnedExpression { get; protected set; }

        public Return (Expression returnedExpression)
        {
            this.returnedExpression = returnedExpression;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // Evaluate expression
            if (returnedExpression != null) {
                container.AddContent (returnedExpression.runtimeObject);
            } 

            // Return Runtime.Void when there's no expression to evaluate
            // (This evaluation will just add the Void object to the evaluation stack)
            else {
                container.AddContent (Runtime.EvaluationCommand.Start ());
                container.AddContent (new Runtime.Void());
                container.AddContent (Runtime.EvaluationCommand.End ());
            }

            // Then pop the call stack
            // (the evaluated expression will leave the return value on the evaluation stack)
            container.AddContent (new Runtime.StackPop ()); 

            return container;
        }
    }
}

