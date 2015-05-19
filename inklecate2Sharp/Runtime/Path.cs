using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace inklecate2Sharp.Runtime
{
	public class Path
	{
		public class Component
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
		}

		public List<Component> components { get; }

		public Component head 
		{ 
			get 
			{ 
				return components.FirstOrDefault (null); 
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

		public Component lastComponent { get { return components.LastOrDefault (null); } }

		public Path()
		{
			components = new List<Component> ();
		}

		public Path(Component head, List<Component> tail) : this()
		{
			components.Add (head);
			components.AddRange (tail);
		}

		public Path(List<Component> components) : this()
		{
			this.components.AddRange (components);
		}
	}
}

