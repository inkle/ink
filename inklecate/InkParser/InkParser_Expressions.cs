using System;
using Inklewriter.Parsed;
using System.Collections.Generic;

namespace Inklewriter
{
	internal partial class InkParser
	{
		protected class InfixOperator
		{
			public string type;
			public int precedence;
            public bool requireWhitespace;

            public InfixOperator(string type, int precedence, bool requireWhitespace) {
				this.type = type;
				this.precedence = precedence;
                this.requireWhitespace = requireWhitespace;
			}

			public override string ToString ()
			{
				return type;
			}
		}

        protected Parsed.Object TempDeclarationOrAssignment()
        {
            Whitespace ();

            bool isNewDeclaration = ParseTempKeyword();

            Whitespace ();

            string varName = null;
            if (isNewDeclaration) {
                varName = (string)Expect (Identifier, "variable name");
            } else {
                varName = Parse(Identifier);
            }

            if (varName == null) {
                return null;
            }

            Whitespace();

            // Optional assignment
            Expression assignedExpression = null;
            if (ParseString ("=") != null) {
                assignedExpression = (Expression)Expect (Expression, "value expression to be assigned to temporary variable");
            }

            // If it's neither an assignment nor a new declaration,
            // it's got nothing to do with this rule (e.g. it's actually just "~ myExpr" or even "~ myFunc()"
            else if (!isNewDeclaration) {
                return null;
            }

            // Default zero assignment
            else {
                assignedExpression = new Number (0);
            }

            var result = new VariableAssignment (varName, assignedExpression);
            result.isNewTemporaryDeclaration = true;
            return result;
        }


        protected bool ParseTempKeyword()
        {
            var ruleId = BeginRule ();

            if (Parse (Identifier) == "temp") {
                SucceedRule (ruleId);
                return true;
            } else {
                FailRule (ruleId);
                return false;
            }
        }
            
        protected Parsed.Return ReturnStatement()
        {
            Whitespace ();

            var returnOrDone = Parse(Identifier);
            if (returnOrDone != "return") {
                return null;
            }

            Whitespace ();

            var expr = Parse(Expression);

            var returnObj = new Return (expr);
            return returnObj;
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
			Whitespace ();

			// First parse a unary expression e.g. "-a" or parethensised "(1 + 2)"
			var expr = ExpressionUnary ();
			if (expr == null) {
                return null;
			}

			Whitespace ();

			// Attempt to parse (possibly multiple) continuing infix expressions (e.g. 1 + 2 + 3)
			while(true) {
				var ruleId = BeginRule ();

				// Operator
				var infixOp = ParseInfixOperator ();
				if (infixOp != null && infixOp.precedence > minimumPrecedence) {

					// Expect right hand side of operator
					var expectationMessage = string.Format("right side of '{0}' expression", infixOp.type);
					var multiaryExpr = Expect (() => ExpressionInfixRight (left: expr, op: infixOp), expectationMessage);
                    if (multiaryExpr == null) {

                        // Fail for operator and right-hand side of multiary expression
                        FailRule (ruleId);

                        return null;
                    }

                    expr = SucceedRule(ruleId, multiaryExpr) as Parsed.Expression;

					continue;
				}

                FailRule (ruleId);
				break;
			}

            Whitespace ();

            return expr;
		}

        protected Expression ExpressionUnary()
		{
            // Divert target is a special case - it can't have any other operators
            // applied to it, and we also want to check for it first so that we don't
            // confuse "->" for subtraction.
            var divertTarget = Parse (ExpressionDivertTarget);
            if (divertTarget != null) {
                return divertTarget;
            }

            var prefixOp = (string) OneOf (String ("-"), String ("!"));

            // Don't parse like the string rules above, in case its actually 
            // a variable that simply starts with "not", e.g. "notable".
            // This rule uses the Identifer rule, which will scan as much text
            // as possible before returning.
            if (prefixOp == null) {
                prefixOp = Parse(ExpressionNot);
            }

			Whitespace ();

            var expr = OneOf (ExpressionParen, ExpressionLiteral, ExpressionFunctionCall, ExpressionVariableName) as Expression;

            // Only recurse immediately if we have one of the (usually optional) unary ops
            if (expr == null && prefixOp != null) {
                expr = ExpressionUnary ();
            }

			if (expr == null)
                return null;

            if (prefixOp != null) {
                expr = new UnaryExpression (expr, prefixOp);
			}

            Whitespace ();

            var postfixOp = (string) OneOf (String ("++"), String ("--"));
            if (postfixOp != null) {
                bool isInc = postfixOp == "++";

                if (!(expr is VariableReference)) {
                    Error ("can only increment and decrement variables, but saw '" + expr + "'");

                    // Drop down and succeed without the increment after reporting error
                } else {
                    var varRef = (VariableReference)expr;
                    expr = new IncDecExpression(varRef.name, isInc);
                }

            }

            return expr;
		}

