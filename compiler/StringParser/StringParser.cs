using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Text;

namespace Ink
{
	public class StringParser
	{
		public delegate object ParseRule();

        public delegate T SpecificParseRule<T>() where T : class;

        public delegate void ErrorHandler(string message, int index, int lineIndex, bool isWarning);

		public StringParser (string str)
		{
            str = PreProcessInputString (str);

            state = new StringParserState();

            if (str != null) {
                _chars = str.ToCharArray ();
            } else {
                _chars = new char[0];
            }

			inputString = str;
		}

		public class ParseSuccessStruct {};
		public static ParseSuccessStruct ParseSuccess = new ParseSuccessStruct();

		public static CharacterSet numbersCharacterSet = new CharacterSet("0123456789");

        protected ErrorHandler errorHandler { get; set; }

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

		public StringParserState state { get; private set; }

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

        protected int BeginRule()
        {
            return state.Push ();
        }

        protected object FailRule(int expectedRuleId)
        {
            state.Pop (expectedRuleId);
            return null;
        }

        protected void CancelRule(int expectedRuleId)
        {
            state.Pop (expectedRuleId);
        }

        protected object SucceedRule(int expectedRuleId, object result = null)
        {
            // Get state at point where this rule stared evaluating
            var stateAtSucceedRule = state.Peek(expectedRuleId);
            var stateAtBeginRule = state.PeekPenultimate ();


            // Allow subclass to receive callback
            RuleDidSucceed (result, stateAtBeginRule, stateAtSucceedRule);

            // Flatten state stack so that we maintain the same values,
            // but remove one level in the stack.
            state.Squash();

            if (result == null) {
                result = ParseSuccess;
            }

            return result;
        }

        protected virtual void RuleDidSucceed(object result, StringParserState.Element startState, StringParserState.Element endState)
        {

        }

        protected object Expect(ParseRule rule, string message = null, ParseRule recoveryRule = null)
		{
            object result = ParseObject(rule);
			if (result == null) {
				if (message == null) {
                    message = rule.Method.Name;
				}

                string butSaw;
                string lineRemainder = LineRemainder ();
                if (lineRemainder == null || lineRemainder.Length == 0) {
                    butSaw = "end of line";
                } else {
                    butSaw = "'" + lineRemainder + "'";
                }

                Error ("Expected "+message+" but saw "+butSaw);

				if (recoveryRule != null) {
					result = recoveryRule ();
				}
			}
			return result;
		}

        protected void Error(string message, bool isWarning = false)
		{
            ErrorOnLine (message, lineIndex + 1, isWarning);
		}

        protected void ErrorWithParsedObject(string message, Parsed.Object result, bool isWarning = false)
        {
            ErrorOnLine (message, result.debugMetadata.startLineNumber, isWarning);
        }

        protected void ErrorOnLine(string message, int lineNumber, bool isWarning)
        {
            if ( !state.errorReportedAlreadyInScope ) {

                var errorType = isWarning ? "Warning" : "Error";

                if (errorHandler == null) {
                    throw new System.Exception (errorType+" on line " + lineNumber + ": " + message);
                } else {
                    errorHandler (message, index, lineNumber-1, isWarning);
                }

                state.NoteErrorReported ();
            }

            if( !isWarning )
                hadError = true;
        }

