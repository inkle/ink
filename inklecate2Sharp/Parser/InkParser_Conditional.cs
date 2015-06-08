using System;
using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
    public partial class InkParser
    {
        protected Conditional InnerConditionalContent()
        {
            BeginRule ();

            var initialQueryExpression = ConditionExpression ();

            List<ConditionalSingleBranch> alternatives;

            bool canBeInline = initialQueryExpression != null;
            bool isInline = Newline () == null;

            if (isInline && !canBeInline) {
                return (Conditional) FailRule ();
            }

            // Inline innards
            if (isInline) {
                alternatives = InlineConditionalBranches ();
            } 

            // Multiline innards
            else {
                alternatives = MultilineConditionalBranches ();
                if (alternatives == null) {
                    return (Conditional) FailRule ();
                }

                if (initialQueryExpression != null) {

                    bool earlierBranchesHaveOwnExpression = false;
                    for (int i = 0; i < alternatives.Count; ++i) {
                        var branch = alternatives [i];
                        if (branch.ownExpression != null) {
                            branch.shouldMatchEquality = true;
                            earlierBranchesHaveOwnExpression = true;
                        } else if (earlierBranchesHaveOwnExpression) {
                            branch.alwaysMatch = true;
                        } else {
                            branch.isBoolCondition = true;
                            branch.boolRequired = i == 0 ? true : false;
                        }
                    }
                } else {

                    for (int i = 0; i < alternatives.Count - 1; ++i) {
                        var alt = alternatives [i];
                        if (alt.ownExpression == null) {
                            Error ("in a multi-line condition that has no initial condition, you need a condition on the branches themselves (except the last, which can be an 'else' clause)");
                        }
                    }

                    if (alternatives.Count == 1 && alternatives [0].ownExpression == null) {
                        Error ("condition block with no actual conditions");
                    }
                    
                }
            }


            // TODO: Come up with water-tight error conditions... it's quite a flexible system!
            // e.g.
            //   - inline expressions must have exactly 1 or 2 alternatives
            //   - multiline expression shouldn't have mixed existence of branch-conditions?

            var cond = new Conditional (initialQueryExpression, alternatives);

            return (Conditional) SucceedRule(cond);
        }

        protected List<ConditionalSingleBranch> InlineConditionalBranches()
        {
            var listOfLists = Interleave<List<Parsed.Object>> (MixedTextAndLogic, Exclude (String ("|")), flatten: false);
            if (listOfLists == null || listOfLists.Count == 0) {
                return (List<ConditionalSingleBranch>) FailRule ();
            }

            var result = new List<ConditionalSingleBranch> ();

            if (listOfLists.Count > 2) {
                Error ("Expected one or two alternatives separated by '|' in inline conditional");
            } else {
                
                var trueBranch = new ConditionalSingleBranch (listOfLists[0]);
                trueBranch.boolRequired = true;
                trueBranch.isBoolCondition = true;
                result.Add (trueBranch);

                if (listOfLists.Count > 1) {
                    var falseBranch = new ConditionalSingleBranch (listOfLists[1]);
                    falseBranch.boolRequired = false;
                    falseBranch.isBoolCondition = true;
                    result.Add (falseBranch);
                }
            }

            return result;
        }

        protected List<ConditionalSingleBranch> MultilineConditionalBranches()
        {
            List<object> multipleConditions = OneOrMore (SingleMultilineCondition);
            if (multipleConditions == null) {
                return null;
            } else {
                return multipleConditions.Cast<ConditionalSingleBranch>().ToList();
            }
        }

        protected ConditionalSingleBranch SingleMultilineCondition()
        {
            BeginRule ();

            Whitespace ();

            if (ParseString ("-") == null)
                return (ConditionalSingleBranch) FailRule ();

            Whitespace ();

            var expr = ConditionExpression ();

            List<Parsed.Object> content = StatementsAtLevel (StatementLevel.InnerBlock);
            if (expr == null && content == null) {
                Error ("expected content for the conditional branch following '-'");
            }

            var branch = new ConditionalSingleBranch (content);
            branch.ownExpression = expr;
            return (ConditionalSingleBranch) SucceedRule (branch);
        }

        protected Expression ConditionExpression()
        {
            BeginRule ();

            var expr = Expression ();
            if (expr == null) {
                return (Expression)FailRule ();
            }

            Whitespace ();

            if (ParseString (":") == null) {
                return (Expression) FailRule ();
            }

            // Optional "..."
            Whitespace();
            ParseCharactersFromString (".");

            return (Expression) SucceedRule (expr);
        }
    }
}

