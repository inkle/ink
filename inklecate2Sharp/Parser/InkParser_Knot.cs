using System;
using System.Collections.Generic;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
		protected Knot KnotDefinition()
		{
			BeginRule ();

			Whitespace ();

            if (KnotTitleEquals () == null) {
                return (Knot) FailRule ();
            }
	
			Whitespace ();

			string knotName = Expect(Identifier, "knot name") as string;

            Whitespace ();

            // Optional equals after name
            KnotTitleEquals ();

			Expect(EndOfLine, "end of line after knot name", recoveryRule: SkipToNextLine);

			ParseRule innerKnotStatements = () => StatementsAtLevel (StatementLevel.Knot);

			var content = Expect(innerKnotStatements, "at least one line within the knot", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;
			 
			Knot knot = new Knot (knotName, content);

            return (Knot) SucceedRule (knot);
		}

        protected string KnotTitleEquals()
        {
            // 2+ "=" starts a knot
            var multiEquals = ParseCharactersFromString ("=");
            if (multiEquals == null || multiEquals.Length <= 1) {
                return null;
            } else {
                return multiEquals;
            }
        }

		protected object StitchDefinition()
		{
			BeginRule ();

			Whitespace ();

            // Single "=" to define a stitch
            if (ParseString ("=") == null) {
                return FailRule ();
            }

            // If there's more than one "=", that's actually a knot definition, so this rule should fail
            if (ParseString ("=") != null) {
                return FailRule ();
            }

			Whitespace ();

			string stitchName = Expect (Identifier, "stitch name") as string;

			Expect(EndOfLine, "end of line after stitch name", recoveryRule: SkipToNextLine);

			ParseRule innerStitchStatements = () => StatementsAtLevel (StatementLevel.Stitch);

			var content = Expect(innerStitchStatements, "at least one line within the stitch", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;

			Stitch stitch = new Stitch (stitchName, content);

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

