using System;
using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser : StringParser
	{
		public InkParserState state { get; }

		public InkParser(string str) : base(str) { 

			RegisterExpressionOperators ();

			state = new InkParserState();
		}
			
		public override int lineIndex
		{
			set {
				state.lineIndex = value;
			}
			get {
				return state.lineIndex;
			}
		}

		public override int index
		{
			set {
				state.characterIndex = value;
			}
			get {
				return state.characterIndex;
			}
		}

		protected override void BeginRule()
		{
			state.Push ();
		}

		protected override object FailRule()
		{
			state.Pop ();
			return null;
		}

		protected override void CancelRule()
		{
			state.Pop ();
		}

		protected override object SucceedRule(object result = null)
		{
			// Get state at point where this rule stared evaluating
			var stateAtBeginRule = state.Peek ();

			// Apply DebugMetadata based on the state at the start of the rule
			// (i.e. use line number as it was at the start of the rule)
			var parsedObj = result as Parsed.Object;
			if ( parsedObj != null) {
				var md = new DebugMetadata ();
				md.lineNumber = stateAtBeginRule.lineIndex + 1;
				parsedObj.debugMetadata = md;
			}

			// Flatten state stack so that we maintain the same values,
			// but remove one level in the stack.
			state.Squash();

			if (result == null) {
				result = ParseSuccess;
			}

			return result;
		}

	}
}

