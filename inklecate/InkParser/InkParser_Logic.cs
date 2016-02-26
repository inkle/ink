using System.Collections.Generic;
using System.Linq;
using Ink.Parsed;

namespace Ink
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
            // ~ temp x = 5  -- var decl + assign
            // ~ temp x      -- var decl
            // ~ x = 5       -- var assign
            // ~ x           -- expr (not var decl or assign)
            // ~ f()         -- expr
            // We don't treat variable decl/assign as an expression since we don't want an assignment
            // to have a return value, or to be used in compound expressions.
            ParseRule afterTilda = () => OneOf (IncludeStatement, ReturnStatement, TempDeclarationOrAssignment, Expression);

            var result = Expect(afterTilda, "expression after '~'", recoveryRule: SkipToNextLine) as Parsed.Object;

            // Parse all expressions, but tell the writer off if they did something useless like:
            //  ~ 5 + 4
            // And even:
            //  ~ false && myFunction()
            // ...since it's bad practice, and won't do what they expect if
            // they're expecting C's lazy evaluation.
            if (result is Expression && !(result is FunctionCall || result is IncDecExpression) ) {
                Error ("Logic following a '~' can't be that type of expression. It can only be something like:\n\t~ include ...\n\t~ return\n\t~ var x = blah\n\t~ x++\n\t~ myFunction()");
            }

            // A function call on its own line could result in a text side effect, in which case
            // it needs a newline on the end. e.g.
            //  ~ printMyName()
            // If no text gets printed, then the extra newline will have to be culled later.
            if (result is FunctionCall) {
                result = new ContentList (result, new Parsed.Text ("\n"));
            }

            return result as Parsed.Object;
        }

        protected Parsed.Object VariableDeclaration()
        {
            Whitespace ();

            var id = Parse (Identifier);
            if (id != "VAR")
                return null;

            Whitespace ();

            var varName = Expect (Identifier, "variable name") as string;

            Whitespace ();

            Expect (String ("="), "the '=' for an assignment of a value, e.g. '= 5' (initial values are mandatory)");

            Whitespace ();

            var expr = Expect (Expression, "initial value for ") as Parsed.Expression;
            if (!(expr is Number || expr is StringExpression || expr is DivertTarget || expr is VariableReference)) {
                Error ("initial value for a variable must be a number, constant, or divert target");
            }

            var result = new VariableAssignment (varName, expr);
            result.isGlobalDeclaration = true;
            return result;
        }


        protected Parsed.Object ConstDeclaration()
        {
            Whitespace ();

            var id = Parse (Identifier);
            if (id != "CONST")
                return null;

            Whitespace ();

            var varName = Expect (Identifier, "constant name") as string;

            Whitespace ();

            Expect (String ("="), "the '=' for an assignment of a value, e.g. '= 5' (initial values are mandatory)");

            Whitespace ();

            var expr = Expect (Expression, "initial value for ") as Parsed.Expression;
            if (!(expr is Number || expr is DivertTarget)) {
                Error ("initial value for a constant must be a number or divert target");
            }

            var result = new ConstantDeclaration (varName, expr);
            return result;
        }

        protected object IncludeStatement()
        {
            if (ParseString ("include") == null)
                return null;

            Whitespace ();

            var filename = (string) Expect(() => ParseUntilCharactersFromString ("\n\r"), "filename for include statement");
            filename = filename.TrimEnd (' ', '\t');

            var fullFilename = filename;
            if (_rootDirectory != null) {
                fullFilename = System.IO.Path.Combine (_rootDirectory, filename);
            }

            Parsed.Story includedStory = null;
            try {
                string includedString = System.IO.File.ReadAllText(fullFilename);

                InkParser parser = new InkParser(includedString, filename, _rootDirectory);
                includedStory = parser.Parse();

                if( includedStory == null ) {
                    // This error should never happen: if the includedStory isn't
                    // returned, then it should've been due to some error that
                    // has already been reported, so this is a last resort.
                    if( !parser.hadError ) {
                        Error ("Failed to parse included file '" + filename);
                    }
                }

            }
            catch {
                string message = "Included file not found: " + filename;
                if (_rootDirectory != null)
                    message += "' (searching in " + _rootDirectory + ")";
                Error (message);
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

            ContentList contentList = logic as ContentList;
            if (!contentList) {
                contentList = new ContentList (logic);
            }

            // Create left-glue. Like normal glue, except it only absorbs newlines to
            // the left, ensuring that the logic is inline, but without having the side effect
            // of possibly absorbing desired newlines that come after.
            var rightGlue = new Parsed.Wrap<Runtime.Glue>(new Runtime.Glue (Runtime.GlueType.Right));
            var leftGlue = new Parsed.Wrap<Runtime.Glue>(new Runtime.Glue (Runtime.GlueType.Left));
            contentList.InsertContent (0, rightGlue);
            contentList.AddContent (leftGlue);
                
            Whitespace ();

            Expect (String("}"), "closing brace '}' for inline logic");

            return contentList;
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

        // Note: we allow identifiers that start with a number,
        // but not if they *only* comprise numbers
        protected string Identifier()
        {
            if (_identifierCharSet == null) {
                _identifierCharSet = new CharacterSet ();
                _identifierCharSet.AddRange ('A', 'Z');
                _identifierCharSet.AddRange ('a', 'z');
                _identifierCharSet.AddRange ('0', '9');
                _identifierCharSet.Add ('_');
            }

            // Parse remaining characters (if any)
            var name = ParseCharactersFromCharSet (_identifierCharSet);
            if (name == null)
                return null;

            // Reject if it's just a number
            bool isNumberCharsOnly = true;
            foreach (var c in name) {
                if ( !(c >= '0' && c <= '9') ) {
                    isNumberCharsOnly = false;
                    break;
                }
            }
            if (isNumberCharsOnly) {
                return null;
            }

            return name;
        }
            
        private CharacterSet _identifierCharSet;
    }
}

