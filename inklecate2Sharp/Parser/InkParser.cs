using System;
using System.Collections.Generic;
using System.Linq;
using inklecate2Sharp.Parsed;

namespace inklecate2Sharp
{
	public partial class InkParser : StringParser
	{
		public InkParser(string str) : base(str) { 
			_stack = new List<InkStateElement> ();
		}

		protected class InkStateElement {
			public int index;
			public int lineIndex;
		}

		protected InkStateElement parseState
		{
			get {
				InkStateElement state = new InkStateElement ();
				state.lineIndex = lineIndex;
				state.index = index;
				return state;
			}

			set {
				InkStateElement state = value as InkStateElement;
				lineIndex = state.lineIndex;
				index = state.index;
			}
		}

		protected override void BeginRule()
		{
			_stack.Add (parseState);
		}

		protected override object FailRule()
		{
			if (_stack.Count == 0) {
				throw new System.Exception ("State stack already empty! Mismatched Begin/Succceed/Fail?");
			}

			// Restore state
			SetParseState(_stack.Last(), dueToFailure:true);
			_stack.RemoveAt (_stack.Count - 1);

			return null;
		}

		protected override void CancelRule()
		{
			FailRule ();
		}

		protected override object SucceedRule(object result = null)
		{
			if (_stack.Count == 0) {
				throw new System.Exception ("State stack already empty! Mismatched Begin/Succceed/Fail?");
			}

			// Get state at point where this rule stared evaluating
			InkStateElement stateAtBeginRule = _stack.Last ();

			// Apply DebugMetadata based on the state at the start of the rule
			// (i.e. use line number as it was at the start of the rule)
			var parsedObj = result as Parsed.Object;
			if ( parsedObj != null) {
				var md = new DebugMetadata ();
				md.lineNumber = stateAtBeginRule.lineIndex + 1;
				parsedObj.debugMetadata = md;
			}

			// Restore state
			SetParseState(stateAtBeginRule, dueToFailure:false);
			_stack.RemoveAt (_stack.Count - 1);

			if (result == null) {
				result = ParseSuccess;
			}

			return result;
		}

		protected void SetParseState(InkStateElement state, bool dueToFailure)
		{
			// Rewind on failure
			if( dueToFailure && state != null ) {
				index = state.index;
				lineIndex = state.lineIndex;
			}

//			if( state != null ) {
//				_currentContentDepth = state.contentDepth;
//			} else {
//				_currentContentDepth = InkContentDepthGlobal;
//			}
		}

		// Main entry point
		public Parsed.Story Parse()
		{
			List<object> topLevelContent = Interleave (Optional(MultilineWhitespace), 
					    							   TopLevelStatement);

			Parsed.Story story = new Parsed.Story (topLevelContent);
			return story;
		}

		protected object TopLevelStatement()
		{
			var statement = OneOf (KnotDefinition, TextContent);

			Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

			return statement;
		}

		protected object SkipToNextLine()
		{
			ParseUntilCharactersFromString ("\n\r");
			Newline ();
			return ParseSuccess;
		}

		protected Parsed.Text TextContent()
		{
			BeginRule ();

			Whitespace ();

			Parsed.Text text = SimpleText ();
			if (text == null) {
				return FailRule () as Parsed.Text;
			}

			Whitespace ();

			return (Parsed.Text) SucceedRule (text);
		}

		protected string Identifier()
		{
			if (_identifierCharSet == null) {

				_identifierCharSet = new CharacterSet();
				_identifierCharSet.AddRange ('A', 'Z');
				_identifierCharSet.AddRange ('a', 'z');
				_identifierCharSet.AddRange ('0', '9');
				_identifierCharSet.Add ('_');
			}

			return ParseCharactersFromCharSet (_identifierCharSet);
		}

		private CharacterSet _identifierCharSet;


		protected Parsed.Text SimpleText()
		{
			if (_simpleTextCharSet == null) {

				_simpleTextCharSet = new CharacterSet();
				_simpleTextCharSet.AddRange ('A', 'Z');
				_simpleTextCharSet.AddRange ('a', 'z');
				_simpleTextCharSet.AddRange ('0', '9');
				_simpleTextCharSet.AddStringCharacters (".? ");
			}

			string parsedText = ParseCharactersFromCharSet (_simpleTextCharSet);
			if (parsedText != null) {

				parsedText = parsedText.TrimEnd(' ');
				if (parsedText.Length > 0) {
					return new Parsed.Text (parsedText);
				}
			}
				
			return null;
		}

		private CharacterSet _simpleTextCharSet;


		private List<InkStateElement> _stack;
	}
}

