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

				// Knots can only be defined at Top/Global scope
				rulesAtLevel.Add (KnotDefinition);
			}

			// The following statements can go anywhere
			rulesAtLevel.Add(Line(Divert));

			// Knots and stitches only
			if (level <= StatementLevel.Knot) {
				rulesAtLevel.Add(Line(Choice));
			}

			// Stitches can (currently) only go in Knots
			if (level == StatementLevel.Knot) {
				rulesAtLevel.Add (StitchDefinition);
			}

			rulesAtLevel.Add (LogicLine);

			rulesAtLevel.Add(Line(MixedTextAndLogic));

			var statement = OneOf (rulesAtLevel.ToArray());
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
			return OneOf(() => ParseString("->"), 
				() => ParseString("-->")) as string;
		}


		protected string Identifier()
		{
			if (_identifierCharSet == null) {

				_identifierCharSet = new CharacterSet();
				_identifierCharSet.AddRange ('A', 'Z');
				_identifierCharSet.AddRange ('a', 'z');
				_identifierCharSet.AddRange ('0', '9');
				_identifierCharSet.Add ('_');
			}

			return ParseCharactersFromCharSet (_identifierCharSet);
		}
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

			Expect (() => ParseString ("}"), "closing brace '}' for inline logic");

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
				_nonTextEndCharacters = new CharacterSet ("§{\n\r");
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

