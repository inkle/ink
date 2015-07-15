
namespace Inklewriter.Runtime
{
    public class VariableReference : Runtime.Object
    {
        // Normal named variable
        public string name { get; set; }

        // Variable reference is actually a path for a visit (read) count
        public Path pathForVisitCount { get; set; }

        public VariableReference (string name)
        {
            this.name = name;
        }

        public override string ToString ()
        {
            return name;
        }
    }
}

