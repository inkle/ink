using Ink.Parsed;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Ink
{
    public partial class InkParser
    {
        void TrimEndWhitespace(List<Parsed.Object> mixedTextAndLogicResults, bool terminateWithSpace)
        {
            // Trim whitespace from end
            if (mixedTextAndLogicResults.Count > 0) {
                var lastObjIdx = mixedTextAndLogicResults.Count - 1;
                var lastObj = mixedTextAndLogicResults[lastObjIdx];
                if (lastObj is Text) {
                    var text = (Text)lastObj;
                    text.text = text.text.TrimEnd (' ', '\t');

                    if (terminateWithSpace)
                        text.text += " ";

                    // No content left at all? trim the whole object
                    else if( text.text.Length == 0 ) {
                        mixedTextAndLogicResults.RemoveAt(lastObjIdx);

                        // Recurse in case there's more whitespace
                        TrimEndWhitespace(mixedTextAndLogicResults, terminateWithSpace:false);
                    }
                }
            }
        }

        protected List<Parsed.Object> LineOfMixedTextAndLogic()
        {
            // Consume any whitespace at the start of the line
            // (Except for escaped whitespace)
            Parse (Whitespace);

            var result = Parse(MixedTextAndLogic);
            if (result == null || result.Count == 0)
                return null;

            // Warn about accidentally writing "return" without "~"
            var firstText = result[0] as Text;
            if (firstText) {
                if (firstText.text.StartsWith ("return")) {
                    Warning ("Do you need a '~' before 'return'? If not, perhaps use a glue: <> (since it's lowercase) or rewrite somehow?");
                }
            }
            if (result.Count == 0)
                return null;

            var lastObj = result [result.Count - 1];
            if (!(lastObj is Divert)) {
                TrimEndWhitespace (result, terminateWithSpace:false);
            }

            EndTagIfNecessary(result);

            // If the line doens't actually contain any normal text content
            // but is in fact entirely a tag, then let's not append
            // a newline, since we want the tag (or tags) to be associated
            // with the line below rather than being completely independent.
            bool lineIsPureTag = result.Count > 0 && result[0] is Parsed.Tag && ((Parsed.Tag)result[0]).isStart;
            if( !lineIsPureTag )
                result.Add (new Text ("\n"));

            Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

            return result;
        }

        protected List<Parsed.Object> MixedTextAndLogic()
        {
            // Check for disallowed "~" within this context
            var disallowedTilda = ParseObject(Spaced(String("~")));
            if (disallowedTilda != null)
                Error ("You shouldn't use a '~' here - tildas are for logic that's on its own line. To do inline logic, use { curly braces } instead");

            // Either, or both interleaved
            var results = Interleave<Parsed.Object>(Optional (ContentText), Optional (InlineLogicOrGlueOrStartTag));

            // Terminating divert?
            // (When parsing content for the text of a choice, diverts aren't allowed.
            //  The divert on the end of the body of a choice is handled specially.)
            if (!_parsingChoice) {

                var diverts = Parse (MultiDivert);
                if (diverts != null) {

                    // May not have had any results at all if there's *only* a divert!
                    if (results == null)
                        results = new List<Parsed.Object> ();

                    // End previously active tag if necessary
                    EndTagIfNecessary(results);

                    TrimEndWhitespace (results, terminateWithSpace:true);

                    results.AddRange (diverts);
                }

            }
                
            if (results == null)
                return null;

            return results;
        }

        protected Parsed.Text ContentText()
        {
            return ContentTextAllowingEcapeChar ();
        }

        protected Parsed.Text ContentTextAllowingEcapeChar()
        {
            StringBuilder sb = null;

            do {
                var str = Parse(ContentTextNoEscape);
                bool gotEscapeChar = ParseString(@"\") != null;

                if( gotEscapeChar || str != null ) {
                    if( sb == null ) {
                        sb = new StringBuilder();
                    }

                    if( str != null ) {
                        sb.Append(str);
                    }

                    if( gotEscapeChar ) {
                        char c = ParseSingleCharacter();
                        sb.Append(c);
                    }

                } else {
                    break;
                }

            } while(true);

            if (sb != null ) {
                return new Parsed.Text (sb.ToString ());

            } else {
                return null;
            }
        }

        // Content text is an unusual parse rule compared with most since it's
        // less about saying "this is is the small selection of stuff that we parse"
        // and more "we parse ANYTHING except this small selection of stuff".
        protected string ContentTextNoEscape()
        {
            // Eat through text, pausing at the following characters, and
            // attempt to parse the nonTextRule.
            // "-": possible start of divert or start of gather
            // "<": possible start of glue
            if (_nonTextPauseCharacters == null) {
                _nonTextPauseCharacters = new CharacterSet ("-<");
            }

            // If we hit any of these characters, we stop *immediately* without bothering to even check the nonTextRule
            // "{" for start of logic
            // "|" for mid logic branch
            if (_nonTextEndCharacters == null) {
                _nonTextEndCharacters = new CharacterSet ("{}|\n\r\\#");
                _notTextEndCharactersChoice = new CharacterSet (_nonTextEndCharacters);
                _notTextEndCharactersChoice.AddCharacters ("[]");
                _notTextEndCharactersString = new CharacterSet (_nonTextEndCharacters);
                _notTextEndCharactersString.AddCharacters ("\"");
            }

            // When the ParseUntil pauses, check these rules in case they evaluate successfully
            ParseRule nonTextRule = () => OneOf (ParseDivertArrow, ParseThreadArrow, EndOfLine, Glue);

            CharacterSet endChars = null;
            if (parsingStringExpression) {
                endChars = _notTextEndCharactersString;
            } 
            else if (_parsingChoice) {
                endChars = _notTextEndCharactersChoice;
            } 
            else {
                endChars = _nonTextEndCharacters;
            }

            string pureTextContent = ParseUntil (nonTextRule, _nonTextPauseCharacters, endChars);
            if (pureTextContent != null ) {
                return pureTextContent;

            } else {
                return null;
            }

        }

        CharacterSet _nonTextPauseCharacters;
        CharacterSet _nonTextEndCharacters;
        CharacterSet _notTextEndCharactersChoice;
        CharacterSet _notTextEndCharactersString;



    }
}

