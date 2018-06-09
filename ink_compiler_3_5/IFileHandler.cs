namespace Ink
{
    public interface IFileHandler
    {
        string ResolveInkFilename (string includeName);
        string LoadInkFileContents (string fullFilename);
    }
}
