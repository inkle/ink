using System;

namespace Inklewriter.Parsed
{
	public class Path
	{
		public string knotName { get; protected set; }
		public string stitchName { get; protected set; }
		public string ambiguousName { get; protected set; }

        public Path debugSuggestedAlternative { 
            get {
                if (this.knotName != null) {
                    return Path.ToStitch (this.knotName);
                } else if (this.stitchName != null) {
                    return Path.ToKnot (this.stitchName);
                }
                return null;
            }
        }

		protected Path(string knotName = null, string stitchName = null, string ambiguousName = null)
		{
			this.knotName = knotName;
			this.stitchName = stitchName;
			this.ambiguousName = ambiguousName;
		}

		public static Path ToStitch(string name)
		{
			return new Path(stitchName: name);
		}

		public static Path ToKnot(string name)
		{
			return new Path(knotName: name);
		}

		public static Path ToStitchInKnot(string knot, string stitch)
		{
			return new Path (knotName: knot, stitchName: stitch);
		}

		public static Path To(string name)
		{
			return new Path(ambiguousName: name);
		}

		public override string ToString ()
		{
			if (knotName != null) {
				return "==> " + knotName;
			}
			if (stitchName != null) {
				return "=> " + stitchName;
			}
			if (ambiguousName != null) {
				return "-?-> " + ambiguousName;
			}

			return "<Unknown path>";
		}
	}
}

