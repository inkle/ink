using System;
using System.Collections.Generic;
using inklecate2Sharp.Parsed;

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

		public Parsed.DebugMetadata CreateDebugMetadata()
		{
			var md = new Parsed.DebugMetadata ();
			md.lineNumber = lineIndex + 1;
			return md;
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


	}
}

