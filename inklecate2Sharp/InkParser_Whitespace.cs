using System;
using System.Collections.Generic;

namespace inklecate2Sharp
{
	public partial class InkParser
	{
		// Automatically includes end of line comments due to newline
		// Handles both newline and endOfFile
		protected object EndOfLine()
		{
			BeginRule();

			object newlineOrEndOfFile = OneOf(Newline, EndOfFile);
			if( newlineOrEndOfFile != null ) {
				return FailRule();
			} else {
				return SucceedRule(newlineOrEndOfFile);
			}
		}

		// Automatically includes end of line comments
		// However, you probably want "endOfLine", since it handles endOfFile too.
		protected object Newline()
		{
			BeginRule();

			// Optional whitespace and comment
			Whitespace();
			SingleLineComment();


			// Optional \r, definite \n to support Windows (\r\n) and Mac/Unix (\n)
			ParseString ("\r");
			bool gotNewline = ParseString ("\n") != null;

			if( !gotNewline ) {
				return FailRule();
			} else {
				IncrementLine();
				return SucceedRule(true);
			}
		}

		protected object EndOfFile()
		{
			BeginRule();

			// Optional whitespace and comment
			Whitespace();
			SingleLineComment();

			if( !endOfInput ) {
				return SucceedRule(true);
			} else {
				return FailRule();
			}
		}

		// You shouldn't need this in main rules since it's included in endOfLine
		protected object SingleLineComment()
		{
			if( ParseString("//") == null ) {
				return null;
			}

			ParseUntilCharactersFromCharSet(_newlineChars);

			return true;
		}

		// General purpose space, returns N-count newlines (fails if no newlines)
		protected object MultilineWhitespace()
		{
			BeginRule();

			List<object> newlines = OneOrMore(Newline);
			if( newlines != null ) {
				return FailRule();
			}

			// Use content field of Token to say how many newlines there were
			// (in most circumstances it's unimportant)
			int numNewlines = newlines.Count;
			if (numNewlines >= 1) {
				return SucceedRule (true);
			} else {
				return FailRule ();
			}

		}

		protected object Whitespace()
		{
			if( ParseCharactersFromCharSet(_inlineWhitespaceChars) != null ) {
				return true;
			}

			return null;
		}

		private CharacterSet _inlineWhitespaceChars = new CharacterSet(" \t");
		private CharacterSet _newlineChars = new CharacterSet("\n\r");
	}
}

