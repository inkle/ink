
namespace Inklewriter.Parsed
{
    internal class AuthorWarning : Parsed.Object
    {
        public string warningMessage;

        public AuthorWarning(string message)
        {
            warningMessage = message;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            Warning (string.Format("\"{0}\"", warningMessage));
            return null;
        }
    }
}

