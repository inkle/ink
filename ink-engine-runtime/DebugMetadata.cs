namespace Ink.Runtime
{
	public class DebugMetadata
	{
		public int startLineNumber;
        public int endLineNumber;
		public string fileName;
        public string sourceName;

		public DebugMetadata ()
		{
		}

        public override string ToString ()
        {
            if (fileName != null) {
                return string.Format ("line {0} of {1}", startLineNumber, fileName);
            } else {
                return "line " + startLineNumber;
            }

        }
	}
}

