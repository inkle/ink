using System;

namespace Inklewriter.Runtime
{
	public class Divert : Runtime.Object
	{
		public Path targetPath { get; set; }

		public Divert ()
		{
		}

        public override string ToString ()
        {
            if (targetPath == null) {
                return "Divert(null)";
            } else {
                return "Divert(" + targetPath.ToString () + ")";
            }
        }
	}
}

