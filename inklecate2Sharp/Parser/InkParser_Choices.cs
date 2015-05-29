using System;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
		protected Choice Choice()
		{
			BeginRule ();

            var bullets = Interleave <string>(OptionalExclude(Whitespace), String("*") );
            if (bullets == null) {
                return (Choice) FailRule ();
            }
                
            string startText = ChoiceText ();
            string optionOnlyText = null;
            string contentOnlyText = null;

            // Check for a the weave style format:
            //   * "Hello[."]," he said.
            bool midTextStarter = ParseString("[") != null;
            if (midTextStarter) {
                optionOnlyText = ChoiceText ();

                Expect (String("]"), "closing ']' for weave-style option");

                contentOnlyText = ChoiceText ();
            }
             
            // Trim
            if (contentOnlyText != null) {
                contentOnlyText = contentOnlyText.TrimEnd (' ', '\t');
                if (contentOnlyText.Length == 0)
                    contentOnlyText = null;
            } else {
                startText = startText.TrimEnd (' ', '\t');
                if (startText.Length == 0)
                    startText = null;
            }

            if (startText == null && optionOnlyText == null) {
                Error ("choice text cannot be empty");
            }
                
			Whitespace ();

			var divert =  Divert ();

            var choice = new Choice (startText, optionOnlyText, contentOnlyText, divert);
            choice.indentationDepth = bullets.Count;

            return SucceedRule(choice) as Choice;

		}

        protected string ChoiceText()
        {
            if( _choiceTextPauseCharacters == null ) {
                _choiceTextPauseCharacters = new CharacterSet ("-");
            }
            if (_choiceTextEndCharacters == null) {
                _choiceTextEndCharacters = new CharacterSet("[]{\n\r");
            }

            return ParseUntil(Divert, pauseCharacters: _choiceTextPauseCharacters, endCharacters: _choiceTextEndCharacters);
        }

		private CharacterSet _choiceTextPauseCharacters;
		private CharacterSet _choiceTextEndCharacters;


        protected Gather Gather()
        {
            BeginRule ();

            // TODO: Handle multiple dashes
            var dashes = Interleave<string>(OptionalExclude(Whitespace), String("-"));
            if (dashes == null) {
                return (Gather) FailRule ();
            }

            // Optional name for the gather
            string optionalName = BracketedName();

            Whitespace ();

            // Optional content from the rest of the line
            var content = MixedTextAndLogic ();

            Gather gather = new Gather (optionalName, content, dashes.Count);

            return (Gather) SucceedRule (gather);
        }

        protected string BracketedName()
        {
            BeginRule ();

            if (ParseString ("(") == null)
                return (string) FailRule ();

            Whitespace ();

            string name = Identifier ();
            if (name == null)
                return (string)FailRule ();

            Whitespace ();

            Expect (String (")"), "closing ')' for bracketed name");

            return (string) SucceedRule (name);
        }
	}
}

