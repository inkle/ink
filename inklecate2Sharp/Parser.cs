using System;
using System.Collections.Generic;

namespace inklecate2Sharp
{
	public class Parser
	{
		public string inputString { get; }

		private char[] _chars;
		private int _index;
		private int _lineIndex;

		public Parser (string str)
		{
			_chars = str.ToCharArray ();
			
			inputString = str;
		}

		public void Parse()
		{
			MultilineWhitespace();

			ParseString ("§");

			Whitespace ();

			string identifier = Identifier ();

			System.Console.WriteLine ("Knot id: " + identifier);
		}


		protected void BeginRule()
		{
		}

		protected object FailRule()
		{
			return null;
		}

		protected object SucceedRule(object result)
		{
			return result;
		}

		protected void IncrementLine()
		{
			_lineIndex++;
		}

		public delegate object ParseRule();

		protected object OneOf(params ParseRule[] array)
		{
			foreach (ParseRule rule in array) {
				BeginRule ();

				object result = rule ();
				if (result != null) {
					return SucceedRule (result);
				} else {
					FailRule ();
				}
			}

			return null;
		}

		protected List<object> OneOrMore(ParseRule rule)
		{
			var results = new List<object> ();

			object result = null;
			do {
				result = rule();
				if( result != null ) {
					results.Add(result);
				}
			} while(result != null);

			if (results.Count > 0) {
				return results;
			} else {
				return null;
			}
		}


		private HashSet<char> _identifierCharSet;


		protected char[] CharsBetween(char start, char end)
		{
			var chars = new char[end - start + 1];
			for(char c=start; c<=end; ++c) {
				chars [c - start] = c;
			}
			return chars;
		}

		protected string Identifier()
		{
			if (_identifierCharSet == null) {

				HashSet<char> charSet = new HashSet<char> ();

				charSet.UnionWith (CharsBetween('A', 'Z'));
				charSet.UnionWith (CharsBetween('a', 'z'));
				charSet.UnionWith (CharsBetween('0', '9'));
				charSet.Add ('_');

				_identifierCharSet = charSet;
			}

			return ParseCharactersFromCharSet (_identifierCharSet);
		}


		//------------------------------------------------------------------------------------
		// Newlines, whitespace and comments
		//------------------------------------------------------------------------------------

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

		protected bool EndOfInput()
		{
			return _index >= _chars.Length;
		}

		protected object EndOfFile()
		{
			BeginRule();

			// Optional whitespace and comment
			Whitespace();
			SingleLineComment();

			if( !EndOfInput() ) {
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

			ParseUntilCharactersFromString ("\n\r");

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
			const string whitespaceCharacters = " \t";

			if( ParseCharactersFromString(whitespaceCharacters) != null ) {
				return true;
			}
				
			return null;
		}

		protected object OptionalWhitespace()
		{
			return Whitespace();
		}
			
		public object ParseString(string str)
		{
			if (str.Length > remainingLength) {
				return null;
			}

			int oldIndex = _index;

			bool success = true;
			foreach (char c in str) {
				if ( _chars[_index] != c) {
					success = false;
					break;
				}
				_index++;
			}

			if (success) {
				return str;
			}
			else {
				_index = oldIndex;
				return null;
			}
		}

		public object ParseUntilCharactersFromString(string str)
		{
			return ParseCharactersFromString(str, false);
		}

		public object ParseCharactersFromString(string str)
		{
			return ParseCharactersFromString(str, true);
		}

		public string ParseCharactersFromString(string str, bool shouldIncludeStrChars)
		{
			return ParseCharactersFromCharSet (CharSetFromString (str), shouldIncludeStrChars);
		}

		protected HashSet<char> CharSetFromString(string str)
		{
			var charSet = new HashSet<char> ();
			foreach (char c in str) {
				charSet.Add (c);
			}
			return charSet;
		}

		protected string ParseCharactersFromCharSet(HashSet<char> charSet, bool shouldIncludeChars = true)
		{
			int startIndex = _index;

			while ( _index < _chars.Length && charSet.Contains (_chars [_index]) == shouldIncludeChars) {
				_index++;
			}

			int lastCharIndex = _index;
			if (lastCharIndex > startIndex) {
				return new string (_chars, startIndex, _index - startIndex);
			} else {
				return null;
			}
		}

		public string remainingString
		{
			get {
				return new string(_chars, _index, remainingLength);
			}
		}

		public int remainingLength
		{
			get {
				return _chars.Length - _index;
			}
		}
	}
}

