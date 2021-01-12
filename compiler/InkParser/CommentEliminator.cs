
namespace Ink
{
    /// <summary>
    /// Pre-pass before main ink parser runs. It actually performs two main tasks:
    ///  - comment elimination to simplify the parse rules in the main parser
    ///  - Conversion of Windows line endings (\r\n) to the simpler Unix style (\n), so
    ///    we don't have to worry about them later.
    /// </summary>
    public class CommentEliminator : StringParser
    {
        public CommentEliminator (string input) : base(input)
        {
        }

        public string Process()
        {
            // Make both comments and non-comments optional to handle trivial empty file case (or *only* comments)
            var stringList = Interleave<string>(Optional (CommentsAndNewlines), Optional(MainInk));

            if (stringList != null) {
                return string.Join("", stringList.ToArray());
            } else {
                return null;
            }
        }

        string MainInk()
        {
            return ParseUntil (CommentsAndNewlines, _commentOrNewlineStartCharacter, null);
        }

        string CommentsAndNewlines()
        {
            var newlines = Interleave<string> (Optional (ParseNewline), Optional (ParseSingleComment));

            if (newlines != null) {
                return string.Join ("", newlines.ToArray());
            } else {
                return null;
            }
        }

        // Valid comments always return either an empty string or pure newlines,
        // which we want to keep so that line numbers stay the same
        string ParseSingleComment()
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
          
        CharacterSet _commentOrNewlineStartCharacter = new CharacterSet ("/\r\n");
        CharacterSet _commentBlockEndCharacter = new CharacterSet("*");
        CharacterSet _newlineCharacters = new CharacterSet ("\n\r");
    }
}

