using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public class Knot : ContainerBase
	{
		public Knot (string name, List<Parsed.Object> topLevelObjects) : base(name, topLevelObjects)
		{
			this.name = name;
		}
	}
}

