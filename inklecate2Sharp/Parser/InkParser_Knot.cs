using System;
using System.Collections.Generic;
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

			Expect(EndOfLine, "end of line after knot name", recoveryRule: SkipToNextLine);

			var content = Expect(InnerKnotStatements, "at least one line within the knot", recoveryRule: () => {
				var recoveredKnotContent = new List<Parsed.Object>();
				recoveredKnotContent.Add( new Parsed.Text("<ERROR IN KNOT>" ) );
				return recoveredKnotContent;
			}) as List<Parsed.Object>;
			 
			Knot knot = new Knot (knotName, content);

			return SucceedRule (knot);
		}

		protected List<Parsed.Object> InnerKnotStatements()
		{
			return StatementsAtLevel (StatementLevel.Knot);
		}
	}
}

