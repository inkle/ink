using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public class Knot : ContainerBase
	{
		public Knot (string name, List<object> topLevelObjects) : base(name, topLevelObjects)
		{
			this.name = name;
		}
	}
}

