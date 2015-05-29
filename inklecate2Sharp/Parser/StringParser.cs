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

            state = new StringParserState();

			_chars = str.ToCharArray ();
			inputString = str;
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

        public StringParserState state { get; }

        public bool hadError { get; protected set; }

        // Don't do anything by default, but provide ability for subclasses
        // to manipulate the string before it's used as input (converted to a char array)
        protected virtual string PreProcessInputString(string str)
        {
            return str;
        }

		//--------------------------------
		// Parse state
		//--------------------------------

        protected void BeginRule()
        {
            state.Push ();
        }

        protected object FailRule()
        {
            state.Pop ();
            return null;
        }

        protected void CancelRule()
        {
            state.Pop ();
        }

        protected object SucceedRule(object result = null)
        {
            // Get state at point where this rule stared evaluating
            var stateAtBeginRule = state.Peek ();

            // Allow subclass to receive callback
            RuleDidSucceed (result, stateAtBeginRule);

            // Flatten state stack so that we maintain the same values,
            // but remove one level in the stack.
            state.Squash();

            if (result == null) {
                result = ParseSuccess;
            }

            return result;
        }

        protected virtual void RuleDidSucceed(object result, StringParserState.Element state)
        {

        }
            
		protected object Expect(ParseRule rule, string message = null, ParseRule recoveryRule = null)
		{
			object result = rule ();
			if (result == null) {
				if (message == null) {
					message = rule.GetMethodInfo ().Name;
				}

				Error ("Expected "+message+" but saw '"+LimitedRemainingString(30)+"'");

				if (recoveryRule != null) {
					result = recoveryRule ();
				}
			}
			return result;
		}

		protected void Error(string message)
		{
			// TODO: Do something more sensible than this. Probably don't assert though?
            Console.WriteLine ("Error on line " + (lineIndex+1) + ": " + message);
            hadError = true;
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


        public int lineIndex
        {
            set {
                state.lineIndex = value;
            }
            get {
                return state.lineIndex;
            }
        }

        public int index
        {
            // If we want subclass parsers to be able to set the index directly,
            // then we would need to know what the lineIndex of the new
            // index would be - would we have to step through manually
            // counting the newlines to do so?
            private set {
                state.characterIndex = value;
            }
            get {
                return state.characterIndex;
            }
        }

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

        // Combination of both of the above
        public ParseRule OptionalExclude(ParseRule rule)
        {
            return () => {
                rule ();
                return ParseSuccess;
            };
        }

        // Convenience method for creating more readable ParseString rules that can be combined
        // in other structuring rules (like OneOf etc)
        // e.g. OneOf(String("one"), String("two"))
        protected ParseRule String(string str)
        {
            return () => ParseString (str);
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

            BeginRule ();

			bool success = true;
			foreach (char c in str) {
				if ( _chars[index] != c) {
					success = false;
					break;
				}
                if (c == '\n') {
                    lineIndex++;
                }
				index++;
			}

			if (success) {
                return (string) SucceedRule(str);
			}
			else {
                return (string) FailRule ();
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
                if (_chars [index] == '\n') {
                    lineIndex++;
                }
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
                        if( pauseCharacter == '\n' ) {
                            lineIndex++;
                        }
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

        // You probably want "endOfLine", since it handles endOfFile too.
        protected string ParseNewline()
        {
            BeginRule();

            // Optional \r, definite \n to support Windows (\r\n) and Mac/Unix (\n)
            var r = ParseString ("\r");
            var n = ParseString ("\n");

            if (r != null && n != null) {
                n = r + n;
            }

            if( n == null ) {
                return (string) FailRule();
            } else {
                return (string) SucceedRule(n);
            }
        }

		private char[] _chars;
	}
}

