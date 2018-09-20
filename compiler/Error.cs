namespace Ink
{
    public delegate void ErrorHandler(string message, ErrorType type);

    public enum ErrorType
    {
        Author,
        Warning,
        Error
    }
}

