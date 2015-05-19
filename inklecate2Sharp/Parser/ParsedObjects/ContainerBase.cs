using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public class ContainerBase : Parsed.Object
	{
		public string name { get; protected set; }
		public List<Parsed.Object> content { get; protected set; }

		public ContainerBase (string name = null, List<Parsed.Object> topLevelObjects = null)
		{
			this.name = name;

			if (topLevelObjects == null) {
				topLevelObjects = new List<Parsed.Object> ();
			}
			this.content = topLevelObjects;
		}

		public override Runtime.Object GenerateRuntimeObject()
		{
			var container = new Runtime.Container ();
			container.name = name;

			foreach (var parsedObj in content) {
				Runtime.Object runtimeObj = parsedObj.runtimeObject;

				// TODO: Treat knots and stitches specially

				container.AddContent (runtimeObj);
			}

			return container;
		}
	}
}