        protected string ExpressionNot()
        {
            var id = Identifier ();
            if (id == "not") {
                return id;
            }

            return null;
        }

		protected Expression ExpressionLiteral()
		{
            return (Expression) OneOf (ExpressionFloat, ExpressionInt, ExpressionBool);
		}

        protected Expression ExpressionDivertTarget()
        {
            Whitespace ();

            var divert = Parse(SingleDivert);
            if (divert == null)
                return null;

            Whitespace ();

            return new DivertTarget (divert);
        }

        protected Number ExpressionInt()
        {
            int? intOrNull = ParseInt ();
            if (intOrNull == null) {
                return null;
            } else {
                return new Number (intOrNull.Value);
            }
        }

        protected Number ExpressionFloat()
        {
            float? floatOrNull = ParseFloat ();
            if (floatOrNull == null) {
                return null;
            } else {
                return new Number (floatOrNull.Value);
            }
        }

        protected Number ExpressionBool()
        {
            var id = Parse(Identifier);
            if (id == "true" || id == "yes" || id == "on") {
                return new Number (1);
            } else if (id == "false" || id == "no" || id == "off") {
                return new Number (0);
            }

            return null;
        }

        protected Expression ExpressionFunctionCall()
        {
            var iden = Parse(Identifier);
            if (iden == null)
                return null;

            Whitespace ();

            var arguments = Parse(ExpressionFunctionCallArguments);
            if (arguments == null) {
                return null;
            }

            return new FunctionCall(iden, arguments);
        }

        protected List<Expression> ExpressionFunctionCallArguments()
        {
            if (ParseString ("(") == null)
                return null;

            // "Exclude" requires the rule to succeed, but causes actual comma string to be excluded from the list of results
            ParseRule commas = Exclude (String (","));
            var arguments = Interleave<Expression>(Expression, commas);
            if (arguments == null) {
                arguments = new List<Expression> ();
            }

            Whitespace ();

            Expect (String (")"), "closing ')' for function call");

            return arguments;
        }

        protected Expression ExpressionVariableName()
        {
            List<string> path = Interleave<string> (Identifier, Exclude (Spaced (String ("."))));
            
            if (path == null)
                return null;
            
            return new VariableReference (path);
        }

		protected Expression ExpressionParen()
		{
			if (ParseString ("(") == null)
                return null;

            var innerExpr = Parse(Expression);
			if (innerExpr == null)
                return null;

			Whitespace ();

            Expect (String(")"), "closing parenthesis ')' for expression");

            return innerExpr;
		}

		protected Expression ExpressionInfixRight(Parsed.Expression left, InfixOperator op)
		{
			Whitespace ();

            var right = Parse(() => Expression (op.precedence));
			if (right) {

				// We assume that the character we use for the operator's type is the same
				// as that used internally by e.g. Runtime.Expression.Add, Runtime.Expression.Multiply etc
				var expr = new BinaryExpression (left, right, op.type);
                return expr;
			}

            return null;

		}

		private InfixOperator ParseInfixOperator()
		{
            foreach (var op in _binaryOperators) {

                int ruleId = BeginRule ();

                if (ParseString (op.type) != null) {

                    if (op.requireWhitespace) {
                        if (Whitespace () == null) {
                            FailRule (ruleId);
                            continue;
                        }
                    }

                    return (InfixOperator) SucceedRule(ruleId, op);
                }

                FailRule (ruleId);
            }

            return null;
		}

		void RegisterExpressionOperators()
		{
            _maxBinaryOpLength = 0;
			_binaryOperators = new List<InfixOperator> ();

            // These will be tried in order, so we need "<=" before "<"
            // for correctness

            RegisterBinaryOperator ("&&", precedence:1);
            RegisterBinaryOperator ("||", precedence:1);
            RegisterBinaryOperator ("and", precedence:1, requireWhitespace: true);
            RegisterBinaryOperator ("or", precedence:1, requireWhitespace: true);

            RegisterBinaryOperator ("==", precedence:2);
            RegisterBinaryOperator (">=", precedence:2);
            RegisterBinaryOperator ("<=", precedence:2);
            RegisterBinaryOperator ("<", precedence:2);
            RegisterBinaryOperator (">", precedence:2);
            RegisterBinaryOperator ("!=", precedence:2);

			RegisterBinaryOperator ("+", precedence:3);
			RegisterBinaryOperator ("-", precedence:4);
			RegisterBinaryOperator ("*", precedence:5);
			RegisterBinaryOperator ("/", precedence:6);

            RegisterBinaryOperator ("%", precedence:7);
            RegisterBinaryOperator ("mod", precedence:7, requireWhitespace:true);
		}

        void RegisterBinaryOperator(string op, int precedence, bool requireWhitespace = false)
		{
            _binaryOperators.Add(new InfixOperator (op, precedence, requireWhitespace));
            _maxBinaryOpLength = Math.Max (_maxBinaryOpLength, op.Length);
		}

        List<InfixOperator> _binaryOperators;
        int _maxBinaryOpLength;
	}
}

