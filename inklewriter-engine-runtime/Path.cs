using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Inklewriter.Runtime;
using Newtonsoft.Json;

namespace Inklewriter.Runtime
{
    [JsonObject(MemberSerialization.OptIn)]
	internal class Path
	{
		internal class Component
		{
			public int index { get; set; }
			public string name { get; set; }
			public bool isIndex { get { return index >= 0; } }

			public Component(int index)
			{
				Debug.Assert(index >= 0);
				this.index = index;
				this.name = null;
			}

			public Component(string name)
			{
				Debug.Assert(name != null && name.Length > 0);
				this.name = name;
				this.index = -1;
			}

			public override string ToString ()
			{
				if (isIndex) {
					return index.ToString ();
				} else {
					return name;
				}
			}

            public override bool Equals (object obj)
            {
                var otherComp = obj as Component;
                if (otherComp != null && otherComp.isIndex == this.isIndex) {
                    if (isIndex) {
                        return index == otherComp.index;   
                    } else {
                        return name == otherComp.name;
                    }
                }

                return false;
            }

            public override int GetHashCode ()
            {
                if (isIndex)
                    return this.index;
                else
                    return this.name.GetHashCode ();
            }
		}

		public List<Component> components { get; private set; }

		public Component head 
		{ 
			get 
			{ 
				if (components.Count > 0) {
					return components.First ();
				} else {
					return null;
				}
			} 
		}

		public Path tail 
		{ 
			get 
			{
				if (components.Count >= 2) {
					List<Component> tailComps = components.GetRange (1, components.Count - 1);
					return new Path(tailComps);
				} else {
					return null;
				}

			}
		}

		public int length { get { return components.Count; } }

		public Component lastComponent 
		{ 
			get 
			{ 
				if (components.Count > 0) {
					return components.Last ();
				} else {
					return null;
				}
			} 
		}

        public bool containsNamedComponent {
            get {
                foreach(var comp in components) {
                    if( !comp.isIndex ) {
                        return true;
                    }
                }
                return false;
            }
        }

		public Path()
		{
			components = new List<Component> ();
		}

		public Path(Component head, Path tail) : this()
		{
			components.Add (head);
			components.AddRange (tail.components);
		}

		public Path(IEnumerable<Component> components) : this()
		{
			this.components.AddRange (components);
		}

        public Path(string componentsString) : this()
        {
            this.componentsString = componentsString;
        }

		public Path PathByAppendingPath(Path pathToAppend)
		{
			Path p = new Path (this.components);
			p.components.AddRange (pathToAppend.components);
			return p;
		}

		public Path PathByAppendingElementWithName(string name)
		{
			Path p = new Path (this.components);
			p.components.Add (new Component (name));
			return p;
		}

		public Path PathByAppendingElementWithIndex(int index)
		{
			Path p = new Path (this.components);
			p.components.Add (new Component (index));
			return p;
		}

        [JsonProperty("p")]
        public string componentsString {
            get {
                return StringExt.Join (".", components);
            }
            set {
                components.Clear ();

                var componentStrings = value.Split('.');
                foreach (var str in componentStrings) {
                    int index;
                    if (int.TryParse (str , out index)) {
                        components.Add (new Component (index));
                    } else {
                        components.Add (new Component (str));
                    }
                }
            }
        }

		static public Path ToFirstElement()
		{
			return ToElementWithIndex (0);
		}

		static public Path ToElementWithIndex(int index)
		{
			var comps = new List<Component> ();
			comps.Add (new Component (index));
			return new Path (comps);
		}

		public override string ToString()
		{
            return componentsString;
		}

        public override bool Equals (object obj)
        {
            var otherPath = obj as Path;
            if( otherPath != null )
                return otherPath.components.SequenceEqual(this.components);

            return false;
        }

        public override int GetHashCode ()
        {
            // TODO: Better way to make a hash code!
            return this.ToString ().GetHashCode ();
        }
	}
}

