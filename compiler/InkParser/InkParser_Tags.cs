using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Ink
{
    public partial class InkParser
    {
        protected Parsed.Tag Tag ()
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

            var fullTagText = sb.ToString ().Trim();

            return new Parsed.Tag (new Runtime.Tag (fullTagText));
        }

        protected List<Parsed.Tag> Tags ()
        {
            var tags = OneOrMore (Tag);
            if (tags == null) return null;

            return tags.Cast<Parsed.Tag>().ToList();
        }

        CharacterSet _endOfTagCharSet = new CharacterSet ("#\n\r\\");
    }
}

