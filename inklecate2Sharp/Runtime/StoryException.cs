namespace Inklewriter.Runtime
{
    public class StoryException : System.Exception
    {
        public bool useEndLineNumber;

        public StoryException () { }
        public StoryException(string message) : base(message) {}
    }
}

