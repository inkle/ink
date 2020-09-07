using System;
using Ink.Parsed;
using System.Collections.Generic;

namespace Ink
{
	public partial class InkParser
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

            Identifier varIdentifier = null;
            if (isNewDeclaration) {
                varIdentifier = (Identifier)Expect (IdentifierWithMetadata, "variable name");
            } else {
                varIdentifier = Parse(IdentifierWithMetadata);
            }

            if (varIdentifier == null) {
                return null;
            }

            Whitespace();

            // += -=
            bool isIncrement = ParseString ("+") != null;
            bool isDecrement = ParseString ("-") != null;
            if (isIncrement && isDecrement) Error ("Unexpected sequence '+-'");

            if (ParseString ("=") == null) {
                // Definitely in an assignment expression?
                if (isNewDeclaration) Error ("Expected '='");
                return null;
            }

            Expression assignedExpression = (Expression)Expect (Expression, "value expression to be assigned");

            if (isIncrement || isDecrement) {
                var result = new IncDecExpression (varIdentifier, assignedExpression, isIncrement);
                return result;
            } else {
                var result = new VariableAssignment (varIdentifier, assignedExpression);
                result.isNewTemporaryDeclaration = isNewDeclaration;
                return result;
            }
        }

        protected void DisallowIncrement (Parsed.Object expr)
        {
        	if (expr is Parsed.IncDecExpression)
        		Error ("Can't use increment/decrement here. It can only be used on a ~ line");
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
            // This rule uses the Identifier rule, which will scan as much text
            // as possible before returning.
            if (prefixOp == null) {
                prefixOp = Parse(ExpressionNot);
            }

			Whitespace ();

            // - Since we allow numbers at the start of variable names, variable names are checked before literals
            // - Function calls before variable names in case we see parentheses
            var expr = OneOf (ExpressionList, ExpressionParen, ExpressionFunctionCall, ExpressionVariableName, ExpressionLiteral) as Expression;

            // Only recurse immediately if we have one of the (usually optional) unary ops
            if (expr == null && prefixOp != null) {
                expr = ExpressionUnary ();
            }

			if (expr == null)
                return null;

            if (prefixOp != null) {
                expr = UnaryExpression.WithInner(expr, prefixOp);
			}

            Whitespace ();

            var postfixOp = (string) OneOf (String ("++"), String ("--"));
            if (postfixOp != null) {
                bool isInc = postfixOp == "++";

                if (!(expr is VariableReference)) {
                    Error ("can only increment and decrement variables, but saw '" + expr + "'");

                    // Drop down and succeed without the increment after reporting error
                } else {
                    // TODO: Language Server - (Identifier combined into one vs. list of Identifiers)
                    var varRef = (VariableReference)expr;
                    expr = new IncDecExpression(varRef.identifier, isInc);
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
            return (Expression) OneOf (ExpressionFloat, ExpressionInt, ExpressionBool, ExpressionString);
		}

        protected Expression ExpressionDivertTarget()
        {
            Whitespace ();

            var divert = Parse(SingleDivert);
            if (divert == null)
                return null;

            if (divert.isThread)
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

        protected StringExpression ExpressionString()
        {
            var openQuote = ParseString ("\"");
            if (openQuote == null)
                return null;

            // Set custom parser state flag so that within the text parser,
            // it knows to treat the quote character (") as an end character
            parsingStringExpression = true;

            List<Parsed.Object> textAndLogic = Parse (MixedTextAndLogic);

            Expect (String ("\""), "close quote for string expression");

            parsingStringExpression = false;

            if (textAndLogic == null) {
                textAndLogic = new List<Ink.Parsed.Object> ();
                textAndLogic.Add (new Parsed.Text (""));
            }

            else if (textAndLogic.Exists (c => c is Divert))
                Error ("String expressions cannot contain diverts (->)");

            return new StringExpression (textAndLogic);
        }

        protected Number ExpressionBool()
        {
            var id = Parse(Identifier);
            if (id == "true") {
                return new Number (true);
            } else if (id == "false") {
                return new Number (false);
            }

            return null;
        }

        protected Expression ExpressionFunctionCall()
        {
            var iden = Parse(IdentifierWithMetadata);
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
            List<Identifier> path = Interleave<Identifier> (IdentifierWithMetadata, Exclude (Spaced (String ("."))));

            if (path == null || Story.IsReservedKeyword (path[0].name) )
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

        protected Parsed.List ExpressionList ()
        {
            Whitespace ();

            if (ParseString ("(") == null)
                return null;

            Whitespace ();

            // When list has:
            //  - 0 elements (null list) - this is okay, it's an empty list: "()"
            //  - 1 element - it could be confused for a single non-list related
            //    identifier expression in brackets, but this is a useless thing
            //    to do, so we reserve that syntax for a list with one item.
            //  - 2 or more elements - normal!
            List<Identifier> memberNames = SeparatedList (ListMember, Spaced (String (",")));

            Whitespace ();

            // May have failed to parse the inner list - the parentheses may
            // be for a normal expression
            if (ParseString (")") == null)
                return null;

            return new List (memberNames);
        }

        protected Identifier ListMember ()
        {
            Whitespace ();

            Identifier identifier = Parse (IdentifierWithMetadata);
            if (identifier == null)
                return null;

            var dot = ParseString (".");
            if (dot != null) {
                Identifier identifier2 = Expect (IdentifierWithMetadata, "element name within the set " + identifier) as Identifier;
                identifier.name = identifier.name + "." + identifier2?.name;
            }

            Whitespace ();

            return identifier;
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

            // (apples, oranges) + cabbages has (oranges, cabbages) == true
            RegisterBinaryOperator ("?", precedence: 3);
            RegisterBinaryOperator ("has", precedence: 3, requireWhitespace:true);
            RegisterBinaryOperator ("!?", precedence: 3);
            RegisterBinaryOperator ("hasnt", precedence: 3, requireWhitespace: true);
            RegisterBinaryOperator ("^", precedence: 3);

			RegisterBinaryOperator ("+", precedence:4);
			RegisterBinaryOperator ("-", precedence:5);
			RegisterBinaryOperator ("*", precedence:6);
			RegisterBinaryOperator ("/", precedence:7);

            RegisterBinaryOperator ("%", precedence:8);
            RegisterBinaryOperator ("mod", precedence:8, requireWhitespace:true);


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

