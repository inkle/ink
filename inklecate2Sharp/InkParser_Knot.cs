using System;

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

			return SucceedRule (knotName);
		}
	}
}

