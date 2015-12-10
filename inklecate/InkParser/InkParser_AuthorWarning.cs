using Ink.Parsed;
//using System.Text;
//using System.Collections.Generic;

namespace Ink
{
    internal partial class InkParser
    {
        protected AuthorWarning AuthorWarning()
        {
            Whitespace ();

            if (Parse (Identifier) != "TODO")
                return null;

            Whitespace ();

            ParseString (":");

            Whitespace ();

            var message = ParseUntilCharactersFromString ("\n\r");

            return new AuthorWarning (message);
        }

    }
}

