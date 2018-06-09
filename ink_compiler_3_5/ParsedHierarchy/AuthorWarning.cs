
namespace Ink.Parsed
{
    public class AuthorWarning : Parsed.Object
    {
        public string warningMessage;

        public AuthorWarning(string message)
        {
            warningMessage = message;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            Warning (warningMessage);
            return null;
        }
    }
}

