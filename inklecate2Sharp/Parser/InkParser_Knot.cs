using System;
using System.Collections.Generic;
using Inklewriter.Parsed;

namespace Inklewriter
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


			ParseRule innerKnotStatements = () => StatementsAtLevel (StatementLevel.Knot);

			var content = Expect(innerKnotStatements, "at least one line within the knot", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;
			 
			Knot knot = new Knot (knotName, content);

			return SucceedRule (knot);
		}

		protected object StitchDefinition()
		{
			BeginRule ();

			Whitespace ();

            var startChar = (string) OneOf (String ("+"), String ("-"));
            if (startChar == null) {
                return FailRule ();
            }

			Whitespace ();

			string stitchName = Expect (Identifier, "stitch name") as string;

			Expect(EndOfLine, "end of line after stitch name", recoveryRule: SkipToNextLine);

			ParseRule innerStitchStatements = () => StatementsAtLevel (StatementLevel.Stitch);

			var content = Expect(innerStitchStatements, "at least one line within the stitch", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;

			Stitch stitch = new Stitch (stitchName, content);

            if (startChar == "+") {

            }

			return SucceedRule (stitch);
		}

		protected object KnotStitchNoContentRecoveryRule()
		{
			var recoveredStitchContent = new List<Parsed.Object>();
			recoveredStitchContent.Add( new Parsed.Text("<ERROR IN STITCH>" ) );
			return recoveredStitchContent;
		}

	}
}

