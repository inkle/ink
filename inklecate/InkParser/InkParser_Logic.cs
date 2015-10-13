using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
    internal partial class InkParser
    {
        
        protected Parsed.Object LogicLine()
        {
            Whitespace ();

            if (ParseString ("~") == null) {
                return null;
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
            ParseRule afterTilda = () => OneOf (IncludeStatement, ReturnStatement, VariableDeclarationOrAssignment, Expression);

            var result = Expect(afterTilda, "expression after '~'", recoveryRule: SkipToNextLine);

            // Parse all expressions, but tell the writer off if they did something useless like:
            //  ~ 5 + 4
            // And even:
            //  ~ false && myFunction()
            // ...since it's bad practice, and won't do what they expect if
            // they're expecting C's lazy evaluation.
            if (result is Expression && !(result is FunctionCall || result is IncDecExpression) ) {
                Error ("Logic following a '~' can't be that type of expression. It can only be something like:\n\t~ include ...\n\t~ return\n\t~ var x = blah\n\t~ x++\n\t~ myFunction()");
            }

            return result as Parsed.Object;
        }

        protected object IncludeStatement()
        {
            if (ParseString ("include") == null)
                return null;

            Whitespace ();

            var filename = (string) Expect(() => ParseUntilCharactersFromString ("\n\r"), "filename for include statement");
            filename = filename.TrimEnd (' ', '\t');

            Parsed.Story includedStory = null;
            try {
                string includedString = System.IO.File.ReadAllText(filename);

                InkParser parser = new InkParser(includedString, filename);
                includedStory = parser.Parse();

                if( includedStory == null ) {
                    // This error should never happen: if the includedStory isn't
                    // returned, then it should've been due to some error that
                    // has already been reported, so this is a last resort.
                    if( !parser.hadError ) {
                        Error ("Failed to parse included file '" + filename + "'");
                    }
                }

            }
            catch {
                Error ("Included file not found: " + filename);
            }

            // Return valid IncludedFile object even when story failed to parse and we have a null story:
            // we don't want to attempt to re-parse the include line as something else
            return new IncludedFile (includedStory);
        }

        protected Parsed.Object InlineLogicOrGlue()
        {
            return (Parsed.Object) OneOf (InlineLogic, Glue);
        }

        protected Parsed.Wrap<Runtime.Glue> Glue()
        {
            // Don't want to parse whitespace, since it might be important
            // surrounding the glue.
            var glueStr = ParseString("<>");
            if (glueStr != null) {
                var glue = new Runtime.Glue ();
                return new Parsed.Wrap<Runtime.Glue> (glue);
            } else {
                return null;
            }
        }

        protected Parsed.Object InlineLogic()
        {
            if ( ParseString ("{") == null) {
                return null;
            }

            Whitespace ();

            var logic = (Parsed.Object) Expect(InnerLogic, "some kind of logic within braces: { ... }");
                
            Whitespace ();

            Expect (String("}"), "closing brace '}' for inline logic");

            return logic;
        }

        protected Parsed.Object ExpectedInnerLogic()
        {

            var innerLogicObj = Expect(InnerLogic, 
                "inner logic or sequence between '{' and '}' braces");

            return (Parsed.Object) innerLogicObj;
        }

        protected Parsed.Object InnerLogic()
        {
            Whitespace ();

            // Explicitly try the combinations of inner logic
            // that could potentially have conflicts first.

            // Explicit sequence annotation?
            SequenceType? explicitSeqType = (SequenceType?) ParseObject(SequenceTypeAnnotation);
            if (explicitSeqType != null) {
                var contentLists = (List<ContentList>) Expect(InnerSequenceObjects, "sequence elements (for cycle/stoping etc)");
                return new Sequence (contentLists, (SequenceType) explicitSeqType);
            }

            // Conditional with expression?
            var initialQueryExpression = Parse(ConditionExpression);
            if (initialQueryExpression) {
                var conditional = (Conditional) Expect(() => InnerConditionalContent (initialQueryExpression), "conditional content following query (i.e. '"+initialQueryExpression+"'");
                return conditional;
            }

            // Now try to evaluate each of the "full" rules in turn
            ParseRule[] rules = {

                // Conditional still necessary, since you can have a multi-line conditional
                // without an initial query expression:
                // {
                //   - true:  this is true
                //   - false: this is false
                // }
                InnerConditionalContent, 
                InnerSequence,
                InnerExpression,
            };

            // Adapted from "OneOf" structuring rule except that in 
            // order for the rule to succeed, it has to maximally 
            // cover the entire string within the { }. Used to
            // differentiate between:
            //  {myVar}                 -- Expression (try first)
            //  {my content is jolly}   -- sequence with single element
            foreach (ParseRule rule in rules) {
                int ruleId = BeginRule ();

                Parsed.Object result = ParseObject(rule) as Parsed.Object;
                if (result) {

                    // Not yet at end?
                    if (Peek (Spaced (String ("}"))) == null)
                        FailRule (ruleId);

                    // Full parse of content within braces
                    else
                        return (Parsed.Object) SucceedRule (ruleId, result);
                    
                } else {
                    FailRule (ruleId);
                }
            }

            return null;
        }

        protected Parsed.Object InnerExpression()
        {
            var expr = Parse(Expression);
            if (expr) {
                expr.outputWhenComplete = true;
            }
            return expr;
        }

        protected string Identifier()
        {
            if (_identifierCharSet == null) {

                _identifierFirstCharSet = new CharacterSet ();
                _identifierFirstCharSet.AddRange ('A', 'Z');
                _identifierFirstCharSet.AddRange ('a', 'z');
                _identifierFirstCharSet.Add ('_');

                _identifierCharSet = new CharacterSet(_identifierFirstCharSet);
                _identifierCharSet.AddRange ('0', '9');
            }

            // Parse single character first
            var name = ParseCharactersFromCharSet (_identifierFirstCharSet, true, 1);
            if (name == null) {
                return null;
            }

            // Parse remaining characters (if any)
            var tailChars = ParseCharactersFromCharSet (_identifierCharSet);
            if (tailChars != null) {
                name = name + tailChars;
            }

            return name;
        }
        private CharacterSet _identifierFirstCharSet;
        private CharacterSet _identifierCharSet;
    }
}

