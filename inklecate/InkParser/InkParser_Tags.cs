using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Ink
{
    internal partial class InkParser
    {
        protected Runtime.Tag Tag ()
        {
            Whitespace ();

            if (ParseString ("#") == null)
                return null;

            Whitespace ();

            var sb = new StringBuilder ();
            do {
                // Read up to another #, end of input or newline
                string tagText = ParseUntilCharactersFromCharSet (_endOfTagCharSet);
                sb.Append (tagText);

                // Escape character
                if (ParseString ("\\") != null) {
                    char c = ParseSingleCharacter ();
                    if( c != (char)0 ) sb.Append(c);
                    continue;
                }

                break;
            } while ( true );

            var fullTagText = sb.ToString ();

            return new Runtime.Tag (fullTagText);
        }

        protected List<Runtime.Tag> Tags ()
        {
            var tags = OneOrMore (Tag);
            if (tags == null) return null;

            return tags.Cast<Runtime.Tag>().ToList();
        }

        protected void ParseTagsAndAddTo (Parsed.Object parsedObj)
        {
            // Try parsing an end of line tag
            var tags = Parse (Tags);
            if (tags != null) {
                parsedObj.AddTags (tags);
            }
        }

        CharacterSet _endOfTagCharSet = new CharacterSet ("#\n\r\\");
    }
}

