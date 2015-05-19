using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public class ContainerBase : Parsed.Object
	{
		public string name { get; protected set; }
		public List<object> content { get; protected set; }

		public ContainerBase (string name = null, List<object> topLevelObjects = null)
		{
			this.name = name;

			if (topLevelObjects == null) {
				topLevelObjects = new List<object> ();
			}
			this.content = topLevelObjects;
		}
	}
}

