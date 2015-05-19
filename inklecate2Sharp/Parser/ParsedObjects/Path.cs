using System;

namespace inklecate2Sharp.Parsed
{
	public class Path
	{
		public string knotName { get; protected set; }
		public string stitchName { get; protected set; }
		public string ambiguousName { get; protected set; }

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
	}
}

