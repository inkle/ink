using System;

namespace inklecate2Sharp.Parsed
{
	public class Knot : Parsed.Object
	{
		public string name { get; set; }

		public Knot (string name)
		{
			this.name = name;
		}
	}
}

