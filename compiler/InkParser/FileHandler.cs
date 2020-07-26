using System.IO;

namespace Ink.InkParser
{
    public interface IFileHandler
    {
        string ResolveInkFilename (string includeName);
        string LoadInkFileContents (string fullFilename);
    }

    public class CurrentDirectoryFileHandler : IFileHandler {
        public string ResolveInkFilename (string includeName)
        {
            var workingDir = Directory.GetCurrentDirectory ();
            var fullRootInkPath = Path.Combine (workingDir, includeName);
            return fullRootInkPath;
        }

        public string LoadInkFileContents (string fullFilename)
        {
        	return File.ReadAllText (fullFilename);
        }
    }
}
