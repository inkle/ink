
namespace Inklewriter.Runtime
{
    public class Error : Runtime.Object
    {
        public string message;
        public bool useEndLineNumber;

        public Error (string message)
        {
            this.message = message;
        }

        public override string ToString ()
        {
            return string.Format("Error: '{0}'", this.message);
        }
    }
}

