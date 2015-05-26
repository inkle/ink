using System;
using Inklewriter.Parsed;
using System.Collections.Generic;

namespace Inklewriter
{
	public partial class InkParser
	{
		protected class InfixOperator
		{
			public string type;
			public int precedence;

			public InfixOperator(string type, int precedence) {
				this.type = type;
				this.precedence = precedence;
			}

			public override string ToString ()
			{
				return type;
			}
		}

        protected Parsed.Object VariableDeclarationOrAssignment()
        {
            BeginRule ();

            Whitespace ();

            // VarKeyword includes trailing whitespace
            bool isNewDeclaration = VarKeyword ();

            string varName = (string) Expect(Identifier, "variable name");

            Whitespace();

            // Optional assignment
            Expression assignedExpression = null;
            if (ParseString ("=") != null) {
                assignedExpression = (Expression)Expect (Expression, "value expression to be assigned to variable");
            }

            // Default zero assignment
            else {
                assignedExpression = new Number (0);
            }

            var result = new VariableAssignment (varName, assignedExpression, isNewDeclaration);

            return SucceedRule(result) as Parsed.Object;
        }


        protected bool VarKeyword()
        {
            BeginRule ();

            if( ParseString ("var") == null ) {
                FailRule ();
                return false;
            }

            // Require whitespace now, since statement could be e.g. ~ variableThing = 5
            if (Whitespace() == null) {
                FailRule ();
                return false;
            }

            SucceedRule ();

            return true;
        }

        protected Parsed.Object ReturnStatement()
        {
            BeginRule ();

            Whitespace ();

            if (ParseString ("return") == null)
                return (Parsed.Object)FailRule ();

            Whitespace ();

            var expr = Expression ();

            var returnObj = new Return (expr);

            return (Parsed.Object) SucceedRule (returnObj);
        }

		protected Expression Expression() {
			return Expression(minimumPrecedence:0);
		}

		// Pratt Parser
		// aka "Top down operator precedence parser"
		// http://journal.stuffwithstuff.com/2011/03/19/pratt-parsers-expression-parsing-made-easy/
		// Algorithm overview:
		// The two types of precedence are handled in two different ways:
		//   ((((a . b) . c) . d) . e)			#1
		//   (a . (b . (c . (d . e))))			#2
		// Where #1 is automatically handled by successive loops within the main 'while' in this function,
		// so long as continuing operators have lower (or equal) precedence (e.g. imagine some series of "*"s then "+" above.
		// ...and #2 is handled by recursion of the right hand term in the binary expression parser.
		// (see link for advice on how to extend for postfix and mixfix operators)
		protected Expression Expression(int minimumPrecedence)
		{
			BeginRule ();

			Whitespace ();

			// First parse a unary expression e.g. "-a" or parethensised "(1 + 2)"
			var expr = ExpressionUnary ();
			if (expr == null) {
				return FailRule () as Expression;
			}

			Whitespace ();

			// Attempt to parse (possibly multiple) continuing infix expressions (e.g. 1 + 2 + 3)
			while(true) {
				BeginRule ();

				// Operator
				var infixOp = ParseInfixOperator ();
				if (infixOp != null && infixOp.precedence > minimumPrecedence) {

					// Expect right hand side of operator
					var expectationMessage = string.Format("right side of '{0}' expression", infixOp.type);
					var multiaryExpr = Expect (() => ExpressionInfixRight (left: expr, op: infixOp), expectationMessage);

					expr = SucceedRule(multiaryExpr) as Parsed.Expression;

					continue;
				}

				FailRule ();
				break;
			}

            Whitespace ();

			return SucceedRule(expr) as Expression;
		}

		protected Expression ExpressionUnary()
		{
			BeginRule ();

			bool negated = ParseString ("-") != null;

			Whitespace ();

            var expr = OneOf (ExpressionParen, ExpressionLiteral, ExpressionFunctionCall, ExpressionVariableName) as Expression;
			if (expr == null) {
				return FailRule () as Expression;
			}

			if (negated) {
				expr = new NegatedExpression (expr);
			}

			return SucceedRule (expr) as Expression;
		}

		protected Expression ExpressionLiteral()
		{
			int? intOrNull = ParseInt ();
			if (intOrNull == null) {
				return null;
			} else {
				return new Number (intOrNull.Value);
			}
		}

        protected Expression ExpressionFunctionCall()
        {
            BeginRule ();

            var iden = Identifier ();
            if (iden == null) 
                return (Expression) FailRule();

            Whitespace ();

            if (ParseString ("(") == null)
                return (Expression)FailRule ();

            // "Exclude" requires the rule to succeed, but causes actual comma string to be excluded from the list of results
            ParseRule commas = Exclude (() => ParseString (","));
            var arguments = Interleave<Expression>(Expression, commas);

            Whitespace ();

            Expect (() => ParseString (")"), "closing ')' for function call");

            // TODO: Build function call object
            var f = new FunctionCall(iden, arguments);

            return (FunctionCall) SucceedRule (f);

        }

        protected Expression ExpressionVariableName()
        {
            var iden = Identifier ();
            if (iden == null) {
                return null;
            } else {
                return new VariableReference (iden);
            }
        }

		protected Expression ExpressionParen()
		{
			BeginRule ();

			if (ParseString ("(") == null) {
				return FailRule () as Expression;
			}

			var innerExpr = Expression ();
			if (innerExpr == null) {
				return FailRule () as Expression;
			}

			Whitespace ();

			Expect (() => ParseString(")"), "closing parenthesis ')' for expression");

			return SucceedRule (innerExpr) as Expression;
		}

		protected Expression ExpressionInfixRight(Parsed.Expression left, InfixOperator op)
		{
			BeginRule ();

			Whitespace ();

			var right = Expression (op.precedence);
			if (right != null) {

				// We assume that the character we use for the operator's type is the same
				// as that used internally by e.g. Runtime.Expression.Add, Runtime.Expression.Multiply etc
				var expr = new BinaryExpression (left, right, op.type);
				return SucceedRule (expr) as Expression;
			}

			return FailRule () as Expression;

		}

		private InfixOperator ParseInfixOperator()
		{
			var strOperator = ParseCharactersFromCharSet (_operatorsCharSet, maxCount: 1);
			if (strOperator != null) {
				return _operators [strOperator];
			} else {
				return null;
			}
		}

		void RegisterExpressionOperators()
		{
			_operators = new Dictionary<string, InfixOperator> ();
			_operatorsCharSet = new CharacterSet ();

			RegisterOperator ("+", precedence:1);
			RegisterOperator ("-", precedence:2);
			RegisterOperator ("*", precedence:3);
			RegisterOperator ("/", precedence:4);
		}

		void RegisterOperator(string op, int precedence)
		{
			_operators [op] = new InfixOperator (op, precedence);
			_operatorsCharSet.Add (op[0]);
		}

		Dictionary<string, InfixOperator> _operators;
		CharacterSet _operatorsCharSet;
	}
}

