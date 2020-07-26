using System.Collections.Generic;

namespace Ink
{
    public class CompilerOptions
    {
        public string sourceFilename;
        public List<string> pluginNames;
        public bool countAllVisits;
        public Ink.InkParser.IFileHandler fileHandler;
    }
}
