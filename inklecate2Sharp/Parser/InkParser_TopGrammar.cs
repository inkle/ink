
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
		// Main entry point
		public Parsed.Story Parse()
		{
			List<Parsed.Object> topLevelContent = StatementsAtLevel (StatementLevel.Top);
            if (hadError) {
                return null;
            }
                
            return new Parsed.Story (topLevelContent);
		}

		protected enum StatementLevel
		{
            InnerBlock,
			Stitch,
			Knot,
			Top
		}

		protected List<Parsed.Object> StatementsAtLevel(StatementLevel level)
		{
			return Interleave<Parsed.Object>(
                Optional (MultilineWhitespace), 
                () => StatementAtLevel (level), 
                untilTerminator: () => StatementsBreakForLevel(level));
		}
            
        protected object StatementAtLevel(StatementLevel level)
        {            
            ParseRule[] rulesAtLevel = _statementRulesAtLevel[(int)level];

            var statement = OneOf (rulesAtLevel);

            // For some statements, allow them to parse, but create errors, since
            // writers may think they can use the statement, so it's useful to have 
            // the error message.
            if (level == StatementLevel.Top) {
                if( statement is Return ) 
                    Error ("should not have return statement outside of a knot");
            }

            return statement;
        }

        protected object StatementsBreakForLevel(StatementLevel level)
        {
            Whitespace ();

            ParseRule[] breakRules = _statementBreakRulesAtLevel[(int)level];

            var breakRuleResult = OneOf (breakRules);
            if (breakRuleResult == null)
                return null;

            return breakRuleResult;
        }

		void GenerateStatementLevelRules()
		{
            var levels = Enum.GetValues (typeof(StatementLevel)).Cast<StatementLevel> ().ToList();

            _statementRulesAtLevel = new ParseRule[levels.Count][];
            _statementBreakRulesAtLevel = new ParseRule[levels.Count][];

            foreach (var level in levels) {
                List<ParseRule> rulesAtLevel = new List<ParseRule> ();
                List<ParseRule> breakingRules = new List<ParseRule> ();

                // Diverts can go anywhere
                // (Check before KnotDefinition since possible "==>" has to be found before "== name ==")
                rulesAtLevel.Add(Line(Divert));

                if (level >= StatementLevel.Top) {

                    // Knots can only be parsed at Top/Global scope
                    rulesAtLevel.Add (KnotDefinition);
                }

                rulesAtLevel.Add(Line(Choice));

                // Gather lines would be confused with multi-line block separators, like
                // within a multi-line if statement
                if (level > StatementLevel.InnerBlock) {
                    rulesAtLevel.Add (Gather);
                }

                // Stitches (and gathers) can (currently) only go in Knots and top level
                if (level >= StatementLevel.Knot) {
                    rulesAtLevel.Add (StitchDefinition);
                }

                // Normal logic / text can go anywhere
                rulesAtLevel.Add(LogicLine);
                rulesAtLevel.Add(LineOfMixedTextAndLogic);

                // --------
                // Breaking rules

                // Break current knot with a new knot
                if (level <= StatementLevel.Knot) {
                    breakingRules.Add (KnotDeclaration);
                }

                // Break current stitch with a new stitch
                if (level <= StatementLevel.Stitch) {
                    breakingRules.Add (StitchDeclaration);
                }

                // Breaking an inner block (like a multi-line condition statement)
                if (level <= StatementLevel.InnerBlock) {
                    breakingRules.Add (String ("-"));
                    breakingRules.Add (String ("}"));
                }

                _statementRulesAtLevel [(int)level] = rulesAtLevel.ToArray ();
                _statementBreakRulesAtLevel [(int)level] = breakingRules.ToArray ();
            }
		}

		protected object SkipToNextLine()
		{
			ParseUntilCharactersFromString ("\n\r");
			ParseNewline ();
			return ParseSuccess;
		}

		// Modifier to turn a rule into one that expects a newline on the end.
		// e.g. anywhere you can use "MixedTextAndLogic" as a rule, you can use 
		// "Line(MixedTextAndLogic)" to specify that it expects a newline afterwards.
		protected ParseRule Line(ParseRule inlineRule)
		{
			return () => {
				var result = ParseObject(inlineRule);
				if( result == null ) {
					return null;
				}

				Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

				return result;
			};
		}

        const string knotDivertArrow = "==>";
        const string stitchDivertArrow = "=>";
        const string weavePointDivertArrow = "->";
        const string weavePointDivertAltArrow = "-->";
        const string weavePointExplicitGather = "<explicit-gather>";

		protected Divert Divert()
		{
			Whitespace ();

            var knotName = ParseDivertTargetWithArrow (knotDivertArrow);
            var stitchName = ParseDivertTargetWithArrow (stitchDivertArrow);
            var weavePointName = ParseDivertTargetWithArrow (weavePointDivertArrow);
            if (knotName == null && stitchName == null && weavePointName == null) {
                return null;
            }

            Whitespace ();

            var optionalArguments = Parse(ExpressionFunctionCallArguments);

            // Weave point explicit gather
            if (weavePointName == weavePointExplicitGather) {
                var gatherDivert = new Divert (null);
                gatherDivert.isToGather = true;
                return gatherDivert;
            }

            // Normal divert
            else {
                Path targetPath = Path.To(knotName, stitchName, weavePointName);
                return new Divert (targetPath, optionalArguments);
            }
		}

        string ParseDivertTargetWithArrow(string arrowStr)
        {
            Whitespace ();

            string parsedArrowResult = ParseString (arrowStr);

            // Allow both -> and --> for weaves
            if (parsedArrowResult == null && arrowStr == weavePointDivertArrow) {
                parsedArrowResult = ParseString (weavePointDivertAltArrow);
            }

            if (parsedArrowResult == null) {
                return null;
            }

            Whitespace ();

            string targetName = null;

            // Weave arrows without a target mean "explicit gather"
            if (arrowStr == weavePointDivertArrow) {
                targetName = Identifier ();
                if (targetName == null) {
                    targetName = weavePointExplicitGather;
                }
            } else {
                targetName = (string) Expect(Identifier, "name of target to divert to");
            }
                
            return targetName;
        }

		protected string DivertArrow()
		{
            return OneOf(String(knotDivertArrow), String(stitchDivertArrow), String(weavePointDivertArrow), String(weavePointDivertAltArrow)) as string;
		}

		protected string Identifier()
		{
			if (_identifierCharSet == null) {

                _identifierFirstCharSet = new CharacterSet ();
                _identifierFirstCharSet.AddRange ('A', 'Z');
                _identifierFirstCharSet.AddRange ('a', 'z');
                _identifierFirstCharSet.Add ('_');

                // TEMP: Allow read counts like "myKnot.myStitch" to be parsed
                _identifierFirstCharSet.Add ('.');

                _identifierCharSet = new CharacterSet(_identifierFirstCharSet);
				_identifierCharSet.AddRange ('0', '9');
			}
                
            // Parse single character first
            var name = ParseCharactersFromCharSet (_identifierFirstCharSet, true, 1);
            if (name == null) {
                return null;
            }

            // Parse remaining characters (if any)
            var tailChars = ParseCharactersFromCharSet (_identifierCharSet);
            if (tailChars != null) {
                name = name + tailChars;
            }

            return name;
		}
        private CharacterSet _identifierFirstCharSet;
		private CharacterSet _identifierCharSet;


        protected Parsed.Text ContentText()
        {
            return ContentTextAllowingEcapeChar ();
        }

        protected Parsed.Text ContentTextAllowingEcapeChar()
        {
            StringBuilder sb = null;

            do {
                var str = Parse(ContentTextNoEscape);
                bool gotEscapeChar = ParseString(@"\") != null;

                if( gotEscapeChar || str != null ) {
                    if( sb == null ) {
                        sb = new StringBuilder();
                    }

                    if( str != null ) {
                        sb.Append(str);
                    }

                    if( gotEscapeChar ) {
                        char c = ParseSingleCharacter();
                        sb.Append(c);
                    }

                } else {
                    break;
                }

            } while(true);

            if (sb != null ) {
                return new Parsed.Text (sb.ToString ());

            } else {
                return null;
            }
        }

		// Content text is an unusual parse rule compared with most since it's
		// less about saying "this is is the small selection of stuff that we parse"
		// and more "we parse ANYTHING except this small selection of stuff".
		protected string ContentTextNoEscape()
		{
			// Eat through text, pausing at the following characters, and
			// attempt to parse the nonTextRule.
			// "-": possible start of divert or start of gather
			if (_nonTextPauseCharacters == null) {
				_nonTextPauseCharacters = new CharacterSet ("-:");
			}

			// If we hit any of these characters, we stop *immediately* without bothering to even check the nonTextRule
            // "{" for start of logic
            // "=" for start of divert or new stitch
			if (_nonTextEndCharacters == null) {
                _nonTextEndCharacters = new CharacterSet ("={}|\n\r\\");
			}

			// When the ParseUntil pauses, check these rules in case they evaluate successfully
			ParseRule nonTextRule = () => OneOf (DivertArrow, EndOfLine, Glue);
			
			string pureTextContent = ParseUntil (nonTextRule, _nonTextPauseCharacters, _nonTextEndCharacters);
			if (pureTextContent != null ) {
                return pureTextContent;

			} else {
                return null;
			}

		}
		private CharacterSet _nonTextPauseCharacters;
		private CharacterSet _nonTextEndCharacters;

        ParseRule[][] _statementRulesAtLevel;
        ParseRule[][] _statementBreakRulesAtLevel;
	}
}

