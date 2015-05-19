using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace inklecate2Sharp.Runtime
{
	public class Container : Runtime.Object, INamedContent
	{
		public string name { get; set; }
		public List<Runtime.Object> content { get; protected set; }
		public Dictionary<string, INamedContent> namedContent { get; protected set; }

		public bool hasValidName 
		{
			get { return name != null && name.Length > 0; }
		}

		public Path pathToFirstLeafContent
		{
			get 
			{
				if (content.Count > 0) {
					Path path = Path.ToFirstElement();
					var subContainer = content.First () as Container;
					if (subContainer != null) {
						Path tailPath = subContainer.pathToFirstLeafContent;
						path = path.PathByAppendingPath (tailPath);
					}
					return path;
				}

				return null;
			}
		}

		public Container ()
		{
			content = new List<Runtime.Object> ();
			namedContent = new Dictionary<string, INamedContent> ();
		}

		public void AddContent(Runtime.Object contentObj)
		{
			content.Add (contentObj);

			TryAddNamedContent (contentObj);
		}

		public void TryAddNamedContent(Runtime.Object contentObj)
		{
			var namedContentObj = contentObj as INamedContent;
			if (namedContentObj != null && namedContentObj.hasValidName) {
				AddToNamedContent (namedContentObj);
			}
		}

		public void AddToNamedContent(INamedContent namedContentObj)
		{
			namedContent [namedContentObj.name] = namedContentObj;
		}

		protected Runtime.Object ContentWithPathComponent(Path.Component component)
		{
			if( component.isIndex ) {

				if( component.index >= 0 && component.index < content.Count ) {
					return content[component.index];
				}

				// When path is out of range, quietly return nil
				// (useful as we step/increment forwards through content)
				else {
					return null;
				}

			} else {
				return (Runtime.Object) namedContent[component.name];
			}
		}

		public Runtime.Object ContentAtPath(Path path)
		{
			Path.Component component = path.head;
			if( component != null ) {
				var contentObj = ContentWithPathComponent(component);

				// Continue deeper into sub-container?
				if( path.length > 1 ) {
					Debug.Assert (contentObj is Container, "Path continues, but content isn't a container");
					Container subContainer = (Container) contentObj;
					return subContainer.ContentAtPath (path.tail);
				} else {
					return contentObj;
				}

			}

			return null;
		}

		public Path IncrementPath(Path path)
		{
			if (path.length > 0) {

				// Try to increment tail
				if (path.length > 1) {
					var currChild = ContentWithPathComponent (path.head);
					Debug.Assert (currChild is Container, "Expected a container for deep path?");

					Container childContainer = (Container)currChild;
					Path incrementedTail = childContainer.IncrementPath (path.tail);

					// Successfully incremented tail
					if (incrementedTail != null) {
						return new Path (path.head, incrementedTail);
					}
				}

				// No tail, or failed to increment tail
				// Try to increment self
				// Can only increment if we have indexed content, and the original component was indexed
				var comp = path.head;
				if (comp.isIndex) {

					// Successfully incremented path in self?
					int nextIndex = comp.index + 1;
					if (content.Count > nextIndex) {
						return Path.ToElementWithIndex (nextIndex);
					}
				}
			}

			// Failed to increment
			return null;
		}
	}
}

