using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	public class Knot : ContainerBase
	{
		public Knot (string name, List<Parsed.Object> topLevelObjects) : base(name, topLevelObjects)
		{
			this.name = name;
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
					// TODO: Cast to stitch and check whether the name matches
				}
			}

			return base.ResolvePath (path);
		}
	}
}

