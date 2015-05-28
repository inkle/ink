using System;
using System.Text;

namespace Inklewriter
{
    // Perform comment elimination as a pre-pass to simplify the parse rules in the main parser
    public class CommentEliminator : StringParser
    {
        public CommentEliminator (string input) : base(input)
        {
            _commentStartCharacter = new CharacterSet ("/");
            _newlineCharacters = new CharacterSet ("\n\r");
            _commentBlockEndPauseCharacters = new CharacterSet ("\n\r*");
        }

        public string Process()
        {
            var stringList = Interleave<string>(Optional (Comment), NonComment);

            return string.Join ("", stringList);
        }

        string NonComment()
        {
            return ParseUntil (Comment, _commentStartCharacter, null);
        }

        // Valid comments always return either an empty string or pure newlines,
        // which we want to keep so that line numbers stay the same
        string Comment()
        {
            return (string) OneOf (EndOfLineComment, BlockComment);
        }

        string EndOfLineComment()
        {
            if (ParseString ("//") == null) {
                return null;
            }

            ParseUntilCharactersFromCharSet (_newlineCharacters);

            return "";
        }

        string BlockComment()
        {
            if (ParseString ("/*") == null) {
                return null;
            }

            var newlinesOnly = new StringBuilder ();

            do {
                
                ParseUntilCharactersFromCharSet (_commentBlockEndPauseCharacters);

                // Parse any newlines - it's important that we retain them so that line numbers remain consistent
                string newlines = ParseCharactersFromCharSet(_newlineCharacters);
                if (newlines != null && newlines.Length > 0) {
                    newlinesOnly.Append (newlines);
                } 

                // Parse end of comment block
                else if (ParseString ("*/") != null) {
                    break;
                }

                // Step past pause character (i.e. likely to be a '*')
                else {
                    index++;
                }

            } while(true);

            return newlinesOnly.ToString();
        }
          
        CharacterSet _commentStartCharacter;
        CharacterSet _commentBlockEndPauseCharacters;
        CharacterSet _newlineCharacters;
    }
}

