using Newtonsoft.Json;

namespace Ink.Runtime
{
    internal enum GlueType
    {
        Bidirectional,
        Left,
        Right
    }

    internal class Glue : Runtime.Object
    {
        public GlueType glueType { get; set; }

        // Ensure that the default value of "0" doesn't cause the field
        // to be ommited, since the very presence of the key is important.
        [JsonProperty("<>", DefaultValueHandling=DefaultValueHandling.Include)]
        [UniqueJsonIdentifier]
        public int glueTypeInt {
            get {
                return (int)glueType;
            }
            set {
                glueType = (GlueType)value;
            }
        }

        public bool isLeft {
            get {
                return glueType == GlueType.Left;
            }
        }

        public bool isBi {
            get {
                return glueType == GlueType.Bidirectional; 
            }
        }

        public bool isRight {
            get {
                return glueType == GlueType.Right;
            }
        }

        public Glue ()
        {
        }

        public Glue(GlueType type) {
            glueType = type;
        }

        public override string ToString ()
        {
            switch (glueType) {
            case GlueType.Bidirectional: return "BidirGlue";
            case GlueType.Left: return "LeftGlue";
            case GlueType.Right: return "RightGlue";
            }

            return "UnexpectedGlueType";
        }
    }
}

