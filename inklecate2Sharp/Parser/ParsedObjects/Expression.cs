using System.Collections.Generic;
using System.Linq;

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
            leftExpression = AddContent(left);
            rightExpression = AddContent(right);
			this.opName = opName;
		}

        public override void GenerateIntoContainer(Runtime.Container container)
		{
			leftExpression.GenerateIntoContainer (container);
			rightExpression.GenerateIntoContainer (container);

            opName = NativeNameForOp (opName);

            container.AddContent(Runtime.NativeFunctionCall.CallWithName(opName));
		}

        string NativeNameForOp(string opName)
        {
            if (opName == "and")
                return "&&";
            if (opName == "or")
                return "||";
            return opName;
        }

        public override string ToString ()
        {
            return string.Format ("({0} {1} {2})", leftExpression, opName, rightExpression);
        }
	}

    public class UnaryExpression : Expression
	{
		public Expression innerExpression;
        public string op;

        public UnaryExpression(Expression inner, string op)
		{
            this.innerExpression = AddContent(inner);
            this.op = op;
		}

        public override void GenerateIntoContainer(Runtime.Container container)
		{
			innerExpression.GenerateIntoContainer (container);

            container.AddContent(Runtime.NativeFunctionCall.CallWithName(nativeNameForOp));
		}

        public override string ToString ()
        {
            return nativeNameForOp + innerExpression;
        }

        string nativeNameForOp
        {
            get {
                // Replace "-" with "~" to make it unique
                if (op == "-")
                    return "~";
                if (op == "not")
                    return "!";
                return op;
            }
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
            if (!context.ResolveVariableWithName (varName, fromNode:this)) {
                Error ("variable for "+incrementDecrementWord+" could not be found: '"+varName+"' after searching: "+this.DescriptionOfScope ());
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

        public override string ToString ()
        {
            return varName + (isInc ? "++" : "--");
        }
    }

    public class MultipleConditionExpression : Expression
    {
        public List<Expression> subExpressions {
            get {
                return this.content.Cast<Expression> ().ToList ();
            }
        }

        public MultipleConditionExpression(List<Expression> conditionExpressions)
        {
            AddContent (conditionExpressions);
        }

        public override void GenerateIntoContainer(Runtime.Container container)
        {
            //    A && B && C && D
            // => (((A B &&) C &&) D &&) etc
            bool isFirst = true;
            foreach (var conditionExpr in subExpressions) {

                conditionExpr.GenerateIntoContainer (container);

                if (!isFirst) {
                    container.AddContent (Runtime.NativeFunctionCall.CallWithName ("&&"));
                }

                isFirst = false;
            }
        }
    }
        
}

