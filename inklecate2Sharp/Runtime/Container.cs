using System;
using System.Diagnostics;
using System.Collections.Generic;

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
	}
}

