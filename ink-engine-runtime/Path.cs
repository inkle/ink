using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Ink.Runtime;

namespace Ink.Runtime
{
    internal class Path : IEquatable<Path>
	{
        static string parentId = "^";

        // Immutable Component
        internal class Component : IEquatable<Component>
		{
			public int index { get; private set; }
			public string name { get; private set; }
			public bool isIndex { get { return index >= 0; } }
            public bool isParent {
                get {
                    return name == Path.parentId;
                }
            }

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

            public static Component ToParent()
            {
                return new Component (parentId);
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
                return Equals (obj as Component);
            }

            public bool Equals(Component otherComp)
            {
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

        public bool isRelative { get; private set; }

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
				} 

                else {
                    return Path.self;
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

		public Path(IEnumerable<Component> components, bool relative = false) : this()
		{
			this.components.AddRange (components);
            this.isRelative = relative;
		}

        public Path(string componentsString) : this()
        {
            this.componentsString = componentsString;
        }

        public static Path self {
            get {
                var path = new Path ();
                path.isRelative = true;
                return path;
            }
        }

		public Path PathByAppendingPath(Path pathToAppend)
		{
            Path p = new Path ();

            int upwardMoves = 0;
            for (int i = 0; i < pathToAppend.components.Count; ++i) {
                if (pathToAppend.components [i].isParent) {
                    upwardMoves++;
                } else {
                    break;
                }
            }

            for (int i = 0; i < this.components.Count - upwardMoves; ++i) {
                p.components.Add (this.components [i]);
            }

            for(int i=upwardMoves; i<pathToAppend.components.Count; ++i) {
                p.components.Add (pathToAppend.components [i]);
            }

			return p;
		}

        public string componentsString {
            get {
                var compsStr = StringExt.Join (".", components);
                if (isRelative)
                    return "." + compsStr;
                else
                    return compsStr;
            }
            private set {
                components.Clear ();

                var componentsStr = value;

                // Empty path, empty components
                // (path is to root, like "/" in file system)
                if (string.IsNullOrEmpty(componentsStr))
                    return;

                // When components start with ".", it indicates a relative path, e.g.
                //   .^.^.hello.5
                // is equivalent to file system style path:
                //  ../../hello/5
                if (componentsStr [0] == '.') {
                    this.isRelative = true;
                    componentsStr = componentsStr.Substring (1);
                } else {
                    this.isRelative = false;
                }

                var componentStrings = componentsStr.Split('.');
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

		public override string ToString()
		{
            return componentsString;
		}

        public override bool Equals (object obj)
        {
            return Equals (obj as Path);
        }

        public bool Equals (Path otherPath)
        {
            if (otherPath == null)
                return false;

            if (otherPath.components.Count != this.components.Count)
                return false;

            if (otherPath.isRelative != this.isRelative)
                return false;

            return otherPath.components.SequenceEqual (this.components);
        }

        public override int GetHashCode ()
        {
            // TODO: Better way to make a hash code!
            return this.ToString ().GetHashCode ();
        }
	}
}

