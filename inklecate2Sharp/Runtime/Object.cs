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

