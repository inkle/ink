using System.Collections.Generic;
using System.Linq;
using Ink.Parsed;

namespace Ink
{
    public partial class InkParser
    {
        protected Sequence InnerSequence()
        {
            Whitespace ();

            // Default sequence type
            SequenceType seqType = SequenceType.Stopping;

            // Optional explicit sequence type
            SequenceType? parsedSeqType = (SequenceType?) Parse(SequenceTypeAnnotation);
            if (parsedSeqType != null)
                seqType = parsedSeqType.Value;

            var contentLists = Parse(InnerSequenceObjects);
            if (contentLists == null || contentLists.Count <= 1) {
                return null;
            }

            return new Sequence (contentLists, seqType);
        }

        protected object SequenceTypeAnnotation()
        {
            var annotation = (SequenceType?) Parse(SequenceTypeSymbolAnnotation);

            if(annotation == null)
                annotation = (SequenceType?) Parse(SequenceTypeWordAnnotation);

            if (annotation == null)
                return null;
                
            switch (annotation.Value)
            {
                case SequenceType.Once:
                case SequenceType.Cycle:
                case SequenceType.Stopping:
                case SequenceType.Shuffle:
                case (SequenceType.Shuffle | SequenceType.Stopping):
                case (SequenceType.Shuffle | SequenceType.Once):
                    break;

                default:
                    Error("Sequence type combination not supported: " + annotation.Value);
                    return SequenceType.Stopping;
            }

            return annotation;
        }

        protected object SequenceTypeSymbolAnnotation()
        {
            if(_sequenceTypeSymbols == null )
                _sequenceTypeSymbols = new CharacterSet("!&~$ ");

            var sequenceType = (SequenceType)0;
            var sequenceAnnotations = ParseCharactersFromCharSet(_sequenceTypeSymbols);
            if (sequenceAnnotations == null)
                return null;

            foreach(char symbolChar in sequenceAnnotations) {
                switch(symbolChar) {
                    case '!': sequenceType |= SequenceType.Once; break;
                    case '&': sequenceType |= SequenceType.Cycle; break;
                    case '~': sequenceType |= SequenceType.Shuffle; break;
                    case '$': sequenceType |= SequenceType.Stopping; break;
                }
            }

            if (sequenceType == (SequenceType)0)
                return null;

            return sequenceType;
        }

        CharacterSet _sequenceTypeSymbols = new CharacterSet("!&~$");

        protected object SequenceTypeWordAnnotation()
        {
            var sequenceTypes = Interleave<SequenceType?>(SequenceTypeSingleWord, Exclude(Whitespace));
            if (sequenceTypes == null || sequenceTypes.Count == 0)
                return null;

            if (ParseString (":") == null)
                return null;

            var combinedSequenceType = (SequenceType)0;
            foreach(var seqType in sequenceTypes) {
                combinedSequenceType |= seqType.Value;
            }

            return combinedSequenceType;
        }

        protected object SequenceTypeSingleWord()
        {
            SequenceType? seqType = null;

            var word = Parse(Identifier);
            switch (word)
            {
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
            var interleavedContentAndPipes = Interleave<object> (Optional (MixedTextAndLogic), String ("|"), flatten:false);
            if (interleavedContentAndPipes == null)
                return null;

            var result = new List<ContentList> ();

            // The content and pipes won't necessarily be perfectly interleaved in the sense that
            // the content can be missing, but in that case it's intended that there's blank content.
            bool justHadContent = false;
            foreach (object contentOrPipe in interleavedContentAndPipes) {

                // Pipe/separator
                if (contentOrPipe as string == "|") {

                    // Expected content, saw pipe - need blank content now
                    if (!justHadContent) {

                        // Add blank content
                        result.Add (new ContentList ());
                    }

                    justHadContent = false;
                } 

                // Real content
                else {

                    var content = contentOrPipe as List<Parsed.Object>;
                    if (content == null) {
                        Error ("Expected content, but got " + contentOrPipe + " (this is an ink compiler bug!)");
                    } else {
                        result.Add (new ContentList (content));
                    }

                    justHadContent = true;
                }
            }

            // Ended in a pipe? Need to insert final blank content
            if (!justHadContent)
                result.Add (new ContentList ());
                
            return result;
        }

        protected List<ContentList> InnerMultilineSequenceObjects()
        {
            MultilineWhitespace ();

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

            if (content == null)
                MultilineWhitespace ();

            // Add newline at the start of each branch
            else {
                content.Insert (0, new Parsed.Text ("\n"));
            }

            return new ContentList (content);
        }
    }
}

