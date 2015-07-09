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
                
            // Optional name for the choice
            string optionalName = Parse(BracketedName);

            Whitespace ();

            Expression conditionExpr = Parse(ChoiceCondition);

            // Condition

            Whitespace ();
                
            string startText = Parse (ChoiceText);
            string optionOnlyText = null;
            ContentList innerContent = null;

            // Check for a the weave style format:
            //   * "Hello[."]," he said.
            bool hasWeaveStyleInlineBrackets = ParseString("[") != null;
            if (hasWeaveStyleInlineBrackets) {
                optionOnlyText = Parse (ChoiceText);

                Expect (String("]"), "closing ']' for weave-style option");

                var innerTextAndLogic = Parse (MixedTextAndLogic);
                if( innerTextAndLogic != null )
                    innerContent = new ContentList (innerTextAndLogic);
            }
             
            // Trim
            if (innerContent) {
                innerContent.TrimTrailingWhitespace ();
                if (innerContent.content.Count == 0) {
                    innerContent = null;
                } else { 
                    // Inner content of a choice counts as full line 
                    // unless there's glue   
                    innerContent.AddContent (new Text ("\n"));
                }
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

            var choice = new Choice (startText, optionOnlyText, innerContent, divert);
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
            var conditions = Interleave<Expression> (ChoiceSingleCondition, ChoiceConditionsSpace);
            if (conditions == null)
                return null;
            else if (conditions.Count == 1)
                return conditions [0];
            else {
                return new MultipleConditionExpression (conditions);
            }
        }
    
        protected object ChoiceConditionsSpace()
        {
            // Both optional
            // Newline includes initial end of line whitespace
            Newline (); 
            Whitespace ();
            return ParseSuccess;
        }

        protected Expression ChoiceSingleCondition()
        {
            if (ParseString ("{") == null)
                return null;

            var condExpr = Expect(Expression, "choice condition inside { }") as Expression;

            Expect (String ("}"), "closing '}' for choice condition");

            return condExpr;
        }

        protected Gather Gather()
        {
            var dashes = Interleave<string>(OptionalExclude(Whitespace), String("-"));
            if (dashes == null) {
                return null;
            }

            // Optional name for the gather
            string optionalName = Parse(BracketedName);

            // Optional newline before gather's content begins
            Newline ();

            return new Gather (optionalName, dashes.Count);
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

