using Newtonsoft.Json;

namespace Inklewriter.Runtime
{
    internal class VariableReference : Runtime.Object
    {
        // Normal named variable
        [JsonProperty("get")]
        [UniqueJsonIdentifier]
        public string name { get; set; }

        // Variable reference is actually a path for a visit (read) count
        public Path pathForVisitCount { get; set; }

        [JsonProperty("readCount")]
        [UniqueJsonIdentifier]
        public string pathStringForVisitCount { 
            get {
                if( pathForVisitCount == null )
                    return null;

                return pathForVisitCount.componentsString;
            }
            set {
                if (value == null)
                    pathForVisitCount = null;
                else
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

