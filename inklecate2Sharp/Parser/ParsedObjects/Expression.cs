using System;
using System.Collections.Generic;
using Inklewriter;

namespace Inklewriter.Parsed
{
	public abstract class Expression : Parsed.Object
	{
        public Runtime.Expression runtimeExpression { get { return (Runtime.Expression)this.runtimeObject; } }

		public override Runtime.Object GenerateRuntimeObject ()
		{
			var termList = new List<object> ();

			GenerateIntoList (termList);

			return new Runtime.Expression (termList);
		}

		public abstract void GenerateIntoList (List<object> termList);

	}

	public class BinaryExpression : Expression
	{
		public Expression leftExpression;
		public Expression rightExpression;
		public char op;

		public BinaryExpression(Expression left, Expression right, char op)
		{
			left.parent = this;
			right.parent = this;

			leftExpression = left;
			rightExpression = right;
			this.op = op;
		}

		public override void GenerateIntoList(List<object> termList)
		{
			leftExpression.GenerateIntoList (termList);
			rightExpression.GenerateIntoList (termList);
			termList.Add (op);
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

		public override void GenerateIntoList(List<object> termList)
		{
			innerExpression.GenerateIntoList (termList);
			termList.Add (Runtime.Expression.Negate); // '~'
		}

        public override void ResolveReferences (Story context)
        {
            innerExpression.ResolveReferences (context);
        }
	}
}

