using System;

namespace Inklewriter.Runtime
{
	public class Object
	{
		public Runtime.Object parent { get; set; }

        public Runtime.DebugMetadata debugMetadata { 
            get {
                if (_debugMetadata == null) {
                    if (parent != null) {
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
        // to have debug metadata on the object itself?!
        DebugMetadata _debugMetadata;

        public int? DebugLineNumberOfPath(Path path)
        {
            if (path == null)
                return null;
            
            // Try to get a line number from debug metadata
            var root = this.rootContentContainer;
            if (root != null) {
                var targetContent = root.ContentAtPath (path);
                if (targetContent != null) {
                    var dm = targetContent.debugMetadata;
                    if (dm != null) {
                        return dm.startLineNumber;
                    }
                }
            }

            return null;
        }

		public Path path 
		{ 
			get 
			{
				if (parent == null) {
					return new Path ();
				} else {
					return parent.PathToChild (this);
				}
			}
		}

        public Container rootContentContainer
        {
            get 
            {
                Runtime.Object ancestor = this;
                while (ancestor.parent != null) {
                    ancestor = ancestor.parent;
                }
                return ancestor as Container;
            }
        }

		public virtual Path PathToChild(Runtime.Object child)
		{
			// Default: Not a child of this object
			return null;
		}

		public Object ()
		{
		}
	}
}

