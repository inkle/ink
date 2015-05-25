using System;
using System.Collections.Generic;

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
		}
			
		public override string ToString ()
		{
			// For debug purposes, just create our own evaluator, even though normally
			// the Story owns it.
			var eval = new ExpressionEvaluator ();
			return eval.StringRepresentation (this);
		}
	}
}

