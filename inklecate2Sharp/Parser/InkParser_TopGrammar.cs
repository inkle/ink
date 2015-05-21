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
			var statements = Interleave (Optional (MultilineWhitespace), 
				() => StatementAtLevel (level));

			return statements.Cast<Parsed.Object> ().ToList ();
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

			rulesAtLevel.Add(Line(TextContent));

			var statement = OneOf (rulesAtLevel.ToArray());
			if (statement == null) {
				return null;
			}

			return statement;
		}

		protected object SkipToNextLine()
		{
			ParseUntilCharactersFromString ("\n\r");
			Newline ();
			return ParseSuccess;
		}

		// Modifier to turn a rule into one that expects a newline on the end.
		// e.g. anywhere you can use "TextContent" as a rule, you can use 
		// "Line(TextContent)" to specify that it expects a newline afterwards.
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

		protected Parsed.Text TextContent()
		{
			BeginRule ();

			Whitespace ();

			Parsed.Text text = SimpleText ();
			if (text == null) {
				return FailRule () as Parsed.Text;
			}

			Whitespace ();

			return (Parsed.Text) SucceedRule (text);
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


		protected Parsed.Text SimpleText()
		{
			if (_simpleTextCharSet == null) {

				_simpleTextCharSet = new CharacterSet();
				_simpleTextCharSet.AddRange ('A', 'Z');
				_simpleTextCharSet.AddRange ('a', 'z');
				_simpleTextCharSet.AddRange ('0', '9');
				_simpleTextCharSet.AddStringCharacters (".,? ");
			}

			string parsedText = ParseCharactersFromCharSet (_simpleTextCharSet);
			if (parsedText != null) {

				parsedText = parsedText.TrimEnd(' ');
				if (parsedText.Length > 0) {
					return new Parsed.Text (parsedText);
				}
			}

			return null;
		}

		private CharacterSet _simpleTextCharSet;
		private CharacterSet _identifierCharSet;
	}
}

