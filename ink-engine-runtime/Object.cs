using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ink.Runtime
{
    /// <summary>
    /// Base class for all ink runtime content.
    /// </summary>
    public /* TODO: abstract */ class Object
	{
        /// <summary>
        /// Runtime.Objects can be included in the main Story as a hierarchy.
        /// Usually parents are Container objects. (TODO: Always?)
        /// </summary>
        /// <value>The parent.</value>
		public Runtime.Object parent { get; set; }

        internal Runtime.DebugMetadata debugMetadata { 
            get {
                if (_debugMetadata == null) {
                    if (parent) {
                        return parent.debugMetadata;
                    }
                }

                return _debugMetadata;
            }

            set {
                _debugMetadata = value;
            }
        }

        // TODO: Come up with some clever solution for not having
        // to have debug metadata on the object itself, perhaps
        // for serialisation purposes at least.
        DebugMetadata _debugMetadata;

        internal int? DebugLineNumberOfPath(Path path)
        {
            if (path == null)
                return null;
            
            // Try to get a line number from debug metadata
            var root = this.rootContentContainer;
            if (root) {
                Runtime.Object targetContent = null;

                // Sometimes paths can be "invalid" if they're externally defined
                // in the game. TODO: Change ContentAtPath to return null, and
                // only throw an exception in places that actually care!
                try {
                    targetContent = root.ContentAtPath (path);
                } catch { }

                if (targetContent) {
                    var dm = targetContent.debugMetadata;
                    if (dm != null) {
                        return dm.startLineNumber;
                    }
                }
            }

            return null;
        }

		internal Path path 
		{ 
			get 
			{
                if (_path == null) {

                    if (parent == null) {
                        _path = new Path ();
                    } else {
                        // Maintain a Stack so that the order of the components
                        // is reversed when they're added to the Path.
                        // We're iterating up the hierarchy from the leaves/children to the root.
                        var comps = new Stack<Path.Component> ();

                        var child = this;
                        Container container = child.parent as Container;

                        while (container) {

                            var namedChild = child as INamedContent;
                            if (namedChild != null && namedChild.hasValidName) {
                                comps.Push (new Path.Component (namedChild.name));
                            } else {
                                comps.Push (new Path.Component (container.content.IndexOf(child)));
                            }

                            child = container;
                            container = container.parent as Container;
                        }

                        _path = new Path (comps);
                    }

                }
				
                return _path;
			}
		}
        Path _path;

        internal Runtime.Object ResolvePath(Path path)
        {
            if (path.isRelative) {

                Container nearestContainer = this as Container;
                if (!nearestContainer) {
                    Debug.Assert (this.parent != null, "Can't resolve relative path because we don't have a parent");
                    nearestContainer = this.parent as Container;
                    Debug.Assert (nearestContainer != null, "Expected parent to be a container");
                    Debug.Assert (path.components [0].isParent);
                    path = path.tail;
                }

                return nearestContainer.ContentAtPath (path);
            } else {
                return this.rootContentContainer.ContentAtPath (path);
            }
        }

        internal Path ConvertPathToRelative(Path globalPath)
        {
            // 1. Find last shared ancestor
            // 2. Drill up using ".." style (actually represented as "^")
            // 3. Re-build downward chain from common ancestor

            var ownPath = this.path;

            int minPathLength = Math.Min (globalPath.components.Count, ownPath.components.Count);
            int lastSharedPathCompIndex = -1;

            for (int i = 0; i < minPathLength; ++i) {
                var ownComp = ownPath.components [i];
                var otherComp = globalPath.components [i];

                if (ownComp.Equals (otherComp)) {
                    lastSharedPathCompIndex = i;
                } else {
                    break;
                }
            }

            // No shared path components, so just use global path
            if (lastSharedPathCompIndex == -1)
                return globalPath;

            int numUpwardsMoves = (ownPath.components.Count-1) - lastSharedPathCompIndex;

            var newPathComps = new List<Path.Component> ();

            for(int up=0; up<numUpwardsMoves; ++up)
                newPathComps.Add (Path.Component.ToParent ());

            for (int down = lastSharedPathCompIndex + 1; down < globalPath.components.Count; ++down)
                newPathComps.Add (globalPath.components [down]);

            var relativePath = new Path (newPathComps, relative:true);
            return relativePath;
        }

        // Find most compact representation for a path, whether relative or global
        internal string CompactPathString(Path otherPath)
        {
            string globalPathStr = null;
            string relativePathStr = null;
            if (otherPath.isRelative) {
                relativePathStr = otherPath.componentsString;
                globalPathStr = this.path.PathByAppendingPath(otherPath).componentsString;
            } else {
                var relativePath = ConvertPathToRelative (otherPath);
                relativePathStr = relativePath.componentsString;
                globalPathStr = otherPath.componentsString;
            }

            if (relativePathStr.Length < globalPathStr.Length) 
                return relativePathStr;
            else
                return globalPathStr;
        }

        internal Container rootContentContainer
        {
            get 
            {
                Runtime.Object ancestor = this;
                while (ancestor.parent) {
                    ancestor = ancestor.parent;
                }
                return ancestor as Container;
            }
        }

		internal Object ()
		{
		}

        internal virtual Object Copy()
        {
            throw new System.NotImplementedException (GetType ().Name + " doesn't support copying");
        }

        internal void SetChild<T>(ref T obj, T value) where T : Runtime.Object
        {
            if (obj)
                obj.parent = null;

            obj = value;

            if( obj )
                obj.parent = this;
        }
            
        /// Allow implicit conversion to bool so you don't have to do:
        /// if( myObj != null ) ...
        public static implicit operator bool (Object obj)
        {
            var isNull = object.ReferenceEquals (obj, null);
            return !isNull;
        }

        /// Required for implicit bool comparison
        public static bool operator ==(Object a, Object b)
        {
            return object.ReferenceEquals (a, b);
        }

        /// Required for implicit bool comparison
        public static bool operator !=(Object a, Object b)
        {
            return !(a == b);
        }

        /// Required for implicit bool comparison
        public override bool Equals (object obj)
        {
            return object.ReferenceEquals (obj, this);
        }

        /// Required for implicit bool comparison
        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }
	}
}

