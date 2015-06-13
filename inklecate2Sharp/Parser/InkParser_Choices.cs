using System;
using System.Collections.Generic;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
		protected Choice Choice()
		{
            bool onceOnlyChoice = true;
            var bullets = Interleave <string>(OptionalExclude(Whitespace), String("*") );
            if (bullets == null) {

                bullets = Interleave <string>(OptionalExclude(Whitespace), String("+") );
                if (bullets == null) {
                    return null;
                }

                onceOnlyChoice = false;
            }

            // Optional name for the gather
            string optionalName = Parse(BracketedName);

            Whitespace ();
                
            string startText = Parse (ChoiceText);
            string optionOnlyText = null;
            string contentOnlyText = null;

            // Check for a the weave style format:
            //   * "Hello[."]," he said.
            bool hasWeaveStyleInlineBrackets = ParseString("[") != null;
            if (hasWeaveStyleInlineBrackets) {
                optionOnlyText = Parse (ChoiceText);

                Expect (String("]"), "closing ']' for weave-style option");

                contentOnlyText = Parse(ChoiceText);
            }
             
            // Trim
            if (contentOnlyText != null) {
                contentOnlyText = contentOnlyText.TrimEnd (' ', '\t');
                if (contentOnlyText.Length == 0)
                    contentOnlyText = null;
            } else if( startText != null ) {
                startText = startText.TrimEnd (' ', '\t');
                if (startText.Length == 0)
                    startText = null;
            }

            if (startText == null && optionOnlyText == null) {
                Error ("choice text cannot be empty");
            }
                
			Whitespace ();

            var divert =  Parse(Divert);

            Whitespace ();

            var conditionExpr = Parse(ChoiceCondition);

            var choice = new Choice (startText, optionOnlyText, contentOnlyText, divert);
            choice.name = optionalName;
            choice.indentationDepth = bullets.Count;
            choice.hasWeaveStyleInlineBrackets = hasWeaveStyleInlineBrackets;
            choice.condition = conditionExpr;
            choice.onceOnly = onceOnlyChoice;

            return choice;

		}

        protected string ChoiceText()
        {
            if( _choiceTextPauseCharacters == null ) {
                _choiceTextPauseCharacters = new CharacterSet ("-");
            }
            if (_choiceTextEndCharacters == null) {
                _choiceTextEndCharacters = new CharacterSet("[]={\n\r");
            }

            return ParseUntil(Divert, pauseCharacters: _choiceTextPauseCharacters, endCharacters: _choiceTextEndCharacters);
        }

		private CharacterSet _choiceTextPauseCharacters;
		private CharacterSet _choiceTextEndCharacters;

        protected Expression ChoiceCondition()
        {
            if (ParseString ("{") == null)
                return null;

            var condExpr = Expect(Expression, "choice condition inside { }") as Expression;

            Expect (String ("}"), "closing '}' for choice condition");

            return condExpr;
        }

        protected Gather GatherLine()
        {
            // TODO: Handle multiple dashes
            var dashes = Interleave<string>(OptionalExclude(Whitespace), String("-"));
            if (dashes == null) {
                return null;
            }

            // Optional name for the gather
            string optionalName = Parse(BracketedName);

            Whitespace ();

            // Optional content from the rest of the line
            var content = Parse(MixedTextAndLogic);
            if (content == null) {
                content = new List<Parsed.Object> ();
            }

            Expect (EndOfLine, "end of line after gather ('-' with content)");
            content.Add (new Parsed.Text ("\n"));

            Gather gather = new Gather (optionalName, content, dashes.Count);

            return gather;
        }

        protected string BracketedName()
        {
            if (ParseString ("(") == null)
                return null;

            Whitespace ();

            string name = Parse(Identifier);
            if (name == null)
                return null;

            Whitespace ();

            Expect (String (")"), "closing ')' for bracketed name");

            return name;
        }
	}
}

