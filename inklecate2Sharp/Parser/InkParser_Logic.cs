using System;
using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
    public partial class InkParser
    {
        
        protected Parsed.Object LogicLine()
        {
            BeginRule ();

            Whitespace ();

            if (ParseString ("~") == null) {
                return FailRule () as Parsed.Object;
            }

            Whitespace ();

            // Some example lines we need to be able to distinguish between:
            // ~ var x = 5  -- var decl + assign
            // ~ var x      -- var decl
            // ~ x = 5      -- var assign
            // ~ x          -- expr (not var decl or assign)
            // ~ f()        -- expr
            // We don't treat variable decl/assign as an expression since we don't want an assignment
            // to have a return value, or to be used in compound expressions.
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

            var logic = Expect(InnerLogic, "inner logic within '{' and '}' braces");
            if (logic == null) {
                return (Parsed.Object) FailRule ();
            }


            Whitespace ();

            Expect (String("}"), "closing brace '}' for inline logic");

            return SucceedRule(logic) as Parsed.Object;
        }

        protected Parsed.Object InnerLogic()
        {
            return (Parsed.Object) OneOf (InnerConditionalContent, InnerExpression);
        }

        protected Conditional InnerConditionalContent()
        {
            BeginRule ();

            var expr = Expression ();
            if (expr == null) {
                return (Conditional) FailRule ();
            }

            Whitespace ();

            if (ParseString (":") == null)
                return (Conditional) FailRule ();

            List<List<Parsed.Object>> alternatives;

            // Multi-line conditional
            if (Newline () != null) {
                alternatives = (List<List<Parsed.Object>>) Expect (MultilineConditionalOptions, "conditional branches on following lines");
            } 

            // Inline conditional
            else {
                alternatives = Interleave<List<Parsed.Object>>(MixedTextAndLogic, Exclude (String ("|")), flatten:false);
            }

            if (alternatives == null || alternatives.Count < 1 || alternatives.Count > 2) {
                Error ("Expected one or two alternatives separated by '|' in inline conditional");
                return (Conditional)FailRule ();
            }

            List<Parsed.Object> contentIfTrue = alternatives [0];
            List<Parsed.Object> contentIfFalse = null;
            if (alternatives.Count > 1) {
                contentIfFalse = alternatives [1];
            }

            var cond = new Conditional (expr, contentIfTrue, contentIfFalse);

            return (Conditional) SucceedRule(cond);
        }

        protected List<List<Parsed.Object>> MultilineConditionalOptions()
        {
            return OneOrMore (IndividualConditionBranchLine).Cast<List<Parsed.Object>>().ToList();
        }

        protected List<Parsed.Object> IndividualConditionBranchLine()
        {
            BeginRule ();

            Whitespace ();

            if (ParseString ("-") == null)
                return (List<Parsed.Object>) FailRule ();

            Whitespace ();

            List<Parsed.Object> content = LineOfMixedTextAndLogic ();

            return (List<Parsed.Object>) SucceedRule (content);
        }

        protected Parsed.Object InnerExpression()
        {
            var expr = Expression ();
            if (expr != null) {
                expr.outputWhenComplete = true;
            }
            return expr;
        }

    }
}

