using Ink.Runtime;

namespace Ink.Runtime
{
    /// <summary>
    /// Internal structure used to point to a particular / current point in the story.
    /// Where Path is a set of components that make content fully addressable, this is
    /// a reference to the current container, and the index of the current piece of 
    /// content within that container. This scheme makes it as fast and efficient as
    /// possible to increment the pointer (move the story forwards) in a way that's as
    /// native to the internal engine as possible.
    /// </summary>
	internal struct Pointer
	{
		public Container container;
		public int index;

        public Pointer (Container container, int index)
        {
            this.container = container;
            this.index = index;
        }

		public Runtime.Object Resolve ()
		{
            if (index < 0) return container;
            if (container == null) return null;
            if (container.content.Count == 0) return container;
            if (index >= container.content.Count) return null;
            return container.content [index];

		}

        public bool isNull {
            get {
                return container == null;
            }
        }

        public Path path {
            get {
                if( isNull ) return null;

                if (index >= 0)
                    return container.path.PathByAppendingComponent (new Path.Component(index));
                else
                    return container.path;
            }
        }

        public override string ToString ()
        {
            if (container == null)
                return "Ink Pointer (null)";

            return "Ink Pointer -> " + container.path.ToString () + " -- index " + index;
        }

        public static Pointer StartOf (Container container)
        {
            return new Pointer {
                container = container,
                index = 0
            };
        }

        public static Pointer Null = new Pointer { container = null, index = -1 };

	}
}