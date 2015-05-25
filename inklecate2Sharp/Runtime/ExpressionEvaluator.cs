using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace Inklewriter.Runtime
{
	public class ExpressionEvaluator
	{
		public ExpressionEvaluator ()
		{
			evaluationStack = new List<object> ();

			binaryOps = new Dictionary<char, BinaryOp> ();
			unaryOps = new Dictionary<char, UnaryOp> ();
			RegisterBinaryOp(Expression.Add, 	  (x, y) => (int)x + (int)y);
			RegisterBinaryOp(Expression.Subtract, (x, y) => (int)x - (int)y);
			RegisterBinaryOp(Expression.Multiply, (x, y) => (int)x * (int)y);
			RegisterBinaryOp(Expression.Divide,   (x, y) => (int)x / (int)y);
			RegisterUnaryOp (Expression.Negate, x => -(int)x); 
		}

        public object Evaluate(Expression expr, Story context = null)
		{
            // Temporarily keep a refrence to the story context
            currentContext = context;

			evaluationStack.Clear ();

			foreach(var term in expr.terms) {

				// Values
                if (term is int) {
                    Push (term);
                } 

				// Operators
				else if (term is char) {
                    char op = (char)term;

                    if (binaryOps.ContainsKey (op)) {
                        DoBinary (binaryOps [op]);
                    } else if (unaryOps.ContainsKey (op)) {
                        DoUnary (unaryOps [op]);
                    }
                }

                // Variable reference
                else if (term is string) {
                    var varName = (string)term;
                    Debug.Assert (currentContext.variables.ContainsKey (varName), "Variable could not be found");
                    object varContents = context.variables [varName];
                    Push (varContents);
                }
			}

            // Reset reference to the story context
            currentContext = null;

			Debug.Assert (evaluationStack.Count == 1);

			if (evaluationStack.Count > 0) {
				return evaluationStack.Last ();
			}

			return null;
		}

		public string StringRepresentation(Expression expr)
		{
			evaluationStack.Clear ();

			foreach(var term in expr.terms) {

				// Values
                if (term is int) {
                    Push (term);
                } 

				// Operators
				else if (term is char) {
                    char op = (char)term;

                    if (binaryOps.ContainsKey (op)) {
                        DoBinary ((x, y) => string.Format ("({0} {1} {2})", x, op, y));
                    }

					// Assume it's prefix unary (we only have negation at time of writing)
					else if (unaryOps.ContainsKey (op)) {

                        // Replace special "~" negation operator with human-friendly "-"
                        if (op == Expression.Negate) {
                            op = '-';
                        }

                        DoUnary (x => string.Format ("{0}{1}", op, x));
                    }
                } 

                // Variable reference
                else if (term is string) {
                    Push (term);
                }
			}

			Debug.Assert (evaluationStack.Count == 1);

			if (evaluationStack.Count > 0) {
				return evaluationStack.Last ().ToString();
			}

			return null;
		}

		delegate object BinaryOp(object left, object right);
		delegate object UnaryOp(object val);

		void RegisterBinaryOp (char opChar, BinaryOp opFunc)
		{
			binaryOps [opChar] = opFunc;
		}

		void RegisterUnaryOp(char opChar, UnaryOp opFunc)
		{
			unaryOps [opChar] = opFunc;
		}

		void DoBinary(BinaryOp op)
		{
			object right = Pop ();
			object left = Pop ();
			object result = op (left, right);
			Push (result);
		}

		void DoUnary(UnaryOp op)
		{
			object val = Pop ();
			object result = op (val);
			Push (result);
		}

		object Pop()
		{
			var count = evaluationStack.Count;
			if (count > 0) {
				var result = evaluationStack [count - 1];
				evaluationStack.RemoveAt (count - 1);
				return result;
			} else {
				return null;
			}
		}

		void Push(object obj)
		{
			evaluationStack.Add (obj);
		}

		List<object> evaluationStack;
		Dictionary<char, BinaryOp> binaryOps;
		Dictionary<char, UnaryOp> unaryOps;
        Story currentContext;
	}
}

