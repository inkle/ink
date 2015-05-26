using System;
using System.Collections.Generic;
using Inklewriter;

namespace Inklewriter.Parsed
{
	public abstract class Expression : Parsed.Object
	{
        public bool outputWhenComplete { get; set; }

		public override Runtime.Object GenerateRuntimeObject ()
		{
            var container = new Runtime.Container ();

            // Tell Runtime to start evaluating the following content as an expression
            container.AddContent (Runtime.EvaluationCommand.Start());

			GenerateIntoContainer (container);

            // Tell Runtime to output the result of the expression evaluation to the output stream
            if (outputWhenComplete) {
                container.AddContent (Runtime.EvaluationCommand.Output());
            }

            // Tell Runtime to stop evaluating the content as an expression
            container.AddContent (Runtime.EvaluationCommand.End());

            return container;
		}

        public abstract void GenerateIntoContainer (Runtime.Container container);

	}

	public class BinaryExpression : Expression
	{
		public Expression leftExpression;
		public Expression rightExpression;
		public string opName;

		public BinaryExpression(Expression left, Expression right, string opName)
		{
			left.parent = this;
			right.parent = this;

			leftExpression = left;
			rightExpression = right;
			this.opName = opName;
		}

        public override void GenerateIntoContainer(Runtime.Container container)
		{
			leftExpression.GenerateIntoContainer (container);
			rightExpression.GenerateIntoContainer (container);
            container.AddContent(Runtime.NativeFunctionCall.CallWithName(opName));
		}

        public override void ResolveReferences (Story context)
        {
            leftExpression.ResolveReferences (context);
            rightExpression.ResolveReferences (context);
        }
	}

	public class NegatedExpression : Expression
	{
		public Expression innerExpression;

		public NegatedExpression(Expression inner)
		{
			inner.parent = this;
			innerExpression = inner;
		}

        public override void GenerateIntoContainer(Runtime.Container container)
		{
			innerExpression.GenerateIntoContainer (container);
            container.AddContent(Runtime.NativeFunctionCall.CallWithName(Runtime.NativeFunctionCall.Negate)); // "~"
		}

        public override void ResolveReferences (Story context)
        {
            innerExpression.ResolveReferences (context);
        }
	}
}

