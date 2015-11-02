using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Inklewriter.Runtime
{
    [JsonObject(MemberSerialization.OptIn)]
	internal class Container : Runtime.Object, INamedContent
	{
        [JsonProperty]
		public string name { get; set; }

        [JsonProperty("c")]
        [UniqueJsonIdentifier]
        public List<Runtime.Object> content { 
            get {
                return _content;
            }
            set {
                AddContent (value);
            }
        }
        List<Runtime.Object> _content;

		public Dictionary<string, INamedContent> namedContent { get; }

        [JsonProperty("namedOnly")]
        public Dictionary<string, Runtime.Object> namedOnlyContent { 
            get {
                var namedOnlyContent = new Dictionary<string, Runtime.Object>();
                foreach (var kvPair in namedContent) {
                    namedOnlyContent [kvPair.Key] = (Runtime.Object)kvPair.Value;
                }

                foreach (var c in content) {
                    var named = c as INamedContent;
                    if (named != null && named.hasValidName) {
                        namedOnlyContent.Remove (named.name);
                    }
                }

                if (namedOnlyContent.Count == 0)
                    namedOnlyContent = null;

                return namedOnlyContent;
            } 
            set {
                var existingNamedOnly = namedOnlyContent;
                if (existingNamedOnly != null) {
                    foreach (var kvPair in existingNamedOnly) {
                        namedContent.Remove (kvPair.Key);
                    }
                }

                if (value == null)
                    return;
                
                foreach (var kvPair in value) {
                    var named = kvPair.Value as INamedContent;
                    if( named != null )
                        AddToNamedContentOnly (named);
                }
            }
        }

        [JsonProperty(propertyName:"count")]
        [DefaultValue(false)]
        public bool visitsShouldBeCounted { get; set; }

        [JsonProperty(propertyName:"turns")]
        [DefaultValue(false)]
        public bool turnIndexShouldBeCounted { get; set; }

		public bool hasValidName 
		{
			get { return name != null && name.Length > 0; }
		}

		public Path pathToFirstLeafContent
		{
			get {
				if (content.Count > 0) {
					Path path = Path.ToFirstElement();
					var subContainer = content.First () as Container;
                    if (subContainer && subContainer.content.Count > 0) {
						Path tailPath = subContainer.pathToFirstLeafContent;
						path = path.PathByAppendingPath (tailPath);
					}
					return path;
				}

				return null;
			}
		}

        public Path pathToEnd
        {
            get {
                return Path.ToElementWithIndex (content.Count);
            }
        }

		public Container ()
		{
            _content = new List<Runtime.Object> ();
			namedContent = new Dictionary<string, INamedContent> ();
		}

		public void AddContent(Runtime.Object contentObj)
		{
			content.Add (contentObj);

            if (contentObj.parent) {
                throw new System.Exception ("content is already in " + contentObj.parent);
            }

			contentObj.parent = this;

			TryAddNamedContent (contentObj);
		}

        public void AddContent(IList<Runtime.Object> contentList)
        {
            foreach (var c in contentList) {
                AddContent (c);
            }
        }

        public void InsertContent(Runtime.Object contentObj, int index)
        {
            content.Insert (index, contentObj);

            if (contentObj.parent) {
                throw new System.Exception ("content is already in " + contentObj.parent);
            }

            contentObj.parent = this;

            TryAddNamedContent (contentObj);
        }
            
		public void TryAddNamedContent(Runtime.Object contentObj)
		{
			var namedContentObj = contentObj as INamedContent;
			if (namedContentObj != null && namedContentObj.hasValidName) {
				AddToNamedContentOnly (namedContentObj);
			}
		}

		public void AddToNamedContentOnly(INamedContent namedContentObj)
		{
			Debug.Assert (namedContentObj is Runtime.Object, "Can only add Runtime.Objects to a Runtime.Container");
			var runtimeObj = (Runtime.Object)namedContentObj;
			runtimeObj.parent = this;

			namedContent [namedContentObj.name] = namedContentObj;
		}

        public void AddContentsOfContainer(Container otherContainer)
        {
            content.AddRange (otherContainer.content);
            foreach (var obj in otherContainer.content) {
                obj.parent = this;
                TryAddNamedContent (obj);
            }
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
                    Path tail = path.tail;
                    Path incrementedTail = childContainer.IncrementPath (tail);

					// Successfully incremented tail
					if (incrementedTail != null) {
						return new Path (path.head, incrementedTail);
					}

                    // A failed increment to a tail that contains a named element anywhere should cause the full path to fail.
                    // Why? Because if you increment off the end of *any* named container, then there's nowhere
                    // you can sensibly go.
                    else if (tail.containsNamedComponent) {
                        
                        return null;
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

        public void BuildStringOfHierarchy(StringBuilder sb, int indentation, Runtime.Object pointedObj)
        {
            Action appendIndentation = () => { 
                const int spacesPerIndent = 4;
                for(int i=0; i<spacesPerIndent*indentation;++i) { 
                    sb.Append(" "); 
                } 
            };

            appendIndentation ();
            sb.Append("[");

            if (this.hasValidName) {
                sb.AppendFormat (" ({0})", this.name);
            }

            if (this == pointedObj) {
                sb.Append ("  <---");
            }

            sb.AppendLine ();

            indentation++;
            
            for (int i=0; i<content.Count; ++i) {

                var obj = content [i];

                if (obj is Container) {

                    var container = (Container)obj;

                    container.BuildStringOfHierarchy (sb, indentation, pointedObj);

                } else {
                    appendIndentation ();
                    sb.Append (obj.ToString ());
                }

                if (i != content.Count - 1) {
                    sb.Append (",");
                }

                if ( !(obj is Container) && obj == pointedObj ) {
                    sb.Append ("  <---");
                }
                    
                sb.AppendLine ();
            }
                

            var onlyNamed = new Dictionary<string, INamedContent> ();

            foreach (var objKV in namedContent) {
                if (content.Contains ((Runtime.Object)objKV.Value)) {
                    continue;
                } else {
                    onlyNamed.Add (objKV.Key, objKV.Value);
                }
            }

            if (onlyNamed.Count > 0) {
                appendIndentation ();
                sb.AppendLine ("-- named: --");

                foreach (var objKV in onlyNamed) {

                    Debug.Assert (objKV.Value is Container, "Can only print out named Containers");
                    var container = (Container)objKV.Value;
                    container.BuildStringOfHierarchy (sb, indentation, pointedObj);

                    sb.AppendLine ();

                }
            }


            indentation--;

            appendIndentation ();
            sb.Append ("]");
        }

        public virtual string BuildStringOfHierarchy()
        {
            var sb = new StringBuilder ();

            BuildStringOfHierarchy (sb, 0, null);

            return sb.ToString ();
        }

	}
}

