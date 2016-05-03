using Ink.Parsed;
using System.Collections.Generic;
using System.IO;


namespace Ink
{
    internal partial class InkParser
    {
        protected object IncludeStatement()
        {
            Whitespace ();

            if (ParseString ("INCLUDE") == null)
                return null;

            Whitespace ();

            var filename = (string) Expect(() => ParseUntilCharactersFromString ("\n\r"), "filename for include statement");
            filename = filename.TrimEnd (' ', '\t');

            // Working directory should already have been set up relative to the root ink file.
            var workingDirectory = Directory.GetCurrentDirectory ();
            var fullFilename = System.IO.Path.Combine (workingDirectory, filename);

            if (FilenameIsAlreadyOpen (fullFilename)) {
                Error ("Recursive INCLUDE detected: '" + fullFilename + "' is already open.");
                ParseUntilCharactersFromString("\r\n");
                return new IncludedFile(null);
            } else {
                AddOpenFilename (fullFilename);
            }

            Parsed.Story includedStory = null;
            string includedString = null;
            try {
                includedString = File.ReadAllText(fullFilename);
            }
            catch {
                Error ("Failed to load: '"+filename+"' (relative to directory: "+workingDirectory+")");
            }


            if (includedString != null ) {
                InkParser parser = new InkParser(includedString, filename, _externalErrorHandler, _rootParser);
                includedStory = parser.Parse();

                if( includedStory == null ) {
                    // This error should never happen: if the includedStory isn't
                    // returned, then it should've been due to some error that
                    // has already been reported, so this is a last resort.
                    if( !parser.hadError ) {
                        Error ("Failed to parse included file '" + filename);
                    }
                }
            }

            RemoveOpenFilename (fullFilename);

            // Return valid IncludedFile object even when story failed to parse and we have a null story:
            // we don't want to attempt to re-parse the include line as something else
            return new IncludedFile (includedStory);
        }

        bool FilenameIsAlreadyOpen(string fullFilename)
        {
            return _rootParser._openFilenames.Contains (fullFilename);
        }

        void AddOpenFilename(string fullFilename)
        {
            _rootParser._openFilenames.Add (fullFilename);
        }

        void RemoveOpenFilename(string fullFilename)
        {
            _rootParser._openFilenames.Remove (fullFilename);
        }
                   
        InkParser _rootParser;
        HashSet<string> _openFilenames;
    }
}

