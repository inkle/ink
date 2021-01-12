using Ink.Parsed;
using System.Collections.Generic;
using System.IO;


namespace Ink
{
    public partial class InkParser
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
            var fullFilename = _rootParser._fileHandler.ResolveInkFilename (filename);

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
                includedString = _rootParser._fileHandler.LoadInkFileContents(fullFilename);
            }
            catch {
                Error ("Failed to load: '"+filename+"'");
            }


            if (includedString != null ) {
                InkParser parser = new InkParser(includedString, filename, _externalErrorHandler, _rootParser);
                includedStory = parser.Parse();
            }

            RemoveOpenFilename (fullFilename);

            // Return valid IncludedFile object even if there were errors when parsing.
            // We don't want to attempt to re-parse the include line as something else,
            // and we want to include the bits that *are* valid, so we don't generate
            // more errors than necessary.
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

