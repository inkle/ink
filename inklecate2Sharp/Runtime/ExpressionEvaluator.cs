using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Runtime
{
	public class ExpressionEvaluator
	{
		public ExpressionEvaluator ()
		{
			evaluationStack = new List<object> ();

			binaryOps = new Dictionary<char, BinaryOp> ();
			unaryOps = new Dictionary<char, UnaryOp> ();
			RegisterBinaryOp('+', (x, y) => (int)x + (int)y);
			RegisterBinaryOp('-', (x, y) => (int)x - (int)y);
			RegisterBinaryOp('*', (x, y) => (int)x * (int)y);
			RegisterBinaryOp('/', (x, y) => (int)x / (int)y);

			// unary negation - use different character to distinguish
			// from binary subtraction
			RegisterUnaryOp ('~', x => -(int)x); 
		}

		public object Evaluate(Expression expr)
		{
			evaluationStack.Clear ();

			foreach(var term in expr.terms) {

				// Values
				if (term is int) {
					evaluationStack.Add (term);
				} 

				// Operators
				else if (term is char) {
					char op = (char)term;

					if (binaryOps.ContainsKey (op)) {
						DoBinary (binaryOps [op]);
					}

					else if (unaryOps.ContainsKey (op)) {
						DoUnary (unaryOps [op]);
					}
				}
			}

			Debug.Assert (evaluationStack.Count == 1);

			if (evaluationStack.Count > 0) {
				return evaluationStack.Last ();
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
	}
}

