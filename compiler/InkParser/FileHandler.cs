using System.IO;

namespace Ink
{
    public interface IFileHandler
    {
        string ResolveInkFilename (string includeName);
        string LoadInkFileContents (string fullFilename);
    }

    public class DefaultFileHandler : Ink.IFileHandler {
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
