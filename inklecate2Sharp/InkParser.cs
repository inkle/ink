using System;
using System.Collections.Generic;

namespace inklecate2Sharp
{
	public partial class InkParser : StringParser
	{
		public InkParser(string str) : base(str) { }

		protected class InkStateElement : StateElement {
			public int index;
			public int lineIndex;
		}

		protected override StateElement parseState
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

		protected override void SetParseState(StateElement s, bool dueToFailure)
		{
			InkStateElement state = s as InkStateElement;

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
		public void Parse()
		{
			List<object> topLevelContent = Interleave (Optional(MultilineWhitespace), 
					    							   TopLevelStatement);

			Console.WriteLine (topLevelContent);
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

		protected string TextContent()
		{
			BeginRule ();

			Whitespace ();

			string text = SimpleText ();
			if (text == null) {
				return FailRule () as string;
			}

			Whitespace ();

			return (string) SucceedRule (text);
		}

		protected object KnotDefinition()
		{
			BeginRule ();

			Whitespace ();

			if (ParseString ("§") == null) {
				return FailRule ();
			}

			Whitespace ();

			string knotName = Identifier ();
			if (knotName == null) {
				return FailRule ();
			}

			return SucceedRule (knotName);
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


		protected string SimpleText()
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
					return parsedText;
				}
			}
				
			return null;
		}

		private CharacterSet _simpleTextCharSet;


	}
}

