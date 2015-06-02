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
            return string.Format ("Line {0} of {1}", lineNumber, fileName);
        }
	}
}

