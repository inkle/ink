using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
    internal partial class InkParser
    {
        protected Conditional InnerConditionalContent()
        {
            var initialQueryExpression = Parse(ConditionExpression);
            var conditional = Parse(() => InnerConditionalContent (initialQueryExpression));
            if (conditional == null)
                return null;

            return conditional;
        }

        protected Conditional InnerConditionalContent(Expression initialQueryExpression)
        {
            List<ConditionalSingleBranch> alternatives;

            bool canBeInline = initialQueryExpression != null;
            bool isInline = Newline () == null;

            if (isInline && !canBeInline) {
                return null;
            }

            // Inline innards
            if (isInline) {
                alternatives = InlineConditionalBranches ();
            } 

            // Multiline innards
            else {
                alternatives = MultilineConditionalBranches ();
                if (alternatives == null) {

                    // Allow single piece of content within multi-line expression, e.g.:
                    // { true: 
                    //    Some content that isn't preceded by '-'
                    // }
                    if (initialQueryExpression) {
                        List<Parsed.Object> soleContent = StatementsAtLevel (StatementLevel.InnerBlock);
                        if (soleContent != null) {
                            var soleBranch = new ConditionalSingleBranch (soleContent);
                            alternatives = new List<ConditionalSingleBranch> ();
                            alternatives.Add (soleBranch);

                            // Also allow a final "- else:" clause
                            var elseBranch = Parse(SingleMultilineCondition);
                            if (elseBranch) {
                                if (!elseBranch.alwaysMatch) {
                                    Error ("Expected an '- else:' clause here rather than an extra condition");
                                    elseBranch.alwaysMatch = true;
                                }
                                alternatives.Add (elseBranch);
                            }
                        }
                    }

                    // Still null?
                    if (alternatives == null) {
                        return null;
                    }
                }

                if (initialQueryExpression) {

                    bool earlierBranchesHaveOwnExpression = false;
                    for (int i = 0; i < alternatives.Count; ++i) {
                        var branch = alternatives [i];
                        if (branch.ownExpression) {
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

                    for (int i = 0; i < alternatives.Count; ++i) {
                        var alt = alternatives [i];
                        bool isLast = (i == alternatives.Count - 1);
                        if (alt.ownExpression == null) {
                            if (isLast) {
                                alt.alwaysMatch = true;
                            } else {
                                Error ("in a multi-line condition that has no initial condition, you need a condition on the branches themselves (except the last, which can be an 'else' clause)");
                            }
                        }
                    }
                        
                    if (alternatives.Count == 1 && alternatives [0].ownExpression == null) {
                        Error ("condition block with no actual conditions");
                    }

                }
            }

            // TODO: Come up with water-tight error conditions... it's quite a flexible system!
            // e.g.
            //   - inline conditionals must have exactly 1 or 2 alternatives
            //   - multiline expression shouldn't have mixed existence of branch-conditions?

            var cond = new Conditional (initialQueryExpression, alternatives);
            return cond;
        }

        protected List<ConditionalSingleBranch> InlineConditionalBranches()
        {
            var listOfLists = Interleave<List<Parsed.Object>> (MixedTextAndLogic, Exclude (String ("|")), flatten: false);
            if (listOfLists == null || listOfLists.Count == 0) {
                return null;
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
            MultilineWhitespace ();

            List<object> multipleConditions = OneOrMore (SingleMultilineCondition);
            if (multipleConditions == null)
                return null;
            
            MultilineWhitespace ();

            return multipleConditions.Cast<ConditionalSingleBranch>().ToList();
        }

        protected ConditionalSingleBranch SingleMultilineCondition()
        {
            Whitespace ();

            // Make sure we're not accidentally parsing a divert
            if (ParseString ("->") != null)
                return null;

            if (ParseString ("-") == null)
                return null;

            Whitespace ();

            Expression expr = null;
            bool isElse = Parse(ElseExpression) != null;

            if( !isElse )
                expr = Parse(ConditionExpression);

            List<Parsed.Object> content = StatementsAtLevel (StatementLevel.InnerBlock);
            if (expr == null && content == null) {
                Error ("expected content for the conditional branch following '-'");

                // Recover
                content = new List<Inklewriter.Parsed.Object> ();
                content.Add (new Text (""));
            }

            var branch = new ConditionalSingleBranch (content);
            branch.ownExpression = expr;
            branch.alwaysMatch = isElse;
            return branch;
        }

        protected Expression ConditionExpression()
        {
            var expr = Parse(Expression);
            if (expr == null)
                return null;

            Whitespace ();

            if (ParseString (":") == null)
                return null;

            // Optional "..."
            Parse(Whitespace);
            ParseCharactersFromString (".");

            return expr;
        }

        protected object ElseExpression()
        {
            if (ParseString ("else") == null)
                return null;

            Whitespace ();

            if (ParseString (":") == null)
                return null;

            return ParseSuccess;
        }
    }
}

