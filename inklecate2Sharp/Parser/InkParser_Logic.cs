using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
    public partial class InkParser
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

            return (Parsed.Object) Expect(afterTilda, "expression after '~'", recoveryRule: SkipToNextLine);
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

        void TrimEndWhitespaceAndAddNewline(List<Parsed.Object> mixedTextAndLogicResults)
        {
            // Trim whitespace from end and add a newline
            if (mixedTextAndLogicResults.Count > 0) {
                var lastObj = mixedTextAndLogicResults[mixedTextAndLogicResults.Count-1];
                if (lastObj is Text) {
                    var text = (Text)lastObj;
                    text.text = text.text.TrimEnd (' ', '\t') + "\n";
                    return;
                }
            }
                
            // Otherwise, last object in line wasn't text (but some kind of logic), so
            // we need to append the newline afterwards using a new object
            // If we end up generating multiple newlines (e.g. due to conditional
            // logic), we rely on the runtime to absorb them.
            // TODO: Is there some more clever logic we can do here?
            mixedTextAndLogicResults.Add (new Text ("\n"));
        }

        protected List<Parsed.Object> LineOfMixedTextAndLogic()
        {
            var result = Parse(MixedTextAndLogic);
            if (result == null || result.Count == 0)
                return null;

            // Trim whitepace from start
            var firstText = result[0] as Text;
            if (firstText != null) {
                firstText.text = firstText.text.TrimStart(' ', '\t');
                if (firstText.text.Length == 0) {
                    result.RemoveAt (0);
                }
            }
            if (result.Count == 0)
                return null;

            var lastObj = result [result.Count - 1];
            if (!(lastObj is Divert)) {
                TrimEndWhitespaceAndAddNewline (result);
            }

            Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

            return result;
        }

        protected List<Parsed.Object> MixedTextAndLogic()
        {
            // Either, or both interleaved
            var results = Interleave<Parsed.Object>(Optional (ContentText), Optional (InlineLogicOrGlue));

            // Terminating divert?
            var divert = Parse (Divert);
            if (divert != null) {

                // May not have had any results at all if there's *only* a divert!
                if (results == null)
                    results = new List<Parsed.Object> ();

                TrimEndWhitespaceAndAddNewline (results);

                results.Add (divert);
            }

            if (results == null)
                return null;

            return results;
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

            var logic = (Parsed.Object) Expect(InnerLogic, "Expected some kind of logic within braces: { ... }");
                
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
            if (initialQueryExpression != null) {
                var conditional = Parse(() => InnerConditionalContent (initialQueryExpression));
                if (conditional != null)
                    return conditional;
            }

            // Now try to evaluate each of the "full" rules in turn
            ParseRule[] rules = {
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
                if (result != null) {

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
            if (expr != null) {
                expr.outputWhenComplete = true;
            }
            return expr;
        }

        protected Sequence InnerSequence()
        {
            Whitespace ();

            // Default sequence type
            SequenceType seqType = SequenceType.Stopping;

            // Optional explicit sequence type
            SequenceType? parsedSeqType = (SequenceType?) Parse(SequenceTypeAnnotation);
            if (parsedSeqType != null)
                seqType = (SequenceType) parsedSeqType;

            var contentLists = Parse(InnerSequenceObjects);
            if (contentLists == null || contentLists.Count <= 1) {
                return null;
            }

            return new Sequence (contentLists, seqType);
        }

        protected object SequenceTypeAnnotation()
        {
            var symbolAnnotation = Parse(SequenceTypeSymbolAnnotation);
            if (symbolAnnotation != null)
                return symbolAnnotation;

            var wordAnnotation = Parse(SequenceTypeWordAnnotation);
            if (wordAnnotation != null)
                return wordAnnotation;

            return null;
        }

        protected object SequenceTypeSymbolAnnotation()
        {
            var symbol = ParseSingleCharacter ();

            switch (symbol) {
            case '!':
                return SequenceType.Once;
            case '&':
                return SequenceType.Cycle;
            case '~':
                return SequenceType.Shuffle;
            case '$':
                return SequenceType.Stopping;
            }

            return null;
        }

        protected object SequenceTypeWordAnnotation()
        {
            SequenceType? seqType = null;

            var word = Parse(Identifier);
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
                return null;

            Whitespace ();

            if (ParseString (":") == null)
                return null;

            return seqType;
        }

        protected List<ContentList> InnerSequenceObjects()
        {
            var multiline = Parse(Newline) != null;

            List<ContentList> result = null;
            if (multiline) {
                result = Parse(InnerMultilineSequenceObjects);
            } else {
                result = Parse(InnerInlineSequenceObjects);
            }

            return result;
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
            Whitespace ();

            if (ParseString ("-") == null)
                return null;

            Whitespace ();

            List<Parsed.Object> content = StatementsAtLevel (StatementLevel.InnerBlock);
            if (content == null) {
                Error ("expected content for the sequence element following '-'");
            }

            return new ContentList (content);
        }
    }
}

