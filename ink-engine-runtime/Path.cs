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

		public Component GetComponent(int index)
		{
			return _components[index];
		}

        public bool isRelative { get; private set; }

		public Component head 
		{ 
			get 
			{ 
				if (_components.Count > 0) {
					return _components.First ();
				} else {
					return null;
				}
			} 
		}

		public Path tail 
		{ 
			get 
			{
				if (_components.Count >= 2) {
					List<Component> tailComps = _components.GetRange (1, _components.Count - 1);
					return new Path(tailComps);
				} 

                else {
                    return Path.self;
				}

			}
		}
            
		public int length { get { return _components.Count; } }

		public Component lastComponent 
		{ 
			get 
			{ 
				var lastComponentIdx = _components.Count-1;
				if( lastComponentIdx >= 0 )
					return _components[lastComponentIdx];
				else
					return null;
			} 
		}

        public bool containsNamedComponent {
            get {
                foreach(var comp in _components) {
                    if( !comp.isIndex ) {
                        return true;
                    }
                }
                return false;
            }
        }

		public Path()
		{
			_components = new List<Component> ();
		}

		public Path(Component head, Path tail) : this()
		{
			_components.Add (head);
			_components.AddRange (tail._components);
		}

		public Path(IEnumerable<Component> components, bool relative = false) : this()
		{
			this._components.AddRange (components);
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
            for (int i = 0; i < pathToAppend._components.Count; ++i) {
                if (pathToAppend._components [i].isParent) {
                    upwardMoves++;
                } else {
                    break;
                }
            }

            for (int i = 0; i < this._components.Count - upwardMoves; ++i) {
                p._components.Add (this._components [i]);
            }

            for(int i=upwardMoves; i<pathToAppend._components.Count; ++i) {
                p._components.Add (pathToAppend._components [i]);
            }

			return p;
		}

        public Path PathByAppendingComponent (Component c)
        {
            Path p = new Path ();
            p._components.AddRange (_components);
            p._components.Add (c);
            return p;
        }

        public string componentsString {
            get {
				if( _componentsString == null ) {
					_componentsString = StringExt.Join (".", _components);
					if (isRelative) _componentsString = "." + _componentsString;
				}
				return _componentsString;
            }
            private set {
                _components.Clear ();

				_componentsString = value;

                // Empty path, empty components
                // (path is to root, like "/" in file system)
                if (string.IsNullOrEmpty(_componentsString))
                    return;

                // When components start with ".", it indicates a relative path, e.g.
                //   .^.^.hello.5
                // is equivalent to file system style path:
                //  ../../hello/5
                if (_componentsString [0] == '.') {
                    this.isRelative = true;
                    _componentsString = _componentsString.Substring (1);
                } else {
                    this.isRelative = false;
                }

                var componentStrings = _componentsString.Split('.');
                foreach (var str in componentStrings) {
                    int index;
                    if (int.TryParse (str , out index)) {
                        _components.Add (new Component (index));
                    } else {
                        _components.Add (new Component (str));
                    }
                }
            }
        }
		string _componentsString;

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

            if (otherPath._components.Count != this._components.Count)
                return false;

            if (otherPath.isRelative != this.isRelative)
                return false;

            return otherPath._components.SequenceEqual (this._components);
        }

        public override int GetHashCode ()
        {
            // TODO: Better way to make a hash code!
            return this.ToString ().GetHashCode ();
        }

		List<Component> _components;
	}
}

