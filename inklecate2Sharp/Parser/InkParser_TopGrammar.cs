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
			Stitch,
			Knot,
			Top
		}

		protected List<Parsed.Object> StatementsAtLevel(StatementLevel level)
		{
			return Interleave<Parsed.Object>(Optional (MultilineWhitespace), 
				() => StatementAtLevel (level));
		}

		protected object StatementAtLevel(StatementLevel level)
		{
			List<ParseRule> rulesAtLevel = new List<ParseRule> ();

			if (level >= StatementLevel.Top) {

				// Knots can only be parsed at Top/Global scope
				rulesAtLevel.Add (KnotDefinition);
			}

			// Diverts can go anywhere
			rulesAtLevel.Add(Line(Divert));

            // Error checking for Choices in the wrong place is below (after parsing)
			rulesAtLevel.Add(Line(Choice));

			// Stitches can (currently) only go in Knots
			if (level == StatementLevel.Knot) {
				rulesAtLevel.Add (StitchDefinition);
			}

            // Normal logic / text can go anywhere
			rulesAtLevel.Add (LogicLine);
			rulesAtLevel.Add(LineOfMixedTextAndLogic);

            // Parse the rules
			var statement = OneOf (rulesAtLevel.ToArray());

            // For some statements, allow them to parse, but create errors, since
            // writers may think they can use the statement, so it's useful to have 
            // the error message.
            if (level == StatementLevel.Top) {

                if( statement is Return ) 
                    Error ("should not have return statement outside of a knot");

                if (statement is Choice)
                    Error ("choices can only be in knots and stitches");
            }

			if (statement == null) {
				return null;
			}

			return statement;
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

		protected Parsed.Divert Divert()
		{
			BeginRule ();

			Whitespace ();

			var returningDivertChar = ParseString("<");
			bool returning = returningDivertChar != null;

			string divertArrowStr = DivertArrow ();
			if (divertArrowStr == null) {
				return FailRule () as Parsed.Divert;
			}
			bool isGlobal = divertArrowStr == "->";

			Whitespace ();

			string targetName = Identifier ();

			Path targetPath = isGlobal ? Path.ToKnot (targetName) : Path.ToStitch (targetName);

			return SucceedRule( new Divert( targetPath, returning ) ) as Divert;
		}

		protected string DivertArrow()
		{
            return OneOf(String("->"), String("-->")) as string;
		}


		protected string Identifier()
		{
			if (_identifierCharSet == null) {

                _identifierFirstCharSet = new CharacterSet ();
                _identifierFirstCharSet.AddRange ('A', 'Z');
                _identifierFirstCharSet.AddRange ('a', 'z');
                _identifierFirstCharSet.AddRange ('0', '9');
                _identifierFirstCharSet.Add ('_');

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


		protected Parsed.Object LogicLine()
		{
			BeginRule ();

			Whitespace ();

			if (ParseString ("~") == null) {
				return FailRule () as Parsed.Object;
			}

			Whitespace ();

            ParseRule afterTilda = () => OneOf (ReturnStatement, VariableDeclarationOrAssignment, Expression);

            var parsedExpr = (Parsed.Object) Expect(afterTilda, "expression after '~'", recoveryRule: SkipToNextLine);

			// TODO: A piece of logic after a tilda shouldn't have its result printed as text (I don't think?)
            return SucceedRule (parsedExpr) as Parsed.Object;
		}

        protected List<Parsed.Object> LineOfMixedTextAndLogic()
        {
            BeginRule ();

            var result = MixedTextAndLogic();
            if (result == null || result.Count == 0) {
                return (List<Parsed.Object>) FailRule();
            }

            // Trim whitepace from start
            var firstText = result[0] as Text;
            if (firstText != null) {
                firstText.content = firstText.content.TrimStart(' ', '\t');
                if (firstText.content.Length == 0) {
                    result.RemoveAt (0);
                }
            }
            if (result.Count == 0) {
                return (List<Parsed.Object>) FailRule();
            }

            // Trim whitespace from end and add a newline
            var lastObj = result.Last ();
            if (lastObj is Text) {
                var text = (Text)lastObj;
                text.content = text.content.TrimEnd (' ', '\t') + "\n";
            } 

            // Last object in line wasn't text (but some kind of logic), so
            // we need to append the newline afterwards using a new object
            // TODO: Under what conditions should we NOT do this?
            else {
                result.Add (new Text ("\n"));
            }

            Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

            return (List<Parsed.Object>) SucceedRule(result);
        }

		protected List<Parsed.Object> MixedTextAndLogic()
		{
			// Either, or both interleaved
			return Interleave<Parsed.Object>(Optional (ContentText), Optional (InlineLogic));
		}

		protected Parsed.Object InlineLogic()
		{
			BeginRule ();

			if ( ParseString ("{") == null) {
				return FailRule () as Parsed.Object;
			}

			Whitespace ();

			var logic = InnerLogic ();

			Whitespace ();

            Expect (String("}"), "closing brace '}' for inline logic");

			return SucceedRule(logic) as Parsed.Object;
		}

		protected Parsed.Object InnerLogic()
		{
            var expr = Expression ();
            expr.outputWhenComplete = true;
            return expr;
		}

		// Content text is an unusual parse rule compared with most since it's
		// less about saying "this is is the small selection of stuff that we parse"
		// and more "we parse ANYTHING except this small selection of stuff".
		protected Parsed.Text ContentText()
		{
			// Eat through text, pausing at the following characters, and
			// attempt to parse the nonTextRule.
			// "/" for possible start of comment
			// "-" for possible start of Divert
			if (_nonTextPauseCharacters == null) {
				_nonTextPauseCharacters = new CharacterSet ("-/");
			}

			// If we hit any of these characters, we stop *immediately* without bothering to even check the nonTextRule
			if (_nonTextEndCharacters == null) {
				_nonTextEndCharacters = new CharacterSet ("={\n\r");
			}

			// When the ParseUntil pauses, check these rules in case they evaluate successfully
			ParseRule nonTextRule = () => OneOf (DivertArrow, EndOfLine);
			
			string pureTextContent = ParseUntil (nonTextRule, _nonTextPauseCharacters, _nonTextEndCharacters);
			if (pureTextContent != null ) {
				return new Parsed.Text (pureTextContent);
			} else {
				return null;
			}

		}
		private CharacterSet _nonTextPauseCharacters;
		private CharacterSet _nonTextEndCharacters;
	}
}

