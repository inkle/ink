using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Runtime
{
	public class Expression : Runtime.Object
	{
		// Terms in reverse polish notation
		// e.g. 3 * (4 + 5) is represented as: 3 4 5 + *
		public List<object> terms { get; protected set; }
		
		public Expression (IEnumerable<object> terms)
		{
			this.terms = new List<object>(terms);
			evaluationStack = new List<object> ();

			// TODO: Move all this to a single ExpressionEvaluator that's owned by the Story?
			binaryOps = new Dictionary<char, BinaryOp> ();
			unaryOps = new Dictionary<char, UnaryOp> ();
			RegisterBinaryOp('+', (x, y) => (int)x + (int)y);
			RegisterBinaryOp('-', (x, y) => (int)x - (int)y);
			RegisterBinaryOp('*', (x, y) => (int)x * (int)y);
			RegisterBinaryOp('/', (x, y) => (int)x / (int)y);
			RegisterUnaryOp ('~', x => -(int)x); // unary negation
		}

		void RegisterBinaryOp (char opChar, BinaryOp opFunc)
		{
			binaryOps [opChar] = opFunc;
		}

		void RegisterUnaryOp(char opChar, UnaryOp opFunc)
		{
			unaryOps [opChar] = opFunc;
		}

		public object Evaluate()
		{
			evaluationStack.Clear ();

			foreach(var term in terms) {

				if (term is int) {
					evaluationStack.Add (term);
				} else if (term is char) {
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

		protected object Pop()
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

		protected void Push(object obj)
		{
			evaluationStack.Add (obj);
		}

		List<object> evaluationStack;
		Dictionary<char, BinaryOp> binaryOps;
		Dictionary<char, UnaryOp> unaryOps;
	}
}

