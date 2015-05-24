using System;
using Inklewriter;

namespace Inklewriter.Parsed
{
	public class Expression : Parsed.Object
	{
		public Expression ()
		{
			
		}

		public override Runtime.Object GenerateRuntimeObject ()
		{
			return new Runtime.Text ("xxx");
		}
	}

	public class BinaryExpression : Expression
	{
		public BinaryExpression(Expression left, Expression right, char op)
		{

		}
	}
}

