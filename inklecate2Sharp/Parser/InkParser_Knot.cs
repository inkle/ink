using System;
using inklecate2Sharp.Parsed;

namespace inklecate2Sharp
{
	public partial class InkParser
	{
		protected object KnotDefinition()
		{
			BeginRule ();

			Whitespace ();

			if (ParseString ("§") == null) {
				return FailRule ();
			}
	
			Whitespace ();

			string knotName = Expect(Identifier, "knot name") as string;

			Knot knot = new Knot (knotName);

			return SucceedRule (knot);
		}
	}
}

