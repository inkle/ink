using System;
using System.Collections.Generic;

namespace Ink
{
    /// <summary>
    /// Given the text of an ink file without any comments in it (cleansed by Comment Eliminator),
    /// this pass simply detects INCLUDE lines and returns the filenames. Used by the ink-Unity-integration
    /// plugin in order to determine the INCLUDE structure within a project.
    /// </summary>
    internal class IncludeExtractor : StringParser
    {
        public IncludeExtractor(string input) : base(input)
        {
            _includeFilenames = new List<string> ();
        }

        public List<string> ExtractIncludes()
        {
            // For each line, try an INCLUDE first, otherwise continue
            while (!endOfInput) {
                Whitespace ();

                OneOf (IncludeLine, OtherLine);
            }

            return _includeFilenames;
        }

        string IncludeLine()
        {
            if (ParseString ("INCLUDE") == null)
                return null;

            Whitespace ();

            string inkFilename = ParseUntilCharactersFromCharSet (_newlineCharacterSet);

            if( inkFilename != null )
                inkFilename = inkFilename.Trim ();

            if (inkFilename == null || inkFilename.Length == 0) {
                Error ("Expected filename after INCLUDE declaration");

                // Parse until the end of the line
                OtherLine ();
                return null;
            }

            _includeFilenames.Add (inkFilename);

            ParseNewline ();

            return inkFilename;
        }

        object OtherLine()
        {
            ParseUntilCharactersFromCharSet (_newlineCharacterSet);

            ParseNewline ();

            return ParseSuccess;
        }

        string Whitespace()
        {
            return ParseCharactersFromCharSet (_whitespaceCharacterSet);
        }

        List<string> _includeFilenames;
        CharacterSet _newlineCharacterSet = new CharacterSet("\r\n");
        CharacterSet _whitespaceCharacterSet = new CharacterSet("\t ");
    }


}

