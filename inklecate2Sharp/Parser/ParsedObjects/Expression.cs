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
            container.AddContent (Runtime.ControlCommand.EvalStart());

            GenerateIntoContainer (container);

            // Tell Runtime to output the result of the expression evaluation to the output stream
            if (outputWhenComplete) {
                container.AddContent (Runtime.ControlCommand.EvalOutput());
            }

            // Tell Runtime to stop evaluating the content as an expression
            container.AddContent (Runtime.ControlCommand.EvalEnd());

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

            opName = NativeNameForOp (opName);

            container.AddContent(Runtime.NativeFunctionCall.CallWithName(opName));
		}

        public override void ResolveReferences (Story context)
        {
            leftExpression.ResolveReferences (context);
            rightExpression.ResolveReferences (context);
        }

        string NativeNameForOp(string opName)
        {
            if (opName == "and")
                return "&&";
            if (opName == "or")
                return "||";
            return opName;
        }
	}

    public class UnaryExpression : Expression
	{
		public Expression innerExpression;
        public string op;

        public UnaryExpression(Expression inner, string op)
		{
			inner.parent = this;
			this.innerExpression = inner;
            this.op = op;
		}

        public override void GenerateIntoContainer(Runtime.Container container)
		{
			innerExpression.GenerateIntoContainer (container);

            string nativeOp = NativeNameForOp(this.op);
            container.AddContent(Runtime.NativeFunctionCall.CallWithName(nativeOp));
		}

        public override void ResolveReferences (Story context)
        {
            innerExpression.ResolveReferences (context);
        }

        string NativeNameForOp(string opName)
        {
            // Replace "-" with "~" to make it unique
            if (opName == "-")
                return "~";
            if (opName == "not")
                return "!";
            return opName;
        }
	}

    public class IncDecExpression : Expression
    {
        public string varName;
        public bool isInc;

        public IncDecExpression(string varName, bool isInc)
        {
            this.varName = varName;
            this.isInc = isInc;
        }

        public override void GenerateIntoContainer(Runtime.Container container)
        {
            // x = x + 1
            // ^^^ ^ ^ ^
            //  4  1 3 2
            // Reverse polish notation: (x 1 +) (assign to x)

            // 1.
            container.AddContent (new Runtime.VariableReference (varName));

            // 2.
            container.AddContent (new Runtime.LiteralInt (isInc ? 1 : -1));

            // 3.
            container.AddContent (Runtime.NativeFunctionCall.CallWithName ("+"));

            // 4.
            container.AddContent (new Runtime.VariableAssignment (varName, false));

            // Finally, leave the variable on the stack so it can be used as a sub-expression
            container.AddContent (new Runtime.VariableReference (varName));
        }

        public override void ResolveReferences (Story context)
        {
            if (!context.HasVariableWithName (varName, allowReadCounts: false)) {
                Error ("variable for "+incrementDecrementWord+" could not be found: '"+varName+"'");
            }
        }

        string incrementDecrementWord {
            get {
                if (isInc)
                    return "increment";
                else
                    return "decrement";
            }
        }
    }
        
}

