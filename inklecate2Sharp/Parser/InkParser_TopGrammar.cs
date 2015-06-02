
using System;
using System.Collections.Generic;
using System.Linq;
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

			Parsed.Story story = new Parsed.Story (topLevelContent);
			return story;
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
			List<ParseRule> rulesAtLevel = new List<ParseRule> ();

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
                rulesAtLevel.Add (GatherLine);
            }

            // Stitches (and gathers) can (currently) only go in Knots and top level
			if (level >= StatementLevel.Knot) {
				rulesAtLevel.Add (StitchDefinition);
			}

            // Normal logic / text can go anywhere
			rulesAtLevel.Add(LogicLine);
			rulesAtLevel.Add(LineOfMixedTextAndLogic);

            // Parse the rules
			var statement = OneOf (rulesAtLevel.ToArray());

            // For some statements, allow them to parse, but create errors, since
            // writers may think they can use the statement, so it's useful to have 
            // the error message.
            if (level == StatementLevel.Top) {
                if( statement is Return ) 
                    Error ("should not have return statement outside of a knot");
            }

			if (statement == null) {
				return null;
			}

			return statement;
		}

        protected object StatementsBreakForLevel(StatementLevel level)
        {
            BeginRule ();

            Whitespace ();

            var breakingRules = new List<ParseRule> ();

            // Break current knot with a new knot
            if (level <= StatementLevel.Knot) {
                breakingRules.Add (KnotTitleEquals);
            }

            // Break current stitch with a new stitch
            if (level <= StatementLevel.Stitch) {
                breakingRules.Add (String("="));
            }

            // Breaking an inner block (like a multi-line condition statement)
            if (level <= StatementLevel.InnerBlock) {
                breakingRules.Add (String ("-"));
                breakingRules.Add (String ("}"));
            }

            var breakRuleResult = OneOf (breakingRules.ToArray ());
            if (breakRuleResult == null) {
                return FailRule ();
            }

            return SucceedRule (breakRuleResult);
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
				var result = inlineRule();
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
        const string weavePointExplicitGather = "<explicit-gather>";

		protected Divert Divert()
		{
			BeginRule ();

			Whitespace ();

            var knotName = DivertTargetWithArrow (knotDivertArrow);
            var stitchName = DivertTargetWithArrow (stitchDivertArrow);
            var weavePointName = DivertTargetWithArrow (weavePointDivertArrow);
            if (knotName == null && stitchName == null && weavePointName == null) {
                return (Divert)FailRule ();
            }

            Whitespace ();

            var optionalArguments = ExpressionFunctionCallArguments ();

            // Weave point explicit gather
            if (weavePointName == weavePointExplicitGather) {
                var gatherDivert = new Divert (null);
                gatherDivert.isToGather = true;
                return (Divert) SucceedRule (gatherDivert);
            }

            // Normal divert
            else {
                Path targetPath = Path.To(knotName, stitchName, weavePointName);
                return (Divert) SucceedRule( new Divert(targetPath, optionalArguments) );
            }
		}

        string DivertTargetWithArrow(string arrowStr)
        {
            BeginRule ();

            Whitespace ();

            if (ParseString (arrowStr) == null)
                return (string)FailRule ();

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
                
            return (string) SucceedRule (targetName);
        }

		protected string DivertArrow()
		{
            return OneOf(String(knotDivertArrow), String(stitchDivertArrow), String(weavePointDivertArrow)) as string;
		}

		protected string Identifier()
		{
			if (_identifierCharSet == null) {

                _identifierFirstCharSet = new CharacterSet ();
                _identifierFirstCharSet.AddRange ('A', 'Z');
                _identifierFirstCharSet.AddRange ('a', 'z');
                _identifierFirstCharSet.AddRange ('0', '9');
                _identifierFirstCharSet.Add ('_');

                // TEMP: Allow read counts like "myKnot.myStitch" to be parsed
                _identifierFirstCharSet.Add ('.');

                _identifierCharSet = new CharacterSet(_identifierFirstCharSet);
				_identifierCharSet.AddRange ('0', '9');
			}

            BeginRule ();

            // Parse single character first
            var name = ParseCharactersFromCharSet (_identifierFirstCharSet, true, 1);
            if (name == null) {
                return (string) FailRule ();
            }

            // Parse remaining characters (if any)
            var tailChars = ParseCharactersFromCharSet (_identifierCharSet);
            if (tailChars != null) {
                name = name + tailChars;
            }

            return (string) SucceedRule(name);
		}
        private CharacterSet _identifierFirstCharSet;
		private CharacterSet _identifierCharSet;


		// Content text is an unusual parse rule compared with most since it's
		// less about saying "this is is the small selection of stuff that we parse"
		// and more "we parse ANYTHING except this small selection of stuff".
		protected Parsed.Text ContentText()
		{
            BeginRule ();

			// Eat through text, pausing at the following characters, and
			// attempt to parse the nonTextRule.
			// "-": possible start of divert or start of gather
			if (_nonTextPauseCharacters == null) {
				_nonTextPauseCharacters = new CharacterSet ("-");
			}

			// If we hit any of these characters, we stop *immediately* without bothering to even check the nonTextRule
            // "{" for start of logic
            // "=" for start of divert or new stitch
			if (_nonTextEndCharacters == null) {
                _nonTextEndCharacters = new CharacterSet ("={}|\n\r");
			}

			// When the ParseUntil pauses, check these rules in case they evaluate successfully
			ParseRule nonTextRule = () => OneOf (DivertArrow, EndOfLine);
			
			string pureTextContent = ParseUntil (nonTextRule, _nonTextPauseCharacters, _nonTextEndCharacters);
			if (pureTextContent != null ) {
                return (Parsed.Text) SucceedRule( new Parsed.Text (pureTextContent) );

			} else {
                return (Parsed.Text) FailRule();
			}

		}
		private CharacterSet _nonTextPauseCharacters;
		private CharacterSet _nonTextEndCharacters;
	}
}

