using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	public class Knot : FlowBase
	{
		public Knot (string name, List<Parsed.Object> topLevelObjects) : base(name, topLevelObjects)
		{
		}

		public override Parsed.Object ResolvePath(Path path)
		{
			string stitchName = path.stitchName;
			if (stitchName == null) {
				stitchName = path.ambiguousName;
			}

			bool searchInSelf = path.knotName == null || path.knotName == this.name;
			if (stitchName != null && searchInSelf) {
				foreach (Parsed.Object contentObj in content) {
					var stitch = contentObj as Stitch;
					if (stitch != null && stitch.name == stitchName) {
						return stitch;
					}
				}
			}

			return base.ResolvePath (path);
		}
	}
}

