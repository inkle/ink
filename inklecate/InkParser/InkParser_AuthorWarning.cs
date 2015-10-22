using Inklewriter.Parsed;
//using System.Text;
//using System.Collections.Generic;

namespace Inklewriter
{
    internal partial class InkParser
    {
        protected AuthorWarning AuthorWarning()
        {
            Whitespace ();

            if (ParseString ("TODO") == null)
                return null;

            Whitespace ();

            ParseString (":");

            Whitespace ();

            var message = ParseUntilCharactersFromString ("\n\r");

            return new AuthorWarning (message);
        }

    }
}

