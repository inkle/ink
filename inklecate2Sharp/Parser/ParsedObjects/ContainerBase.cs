using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
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

				bool isKnotOrStitch = parsedObj is Knot || parsedObj is Stitch;
				bool hasInitialContent = container.content.Count > 0;

				// Add named content (knots and stitches)
				if (isKnotOrStitch && hasInitialContent) {

					var knotOrStitch = parsedObj as ContainerBase;
					if ( container.namedContent.ContainsKey(knotOrStitch.name) ) {
						Error ("Duplicate content named " + knotOrStitch.name);
					}

					container.AddToNamedContentOnly ((Runtime.INamedContent) runtimeObj);
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

