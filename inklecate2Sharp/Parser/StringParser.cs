using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Text;

namespace Inklewriter
{
	public class StringParser
	{
		public delegate object ParseRule();
		
		public StringParser (string str)
		{
            str = PreProcessInputString (str);

			_chars = str.ToCharArray ();
			inputString = str;

            _indexStack = new List<int> ();

            // Default element
            _indexStack.Add (0);
		}
            
		public class ParseSuccessStruct {};
		public static ParseSuccessStruct ParseSuccess = new ParseSuccessStruct();

		public static CharacterSet numbersCharacterSet = new CharacterSet("0123456789");

		public char currentCharacter
		{
			get 
			{
				if (index >= 0 && remainingLength > 0) {
					return _chars [index];
				} else {
					return (char)0;
				}
			}
		}

        // Don't do anything by default, but provide ability for subclasses
        // to manipulate the string before it's used as input (converted to a char array)
        protected virtual string PreProcessInputString(string str)
        {
            return str;
        }

		//--------------------------------
		// Parse state
		//--------------------------------

		protected virtual void BeginRule()
		{
            _indexStack.Add (index);
		}

		protected virtual object FailRule()
		{
            _indexStack.RemoveAt (_indexStack.Count - 1);
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

            var currIndex = index;
            _indexStack.RemoveRange (_indexStack.Count - 2, 2);
            _indexStack.Add (currIndex);

			return result;
		}

		protected object Expect(ParseRule rule, string message = null, ParseRule recoveryRule = null)
		{
			object result = rule ();
			if (result == null) {
				if (message == null) {
					message = rule.GetMethodInfo ().Name;
				}

				Error ("Expected "+message+" on line "+(lineIndex+1)+" but saw '"+LimitedRemainingString(30)+"'");

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

		public string LimitedRemainingString(int limit)
		{
			return new string (_chars, index, Math.Min (limit, remainingLength));
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
        public virtual int index { get { return _indexStack[_indexStack.Count-1]; } set { _indexStack[_indexStack.Count-1] = value; } }

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

        // Return ParseSuccess instead the real result so that it gets excluded
        // from result arrays (e.g. Interleave)
        public ParseRule Exclude(ParseRule rule)
        {
            return () => {
                object result = rule ();
                if( result == null ) {
                    return null;
                }
                return ParseSuccess;
            };
        }

		private void TryAddResultToList<T>(object result, List<T> list, bool flatten = true)
		{
			if (result == ParseSuccess) {
				return;
			}

			if (flatten) {
				var resultCollection = result as System.Collections.ICollection;
				if (resultCollection != null) {
					foreach (object obj in resultCollection) {
						Debug.Assert (obj is T);
						list.Add ((T)obj);
					}
					return;
				} 
			}

			Debug.Assert (result is T);
			list.Add ((T)result);
		}

		public List<T> Interleave<T>(ParseRule ruleA, ParseRule ruleB, ParseRule untilTerminator = null, bool flatten = true)
		{
			var results = new List<T> ();

			// First outer padding
			var firstA = ruleA();
			if (firstA == null) {
				return null;
			} else {
				TryAddResultToList(firstA, results, flatten);
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
				} else {
					TryAddResultToList(lastMainResult, results, flatten);
				}

				// Outer result (i.e. last A in ABA)
				outerResult = null;
				if( lastMainResult != null ) {
					outerResult = ruleA();
					if (outerResult == null) {
						break;
					} else {
						TryAddResultToList(outerResult, results, flatten);
					}
				}

			// Stop if there are no results, or if both are the placeholder "ParseSuccess" (i.e. Optional success rather than a true value)
			} while((lastMainResult != null || outerResult != null) 
				 && !(lastMainResult == ParseSuccess && outerResult == ParseSuccess) && remainingLength > 0);

			if (results.Count == 0) {
				return null;
			}

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

		public string ParseUntilCharactersFromString(string str, int maxCount = -1)
		{
			return ParseCharactersFromString(str, false, maxCount);
		}

		public string ParseUntilCharactersFromCharSet(CharacterSet charSet, int maxCount = -1)
		{
			return ParseCharactersFromCharSet(charSet, false, maxCount);
		}

		public string ParseCharactersFromString(string str, int maxCount = -1)
		{
			return ParseCharactersFromString(str, true, maxCount);
		}

		public string ParseCharactersFromString(string str, bool shouldIncludeStrChars, int maxCount = -1)
		{
			return ParseCharactersFromCharSet (new CharacterSet(str), shouldIncludeStrChars);
		}

		public string ParseCharactersFromCharSet(CharacterSet charSet, bool shouldIncludeChars = true, int maxCount = -1)
		{
			if (maxCount == -1) {
				maxCount = int.MaxValue;
			}

			int startIndex = index;

			int count = 0;
			while ( index < _chars.Length && charSet.Contains (_chars [index]) == shouldIncludeChars && count < maxCount ) {
				index++;
				count++;
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

			if (parsedString.Length > 0) {
				return SucceedRule (parsedString.ToString ()) as string;
			} else {
				return FailRule () as string;
			}

		}

		public int? ParseInt()
		{
			int oldIndex = index;

			bool negative = ParseString ("-") != null;

			// Optional whitespace
			ParseCharactersFromString (" \t");

			var parsedString = ParseCharactersFromCharSet (numbersCharacterSet);
			int parsedInt;
			if (int.TryParse (parsedString, out parsedInt)) {
				return negative ? -parsedInt : parsedInt;
			}

			// Roll back and fail
			index = oldIndex;
			return null;
		}

		private char[] _chars;

		// WARNING: These are invalid in InkParser since the index and lineIndex properties are overridden
		private int _lineIndex;
        private List<int> _indexStack;

	}
}

