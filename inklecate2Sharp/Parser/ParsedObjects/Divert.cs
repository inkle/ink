using System;

namespace inklecate2Sharp.Parsed
{
	public class Divert : Parsed.Object
	{
		public Parsed.Path target { get; protected set; }

		public Divert (Parsed.Path target)
		{
			this.target = target;
		}

		public override Runtime.Object GenerateRuntimeObject ()
		{
			return new Runtime.Divert ();
		}
	}
}

