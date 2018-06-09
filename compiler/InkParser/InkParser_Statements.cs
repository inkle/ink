using System;
using System.Collections.Generic;
using System.Linq;
using Ink.Parsed;

namespace Ink
{
	public partial class InkParser
	{
		protected enum StatementLevel
		{
            InnerBlock,
			Stitch,
			Knot,
			Top
		}

		protected List<Parsed.Object> StatementsAtLevel(StatementLevel level)
		{
            // Check for error: Should not be allowed gather dashes within an inner block
            if (level == StatementLevel.InnerBlock) {
                object badGatherDashCount = Parse(GatherDashes);
                if (badGatherDashCount != null) {
                    Error ("You can't use a gather (the dashes) within the { curly braces } context. For multi-line sequences and conditions, you should only use one dash.");
                }
            }
                
			return Interleave<Parsed.Object>(
                Optional (MultilineWhitespace), 
                () => StatementAtLevel (level), 
                untilTerminator: () => StatementsBreakForLevel(level));
		}
            
        protected object StatementAtLevel(StatementLevel level)
        {            
            ParseRule[] rulesAtLevel = _statementRulesAtLevel[(int)level];

            var statement = OneOf (rulesAtLevel);

            // For some statements, allow them to parse, but create errors, since
            // writers may think they can use the statement, so it's useful to have 
            // the error message.
            if (level == StatementLevel.Top) {
                if( statement is Return ) 
                    Error ("should not have return statement outside of a knot");
            }

            return statement;
        }

        protected object StatementsBreakForLevel(StatementLevel level)
        {
            Whitespace ();

            ParseRule[] breakRules = _statementBreakRulesAtLevel[(int)level];

            var breakRuleResult = OneOf (breakRules);
            if (breakRuleResult == null)
                return null;

            return breakRuleResult;
        }

		void GenerateStatementLevelRules()
		{
            var levels = Enum.GetValues (typeof(StatementLevel)).Cast<StatementLevel> ().ToList();

            _statementRulesAtLevel = new ParseRule[levels.Count][];
            _statementBreakRulesAtLevel = new ParseRule[levels.Count][];

            foreach (var level in levels) {
                List<ParseRule> rulesAtLevel = new List<ParseRule> ();
                List<ParseRule> breakingRules = new List<ParseRule> ();

                // Diverts can go anywhere
                rulesAtLevel.Add(Line(MultiDivert));

                // Knots can only be parsed at Top/Global scope
                if (level >= StatementLevel.Top)
                    rulesAtLevel.Add (KnotDefinition);

                rulesAtLevel.Add(Line(Choice));

                rulesAtLevel.Add(Line(AuthorWarning));

                // Gather lines would be confused with multi-line block separators, like
                // within a multi-line if statement
                if (level > StatementLevel.InnerBlock) {
                    rulesAtLevel.Add (Gather);
                }

                // Stitches (and gathers) can (currently) only go in Knots and top level
                if (level >= StatementLevel.Knot) {
                    rulesAtLevel.Add (StitchDefinition);
                }

                // Global variable declarations can go anywhere
                rulesAtLevel.Add(Line(ListDeclaration));
                rulesAtLevel.Add(Line(VariableDeclaration));
                rulesAtLevel.Add(Line(ConstDeclaration));
                rulesAtLevel.Add(Line(ExternalDeclaration));

                // Global include can go anywhere
                rulesAtLevel.Add(Line(IncludeStatement));

                // Normal logic / text can go anywhere
                rulesAtLevel.Add(LogicLine);
                rulesAtLevel.Add(LineOfMixedTextAndLogic);

                // --------
                // Breaking rules

                // Break current knot with a new knot
                if (level <= StatementLevel.Knot) {
                    breakingRules.Add (KnotDeclaration);
                }

                // Break current stitch with a new stitch
                if (level <= StatementLevel.Stitch) {
                    breakingRules.Add (StitchDeclaration);
                }

                // Breaking an inner block (like a multi-line condition statement)
                if (level <= StatementLevel.InnerBlock) {
                    breakingRules.Add (ParseDashNotArrow);
                    breakingRules.Add (String ("}"));
                }

                _statementRulesAtLevel [(int)level] = rulesAtLevel.ToArray ();
                _statementBreakRulesAtLevel [(int)level] = breakingRules.ToArray ();
            }
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
				object result = ParseObject(inlineRule);
                if (result == null) {
                    return null;
                }

				Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

				return result;
			};
		}


        ParseRule[][] _statementRulesAtLevel;
        ParseRule[][] _statementBreakRulesAtLevel;
	}
}

