using System;

namespace Inklewriter.Runtime
{
	public class DebugMetadata
	{
		public int lineNumber;
		public string fileName;

		public DebugMetadata ()
		{
		}

        public override string ToString ()
        {
            if (fileName != null) {
                return string.Format ("line {0} of {1}", lineNumber, fileName);
            } else {
                return "line " + lineNumber;
            }

        }
	}
}

