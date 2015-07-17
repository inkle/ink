using Newtonsoft.Json;

namespace Inklewriter.Runtime
{
    public class VariableReference : Runtime.Object
    {
        // Normal named variable
        [JsonProperty("var")]
        public string name { get; set; }

        // Variable reference is actually a path for a visit (read) count
        public Path pathForVisitCount { get; set; }

        [JsonProperty("readCount")]
        public string pathStringForVisitCount { 
            get {
                if( pathForVisitCount == null )
                    return null;

                return pathForVisitCount.componentsString;
            }
            set {
                if (value == null)
                    pathForVisitCount = null;

                pathForVisitCount = new Path (value);
            }
        }

        public VariableReference (string name)
        {
            this.name = name;
        }

        // Require default constructor for serialisation
        public VariableReference() {}

        public override string ToString ()
        {
            return name;
        }
    }
}

