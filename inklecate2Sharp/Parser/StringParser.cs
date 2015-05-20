using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Inklewriter
{
	public class StringParser
	{
		public delegate object ParseRule();
		
		public StringParser (string str)
		{
			_chars = str.ToCharArray ();
			inputString = str;
		}
			
		public class ParseSuccessStruct {};
		public static ParseSuccessStruct ParseSuccess = new ParseSuccessStruct();

		public char currentCharacter
		{
			get 
			{
				if (index >= 0 && remainingLength <= 0) {
					return _chars [index];
				} else {
					return (char)0;
				}
			}
		}

		//--------------------------------
		// Parse state
		//--------------------------------

		protected virtual void BeginRule()
		{
			
		}

		protected virtual object FailRule()
		{
			return null;
		}

		protected virtual void CancelRule()
		{
			FailRule ();
		}

		protected virtual object SucceedRule(object result = null)
		{
			if (result == null) {
				result = ParseSuccess;
			}

			return result;
		}

		protected object Expect(ParseRule rule, string message = null, ParseRule recoveryRule = null)
		{
			object result = rule ();
			if (result == null) {
				if (message == null) {
					message = rule.GetMethodInfo ().Name;
				}

				Error ("Expected "+message+" on line "+(lineIndex+1));

				if (recoveryRule != null) {
					result = recoveryRule ();
				}
			}
			return result;
		}

		protected void Error(string message)
		{
			// TODO: Do something more sensible than this. Probably don't assert though?
			Console.WriteLine ("ERROR: " + message);
		}

		protected void IncrementLine()
		{
			lineIndex++;
		}

		public bool endOfInput
		{
			get { return index >= _chars.Length; }
		}

		public string remainingString
		{
			get {
				return new string(_chars, index, remainingLength);
			}
		}

		public int remainingLength
		{
			get {
				return _chars.Length - index;
			}
		}

		public string inputString { get; }

		// These are overriden to use the InkParserState values in InkParser
		public virtual int lineIndex { get { return _lineIndex; } set { _lineIndex = value; } }
		public virtual int index { get { return _index; } set { _index = value; } }

		//--------------------------------
		// Structuring
		//--------------------------------

		public object OneOf(params ParseRule[] array)
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

		public List<object> OneOrMore(ParseRule rule)
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

		public ParseRule Optional(ParseRule rule)
		{
			return () => {
				object result = rule ();
				if( result == null ) {
					result = ParseSuccess;
				}
				return result;
			};
		}

		public List<object> Interleave(ParseRule ruleA, ParseRule ruleB, ParseRule untilTerminator = null)
		{
			var results = new List<object> ();

			// First outer padding
			var firstA = ruleA();
			if (firstA == null) {
				return null;
			} else if (firstA != ParseSuccess) {
				results.Add (firstA);
			}

			object lastMainResult = null, outerResult = null;
			do {

				// "until" condition hit?
				// TODO: Do this
//				if( untilTerminator != null && LookaheadParseRule(untilTerminator) ) {
//					break;
//				}

				// Main inner
				lastMainResult = ruleB();
				if( lastMainResult == null ) {
					break;
				} else if( lastMainResult != ParseSuccess ) {
					results.Add(lastMainResult);
				}

				// Outer result (i.e. last A in ABA)
				outerResult = null;
				if( lastMainResult != null ) {
					outerResult = ruleA();
					if (outerResult == null) {
						break;
					} else if (outerResult != ParseSuccess) {
						results.Add (outerResult);
					}
				}

			} while((lastMainResult != null || outerResult != null) && remainingLength > 0);

			return results;
		}

		//--------------------------------
		// Basic string parsing
		//--------------------------------

		public string ParseString(string str)
		{
			if (str.Length > remainingLength) {
				return null;
			}

			int oldIndex = index;

			bool success = true;
			foreach (char c in str) {
				if ( _chars[index] != c) {
					success = false;
					break;
				}
				index++;
			}

			if (success) {
				return str;
			}
			else {
				index = oldIndex;
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

		public string ParseCharactersFromCharSet(CharacterSet charSet, bool shouldIncludeChars = true)
		{
			int startIndex = index;

			while ( index < _chars.Length && charSet.Contains (_chars [index]) == shouldIncludeChars) {
				index++;
			}

			int lastCharIndex = index;
			if (lastCharIndex > startIndex) {
				return new string (_chars, startIndex, index - startIndex);
			} else {
				return null;
			}
		}

		public object Peek(ParseRule rule)
		{
			BeginRule ();
			object result = rule ();
			CancelRule ();
			return result;
		}

		public string ParseUntil(ParseRule stopRule, CharacterSet pauseCharacters = null, CharacterSet endCharacters = null)
		{
			BeginRule ();

			
			CharacterSet pauseAndEnd = new CharacterSet ();
			if (pauseCharacters != null) {
				pauseAndEnd.UnionWith (pauseCharacters);
			}
			if (endCharacters != null) {
				pauseAndEnd.UnionWith (endCharacters);
			}

			StringBuilder parsedString = new StringBuilder ();
			object ruleResultAtPause = null;

			// Keep attempting to parse strings up to the pause (and end) points.
			//  - At each of the pause points, attempt to parse according to the rule
			//  - When the end point is reached (or EOF), we're done
			do {

				// TODO: Perhaps if no pause or end characters are passed, we should check *every* character for stopRule?
				string partialParsedString = ParseUntilCharactersFromCharSet(pauseAndEnd);
				if( partialParsedString != null ) {
					parsedString.Append(partialParsedString);
				}

				// Attempt to run the parse rule at this pause point
				ruleResultAtPause = Peek(stopRule);

				// Rule completed - we're done
				if( ruleResultAtPause != null ) {
					break;
				} else {

					if( endOfInput ) {
						break;
					}

					// Reached a pause point, but rule failed. Step past and continue parsing string
					char pauseCharacter = currentCharacter;
					if( pauseCharacters != null && pauseCharacters.Contains(pauseCharacter) ) {
						parsedString.Append(pauseCharacter);
						index++;
						continue;
					} else {
						break;
					}
				}

			} while(true);

			return SucceedRule(parsedString.ToString()) as string;
		}

			

		private char[] _chars;

		// WARNING: These are invalid in InkParser since the index and lineIndex properties are overridden
		private int _index;
		private int _lineIndex;
	}
}

