using System;

namespace inklecate2Sharp.Runtime
{
	public class Object
	{
		public Runtime.Object parent { get; set; }

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

