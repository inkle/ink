
namespace Inklewriter
{
    // Perform comment elimination as a pre-pass to simplify the parse rules in the main parser
    public class CommentEliminator : StringParser
    {
        public CommentEliminator (string input) : base(input)
        {
        }

        public string Process()
        {
            // Make both comments and non-comments optional to handle trivial empty file case (or *only* comments)
            var stringList = Interleave<string>(Optional (Comment), Optional(NonComment));

            if (stringList != null) {
                return string.Join ("", stringList);
            } else {
                return null;
            }
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

            int startLineIndex = lineIndex;

            var commentResult = ParseUntil (String("*/"), _commentBlockEndCharacter, null);

            if (!endOfInput) {
                ParseString ("*/");
            }

            // Count the number of lines that were inside the block, and replicate them as newlines
            // so that the line indexing still works from the original source
            if (commentResult != null) {
                return new string ('\n', lineIndex - startLineIndex);
            } 

            // No comment at all
            else {
                return null;
            }
        }
          
        CharacterSet _commentStartCharacter = new CharacterSet ("/");
        CharacterSet _commentBlockEndCharacter = new CharacterSet("*");
        CharacterSet _newlineCharacters = new CharacterSet ("\n\r");
    }
}

