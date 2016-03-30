namespace Ink.Runtime
{
	internal class DebugMetadata
	{
		internal int startLineNumber = 0;
        internal int endLineNumber = 0;
        internal string fileName = null;
        internal string sourceName = null;

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