        protected void Warning(string message)
        {
            Error(message, isWarning:true);
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

        public string LineRemainder()
		{
            return (string) Peek (() => ParseUntilCharactersFromString ("\n\r"));
		}

		public int remainingLength
		{
			get {
				return _chars.Length - index;
			}
		}

		public string inputString { get; private set; }


        public int lineIndex
        {
            set {
                state.lineIndex = value;
            }
            get {
                return state.lineIndex;
            }
        }

        public int characterInLineIndex
        {
            set {
                state.characterInLineIndex = value;
            }
            get {
                return state.characterInLineIndex;
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

        public void SetFlag(uint flag, bool trueOrFalse) {
            if (trueOrFalse) {
                state.customFlags |= flag;
            } else {
                state.customFlags &= ~flag;
            }
        }

        public bool GetFlag(uint flag) {
            return (state.customFlags & flag) != 0;
        }

		//--------------------------------
		// Structuring
		//--------------------------------

        public object ParseObject(ParseRule rule)
        {
            int ruleId = BeginRule ();

            var stackHeightBefore = state.stackHeight;

            var result = rule ();

            if (stackHeightBefore != state.stackHeight) {
                throw new System.Exception ("Mismatched Begin/Fail/Succeed rules");
            }

            if (result == null)
                return FailRule (ruleId);

            SucceedRule (ruleId, result);
            return result;
        }

        public T Parse<T>(SpecificParseRule<T> rule) where T : class
        {
            int ruleId = BeginRule ();

            var result = rule () as T;
            if (result == null) {
                FailRule (ruleId);
                return null;
            }

            SucceedRule (ruleId, result);
            return result;
        }

		public object OneOf(params ParseRule[] array)
		{
			foreach (ParseRule rule in array) {
                object result = ParseObject(rule);
				if (result != null)
                    return result;
			}

			return null;
		}

		public List<object> OneOrMore(ParseRule rule)
		{
			var results = new List<object> ();

			object result = null;
			do {
                result = ParseObject(rule);
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
                object result = ParseObject(rule);
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
                object result = ParseObject(rule);
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
                ParseObject(rule);
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
            int ruleId = BeginRule ();

			var results = new List<T> ();

			// First outer padding
            var firstA = ParseObject(ruleA);
			if (firstA == null) {
                return (List<T>) FailRule(ruleId);
			} else {
				TryAddResultToList(firstA, results, flatten);
			}

			object lastMainResult = null, outerResult = null;
			do {

				// "until" condition hit?
				if( untilTerminator != null && Peek(untilTerminator) != null ) {
					break;
				}

				// Main inner
                lastMainResult = ParseObject(ruleB);
				if( lastMainResult == null ) {
					break;
				} else {
					TryAddResultToList(lastMainResult, results, flatten);
				}

				// Outer result (i.e. last A in ABA)
				outerResult = null;
				if( lastMainResult != null ) {
                    outerResult = ParseObject(ruleA);
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
                return (List<T>) FailRule(ruleId);
			}

            return (List<T>) SucceedRule(ruleId, results);
		}

		//--------------------------------
		// Basic string parsing
		//--------------------------------

		public string ParseString(string str)
		{
			if (str.Length > remainingLength) {
				return null;
			}

            int ruleId = BeginRule ();

            // Optimisation from profiling:
            // Store in temporary local variables
            // since they're properties that would have to access
            // the rule stack every time otherwise.
            int i = index;
            int cli = characterInLineIndex;
            int li = lineIndex;

			bool success = true;
			foreach (char c in str) {
				if ( _chars[i] != c) {
					success = false;
					break;
				}
                if (c == '\n') {
                    li++;
                    cli = -1;
                }
				i++;
                cli++;
			}

            index = i;
            characterInLineIndex = cli;
            lineIndex = li;

			if (success) {
                return (string) SucceedRule(ruleId, str);
			}
			else {
                return (string) FailRule (ruleId);
			}
		}

        public char ParseSingleCharacter()
        {
            if (remainingLength > 0) {
                char c = _chars [index];
                if (c == '\n') {
                    lineIndex++;
                    characterInLineIndex = -1;
                }
                index++;
                characterInLineIndex++;
                return c;
            } else {
                return (char)0;
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

            // Optimisation from profiling:
            // Store in temporary local variables
            // since they're properties that would have to access
            // the rule stack every time otherwise.
            int i = index;
            int cli = characterInLineIndex;
            int li = lineIndex;

			int count = 0;
            while ( i < _chars.Length && charSet.Contains (_chars [i]) == shouldIncludeChars && count < maxCount ) {
                if (_chars [i] == '\n') {
                    li++;
                    cli = -1;
                }
                i++;
                cli++;
				count++;
			}

            index = i;
            characterInLineIndex = cli;
            lineIndex = li;

			int lastCharIndex = index;
			if (lastCharIndex > startIndex) {
				return new string (_chars, startIndex, index - startIndex);
			} else {
				return null;
			}
		}

		public object Peek(ParseRule rule)
		{
			int ruleId = BeginRule ();
			object result = rule ();
            CancelRule (ruleId);
			return result;
		}

		public string ParseUntil(ParseRule stopRule, CharacterSet pauseCharacters = null, CharacterSet endCharacters = null)
		{
			int ruleId = BeginRule ();


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
                            characterInLineIndex = -1;
                        }
						index++;
                        characterInLineIndex++;
						continue;
					} else {
						break;
					}
				}

			} while(true);

			if (parsedString.Length > 0) {
                return (string) SucceedRule (ruleId, parsedString.ToString ());
			} else {
                return (string) FailRule (ruleId);
			}

		}

        // No need to Begin/End rule since we never parse a newline, so keeping oldIndex is good enough
		public int? ParseInt()
		{
			int oldIndex = index;
            int oldCharacterInLineIndex = characterInLineIndex;

			bool negative = ParseString ("-") != null;

			// Optional whitespace
			ParseCharactersFromString (" \t");

			var parsedString = ParseCharactersFromCharSet (numbersCharacterSet);
            if(parsedString == null) {
                // Roll back and fail
                index = oldIndex;
                characterInLineIndex = oldCharacterInLineIndex;
                return null;
            }

			int parsedInt;
			if (int.TryParse (parsedString, out parsedInt)) {
				return negative ? -parsedInt : parsedInt;
			}

            Error("Failed to read integer value: " + parsedString + ". Perhaps it's out of the range of acceptable numbers ink supports? (" + int.MinValue + " to " + int.MaxValue + ")");
            return null;
        }

        // No need to Begin/End rule since we never parse a newline, so keeping oldIndex is good enough
        public float? ParseFloat()
        {
            int oldIndex = index;
            int oldCharacterInLineIndex = characterInLineIndex;

            int? leadingInt = ParseInt ();
            if (leadingInt != null) {

                if (ParseString (".") != null) {

                    var afterDecimalPointStr = ParseCharactersFromCharSet (numbersCharacterSet);
                    return float.Parse (leadingInt+"." + afterDecimalPointStr, System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            // Roll back and fail
            index = oldIndex;
            characterInLineIndex = oldCharacterInLineIndex;
            return null;
        }

        // You probably want "endOfLine", since it handles endOfFile too.
        protected string ParseNewline()
        {
            int ruleId = BeginRule();

            // Optional \r, definite \n to support Windows (\r\n) and Mac/Unix (\n)
            // 2nd May 2016: Always collapse \r\n to just \n
            ParseString ("\r");

            if( ParseString ("\n") == null ) {
                return (string) FailRule(ruleId);
            } else {
                return (string) SucceedRule(ruleId, "\n");
            }
        }

		private char[] _chars;
	}
}

