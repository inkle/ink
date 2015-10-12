using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
    internal partial class InkParser
    {
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
            var contentLists = OneOrMore (SingleMultilineSequenceElement);
            if (contentLists == null)
                return null;

            return contentLists.Cast<ContentList> ().ToList();
        }

        protected ContentList SingleMultilineSequenceElement()
        {
            Whitespace ();

            // Make sure we're not accidentally parsing a divert
            if (ParseString ("->") != null)
                return null;

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

