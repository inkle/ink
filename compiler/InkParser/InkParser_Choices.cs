using Ink.Parsed;
using System.Diagnostics;

namespace Ink
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
            Identifier optionalName = Parse(BracketedName);

            Whitespace ();

            // Allow optional newline right after a choice name
            if( optionalName != null ) Newline ();

            // Optional condition for whether the choice should be shown to the player
            Expression conditionExpr = Parse(ChoiceCondition);

            Whitespace ();

            // Ordinarily we avoid parser state variables like these, since
            // nesting would require us to store them in a stack. But since you should
            // never be able to nest choices within choice content, it's fine here.
            Debug.Assert(_parsingChoice == false, "Already parsing a choice - shouldn't have nested choices");
            _parsingChoice = true;

            ContentList startContent = null;
            var startTextAndLogic = Parse (MixedTextAndLogic);
            if (startTextAndLogic != null)
                startContent = new ContentList (startTextAndLogic);


            ContentList optionOnlyContent = null;
            ContentList innerContent = null;

            // Check for a the weave style format:
            //   * "Hello[."]," he said.
            bool hasWeaveStyleInlineBrackets = ParseString("[") != null;
            if (hasWeaveStyleInlineBrackets) {

                EndTagIfNecessary(startContent);

                var optionOnlyTextAndLogic = Parse (MixedTextAndLogic);
                if (optionOnlyTextAndLogic != null)
                    optionOnlyContent = new ContentList (optionOnlyTextAndLogic);


                Expect (String("]"), "closing ']' for weave-style option");

                EndTagIfNecessary(optionOnlyContent);

                var innerTextAndLogic = Parse (MixedTextAndLogic);
                if( innerTextAndLogic != null )
                    innerContent = new ContentList (innerTextAndLogic);
            }

			Whitespace ();

            EndTagIfNecessary(innerContent ?? startContent);

            // Finally, now we know we're at the end of the main choice body, parse
            // any diverts separately.
            var diverts =  Parse(MultiDivert);

            _parsingChoice = false;

            Whitespace ();

            // Completely empty choice without even an empty divert?
            bool emptyContent = !startContent && !innerContent && !optionOnlyContent;
            if (emptyContent && diverts == null)
                Warning ("Choice is completely empty. Interpretting as a default fallback choice. Add a divert arrow to remove this warning: * ->");

            // * [] some text
            else if (!startContent && hasWeaveStyleInlineBrackets && !optionOnlyContent)
                Warning ("Blank choice - if you intended a default fallback choice, use the `* ->` syntax");

            if (!innerContent) innerContent = new ContentList ();

            EndTagIfNecessary(innerContent);

            // Normal diverts on the end of a choice - simply add to the normal content
            if (diverts != null) {
                foreach (var divObj in diverts) {
                    // may be TunnelOnwards
                    var div = divObj as Divert;

                    // Empty divert serves no purpose other than to say
                    // "this choice is intentionally left blank"
                    // (as an invisible default choice)
                    if (div && div.isEmpty) continue;

                    innerContent.AddContent (divObj);
                }
            }

            // Terminate main content with a newline since this is the end of the line
            // Note that this will be redundant if the diverts above definitely take
            // the flow away permanently.
            innerContent.AddContent (new Text ("\n"));

            var choice = new Choice (startContent, optionOnlyContent, innerContent);
            choice.identifier = optionalName;
            choice.indentationDepth = bullets.Count;
            choice.hasWeaveStyleInlineBrackets = hasWeaveStyleInlineBrackets;
            choice.condition = conditionExpr;
            choice.onceOnly = onceOnlyChoice;
            choice.isInvisibleDefault = emptyContent;

            return choice;

		}

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
            DisallowIncrement (condExpr);

            Expect (String ("}"), "closing '}' for choice condition");

            return condExpr;
        }

        protected Gather Gather()
        {
            object gatherDashCountObj = Parse(GatherDashes);
            if (gatherDashCountObj == null) {
                return null;
            }

            int gatherDashCount = (int)gatherDashCountObj;

            // Optional name for the gather
            Identifier optionalName = Parse(BracketedName);

            var gather = new Gather (optionalName, gatherDashCount);

            // Optional newline before gather's content begins
            Newline ();

            return gather;
        }

        protected object GatherDashes()
        {
            Whitespace ();

            int gatherDashCount = 0;

            while (ParseDashNotArrow () != null) {
                gatherDashCount++;
                Whitespace ();
            }

            if (gatherDashCount == 0)
                return null;

            return gatherDashCount;
        }

        protected object ParseDashNotArrow()
        {
            var ruleId = BeginRule ();

            if (ParseString ("->") == null && ParseSingleCharacter () == '-') {
                return SucceedRule (ruleId);
            } else {
                return FailRule (ruleId);
            }
        }

        protected Identifier BracketedName()
        {
            if (ParseString ("(") == null)
                return null;

            Whitespace ();

            Identifier name = Parse(IdentifierWithMetadata);
            if (name == null)
                return null;

            Whitespace ();

            Expect (String (")"), "closing ')' for bracketed name");

            return name;
        }

        bool _parsingChoice;
	}
}

