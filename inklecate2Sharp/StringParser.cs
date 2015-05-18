using System;
using System.Collections.Generic;

namespace inklecate2Sharp
{
	public abstract class StringParser
	{
		public delegate object ParseRule();

		protected StringParser (string str)
		{
			_chars = str.ToCharArray ();
			
			inputString = str;
		}

		abstract public void Parse ();

		//--------------------------------
		// Parse state
		//--------------------------------

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

		public bool endOfInput
		{
			get { return _index >= _chars.Length; }
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

		public string inputString { get; }

		//--------------------------------
		// Structuring
		//--------------------------------

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

		//--------------------------------
		// Basic string parsing
		//--------------------------------

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

		public string ParseUntilCharactersFromString(string str)
		{
			return ParseCharactersFromString(str, false);
		}

		public string ParseUntilCharactersFromCharSet(CharacterSet charSet)
		{
			return ParseCharactersFromCharSet(charSet, false);
		}

		public string ParseCharactersFromString(string str)
		{
			return ParseCharactersFromString(str, true);
		}

		public string ParseCharactersFromString(string str, bool shouldIncludeStrChars)
		{
			return ParseCharactersFromCharSet (new CharacterSet(str), shouldIncludeStrChars);
		}

		protected string ParseCharactersFromCharSet(CharacterSet charSet, bool shouldIncludeChars = true)
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
			

		private char[] _chars;
		private int _index;
		private int _lineIndex;
	}
}

