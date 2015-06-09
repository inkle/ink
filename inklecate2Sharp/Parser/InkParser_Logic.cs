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
            ParseRule afterTilda = () => OneOf (IncludeStatement, ReturnStatement, VariableDeclarationOrAssignment, Expression);

            var parsedExpr = (Parsed.Object) Expect(afterTilda, "expression after '~'", recoveryRule: SkipToNextLine);

            // TODO: A piece of logic after a tilda shouldn't have its result printed as text (I don't think?)
            return SucceedRule (parsedExpr) as Parsed.Object;
        }

        protected object IncludeStatement()
        {
            BeginRule ();

            if (ParseString ("include") == null) {
                return (IncludedFile)FailRule ();
            }

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

            // Succeed even when story failed to parse and we have a null story:
            //  we don't want to attempt to re-parse the include line as something else
            var includedFile = new IncludedFile (includedStory);
            return SucceedRule (includedFile);
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
                firstText.text = firstText.text.TrimStart(' ', '\t');
                if (firstText.text.Length == 0) {
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
                text.text = text.text.TrimEnd (' ', '\t') + "\n";
            } 

            // Last object in line wasn't text (but some kind of logic), so
            // we need to append the newline afterwards using a new object
            // If we end up generating multiple newlines (e.g. due to conditional
            // logic), we rely on the runtime to absorb them.
            // TODO: Is there some more clever logic we can do here?
            else {
                result.Add (new Text ("\n"));
            }

            Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

            return (List<Parsed.Object>) SucceedRule(result);
        }

        protected List<Parsed.Object> MixedTextAndLogic()
        {
            // Either, or both interleaved
            return Interleave<Parsed.Object>(Optional (ContentText), Optional (InlineLogicOrGlue));
        }

        protected Parsed.Object InlineLogicOrGlue()
        {
            return (Parsed.Object) OneOf (InlineLogic, Glue);
        }

        protected Parsed.Wrap<Runtime.Glue> Glue()
        {
            // Don't want to parse whitespace, since it might be important
            // surrounding the glue.
            var glueStr = ParseString("::");
            if (glueStr != null) {
                var glue = new Runtime.Glue ();
                return new Parsed.Wrap<Runtime.Glue> (glue);
            } else {
                return null;
            }
        }

        protected Parsed.Object InlineLogic()
        {
            BeginRule ();

            if ( ParseString ("{") == null) {
                return FailRule () as Parsed.Object;
            }

            Whitespace ();

            var logic = InnerLogic ();
            if (logic == null) {
                return (Parsed.Object) FailRule ();
            }
                
            Whitespace ();

            Expect (String("}"), "closing brace '}' for inline logic");

            return SucceedRule(logic) as Parsed.Object;
        }

        protected Parsed.Object ExpectedInnerLogic()
        {

            var innerLogicObj = Expect(InnerLogic, 
                "inner logic or sequence between '{' and '}' braces");

            return (Parsed.Object) innerLogicObj;
        }

        protected Parsed.Object InnerLogic()
        {
            BeginRule ();

            Whitespace ();

            // Explicitly try the combinations of inner logic
            // that could potentially have conflicts first.
            // e.g.:
            //   {myBool:blah} v.s. {cycle:blah|blah2}
            //   {myVar} v.s. {my beautiful horse} -- (latter is single element cycle... perhaps an error?)
            //
            //   {                        {
            //      - cycle 1     v.s.       - x: condition 1
            //      - cycle 2                - y: condition 2
            //   }                        {

            // Explicit sequence annotation?
            SequenceType? explicitSeqType = SequenceTypeAnnotation ();
            if (explicitSeqType != null) {
                var contentLists = (List<ContentList>) Expect(InnerSequenceObjects, "sequence elements (for cycle/stoping etc)");
                var seq = new Sequence (contentLists, (SequenceType) explicitSeqType);
                return (Parsed.Object) SucceedRule (seq);
            }

            // Conditional with expression?
            var initialQueryExpression = ConditionExpression ();
            if (initialQueryExpression != null) {
                var conditional = InnerConditionalContent (initialQueryExpression);
                if (conditional != null) {
                    return (Parsed.Object)conditional;
                }
            }

            // Now try to evaluate each of the "full" rules in turn
            ParseRule[] rules = {
                InnerExpression,
                InnerSequence,
                InnerConditionalContent
            };

            // Adapted from "OneOf" structuring rule except that in 
            // order for the rule to succeed, it has to maximally 
            // cover the entire string within the { }. Used to
            // differentiate between:
            //  {myVar}                 -- Expression (try first)
            //  {my content is jolly}   -- sequence with single element
            foreach (ParseRule rule in rules) {
                BeginRule ();

                Parsed.Object result = rule () as Parsed.Object;
                if (result != null) {

                    // Not yet at end?
                    if (Peek (Spaced (String ("}"))) == null)
                        FailRule ();

                    // Full parse of content within braces
                    else
                        return (Parsed.Object) SucceedRule (result);
                    
                } else {
                    FailRule ();
                }
            }

            return null;
        }

        protected Parsed.Object InnerExpression()
        {
            var expr = Expression ();
            if (expr != null) {
                expr.outputWhenComplete = true;
            }
            return expr;
        }

        protected Sequence InnerSequence()
        {
            BeginRule ();

            Whitespace ();

            // Default sequence type
            SequenceType seqType = SequenceType.Stopping;

            // Optional explicit sequence type
            SequenceType? parsedSeqType = SequenceTypeAnnotation ();
            if (parsedSeqType != null)
                seqType = (SequenceType) parsedSeqType;

            var contentLists = InnerSequenceObjects ();
            if (contentLists == null) {
                return (Sequence) FailRule ();
            }

            var seq = new Sequence (contentLists, seqType);
            return (Sequence) SucceedRule (seq);
        }

        protected SequenceType? SequenceTypeAnnotation()
        {
            var symbolAnnotation = SequenceTypeSymbolAnnotation ();
            if (symbolAnnotation != null)
                return symbolAnnotation;

            var wordAnnotation = SequenceTypeWordAnnotation ();
            if (wordAnnotation != null)
                return wordAnnotation;

            return null;
        }

        protected SequenceType? SequenceTypeSymbolAnnotation()
        {
            var symbol = ParseCharactersFromString ("!&~$", 1);
            if (symbol != null) {
                switch (symbol) {
                case "!":
                    return SequenceType.Once;
                case "&":
                    return SequenceType.Cycle;
                case "~":
                    return SequenceType.Shuffle;
                case "$":
                    return SequenceType.Stopping;
                }
            }

            return null;
        }

        protected SequenceType? SequenceTypeWordAnnotation()
        {
            BeginRule ();

            SequenceType? seqType = null;

            var word = Identifier ();
            switch (word) {
            case "once":
                seqType = SequenceType.Once;
                break;
            case "cycle":
                seqType = SequenceType.Cycle;
                break;
            case "shuffle":
                seqType = SequenceType.Shuffle;
                break;
            case "stopping":
                seqType = SequenceType.Stopping;
                break;
            }

            if (seqType == null)
                return (SequenceType?) FailRule ();

            Whitespace ();

            if (ParseString (":") == null)
                return (SequenceType?) FailRule ();

            return (SequenceType?) SucceedRule (seqType);
        }

        protected List<ContentList> InnerSequenceObjects()
        {
            BeginRule ();

            var multiline = Newline () != null;

            List<ContentList> result = null;
            if (multiline) {
                result = InnerMultilineSequenceObjects ();
            } else {
                result = InnerInlineSequenceObjects ();
            }

            if (result == null)
                return (List<ContentList>) FailRule ();

            return (List<ContentList>) SucceedRule (result);

        }

        protected List<ContentList> InnerInlineSequenceObjects()
        {
            var listOfLists = Interleave<List<Parsed.Object>> (Optional (MixedTextAndLogic), Exclude(String ("|")), flatten:false);
            if (listOfLists == null)
                return null;

            var result = new List<ContentList> ();
            foreach (var list in listOfLists) {
                result.Add (new ContentList (list));
            }

            return result;
        }

        protected List<ContentList> InnerMultilineSequenceObjects()
        {
            return OneOrMore (SingleMultilineSequenceElement).Cast<ContentList>().ToList();
        }

        protected ContentList SingleMultilineSequenceElement()
        {
            BeginRule ();

            Whitespace ();

            if (ParseString ("-") == null)
                return (ContentList) FailRule ();

            Whitespace ();

            List<Parsed.Object> content = StatementsAtLevel (StatementLevel.InnerBlock);
            if (content == null) {
                Error ("expected content for the sequence element following '-'");
            }

            var contentList = new ContentList (content);
            return (ContentList) SucceedRule (contentList);
        }
    }
}

