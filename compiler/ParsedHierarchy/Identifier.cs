namespace Ink.Parsed {
    public class Identifier {
        public string name;
        public Runtime.DebugMetadata debugMetadata;

        public override string ToString()
        {
            return name;
        }

        public static Identifier Done = new Identifier { name = "DONE", debugMetadata = null };
    }
}
