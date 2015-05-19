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

			foreach (var child in this.content) {
				child.parent = this;
			}
		}

		public override Runtime.Object GenerateRuntimeObject()
		{
			var container = new Runtime.Container ();
			container.name = name;

			foreach (var parsedObj in content) {
				Runtime.Object runtimeObj = parsedObj.runtimeObject;

				bool hasInitialContent = container.content.Count > 0;

				if (parsedObj is Knot && hasInitialContent) {
					container.AddToNamedContentOnly ((INamedContent) runtimeObj);
				} else {
					container.AddContent (runtimeObj);
				}
					
			}

			return container;
		}

		public override void ResolvePaths ()
		{
			foreach (Parsed.Object obj in content) {
				obj.ResolvePaths (); 
			}
		}
	}
}

