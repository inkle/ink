namespace Ink.Runtime
{
	public class DebugMetadata
	{
		public int startLineNumber = 0;
        public int endLineNumber = 0;
        public string fileName = null;
        public string sourceName = null;

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

