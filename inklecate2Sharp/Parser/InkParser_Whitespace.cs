using System;
using System.Collections.Generic;

namespace Inklewriter
{
	public partial class InkParser
	{
		// Handles both newline and endOfFile
		protected object EndOfLine()
		{
			BeginRule();

			object newlineOrEndOfFile = OneOf(Newline, EndOfFile);
			if( newlineOrEndOfFile == null ) {
				return FailRule();
			} else {
				return SucceedRule(newlineOrEndOfFile);
			}
		}

		// You probably want "endOfLine", since it handles endOfFile too.
		protected object Newline()
		{
			BeginRule();

			Whitespace();

			// Optional \r, definite \n to support Windows (\r\n) and Mac/Unix (\n)
			ParseString ("\r");
			bool gotNewline = ParseString ("\n") != null;

			if( !gotNewline ) {
				return FailRule();
			} else {
				IncrementLine();
				return SucceedRule(ParseSuccess);
			}
		}

		protected object EndOfFile()
		{
			BeginRule();

			Whitespace();

			if( endOfInput ) {
				return SucceedRule();
			} else {
				return FailRule();
			}
		}


		// General purpose space, returns N-count newlines (fails if no newlines)
		protected object MultilineWhitespace()
		{
			BeginRule();

			List<object> newlines = OneOrMore(Newline);
			if( newlines == null ) {
				return FailRule();
			}

			// Use content field of Token to say how many newlines there were
			// (in most circumstances it's unimportant)
			int numNewlines = newlines.Count;
			if (numNewlines >= 1) {
				return SucceedRule ();
			} else {
				return FailRule ();
			}

		}

		protected object Whitespace()
		{
			if( ParseCharactersFromCharSet(_inlineWhitespaceChars) != null ) {
				return ParseSuccess;
			}

			return null;
		}

		private CharacterSet _inlineWhitespaceChars = new CharacterSet(" \t");
	}
}

