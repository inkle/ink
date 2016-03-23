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

