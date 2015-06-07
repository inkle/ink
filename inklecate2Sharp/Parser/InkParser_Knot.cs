using System;
using System.Collections.Generic;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
        protected class KnotDecl
        {
            public string name;
            public List<string> parameters;
        }

		protected Knot KnotDefinition()
		{
			BeginRule ();

            var knotDecl = KnotDeclaration ();
            if (knotDecl == null)
                return (Knot) FailRule ();

			Expect(EndOfLine, "end of line after knot name definition", recoveryRule: SkipToNextLine);

			ParseRule innerKnotStatements = () => StatementsAtLevel (StatementLevel.Knot);

			var content = Expect(innerKnotStatements, "at least one line within the knot", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;
			 
            Knot knot = new Knot (knotDecl.name, content, knotDecl.parameters);

            return (Knot) SucceedRule (knot);
		}

        protected KnotDecl KnotDeclaration()
        {
            BeginRule ();

            Whitespace ();

            if (KnotTitleEquals () == null) {
                return (KnotDecl) FailRule ();
            }

            Whitespace ();

            string knotName = Identifier();
            if (knotName == null)
                return (KnotDecl) FailRule ();

            Whitespace ();

            List<string> parameterNames = BracketedParameterNames ();

            Whitespace ();


            // Optional equals after name
            KnotTitleEquals ();

            var decl = new KnotDecl () { name = knotName, parameters = parameterNames };
            return (KnotDecl) SucceedRule (decl);
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

            Whitespace ();

            List<string> parameterNames = BracketedParameterNames ();

            Whitespace ();

			Expect(EndOfLine, "end of line after stitch name", recoveryRule: SkipToNextLine);

			ParseRule innerStitchStatements = () => StatementsAtLevel (StatementLevel.Stitch);

			var content = Expect(innerStitchStatements, "at least one line within the stitch", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;

            Stitch stitch = new Stitch (stitchName, content, parameterNames);

			return SucceedRule (stitch);
		}


		protected object KnotStitchNoContentRecoveryRule()
		{
			var recoveredStitchContent = new List<Parsed.Object>();
			recoveredStitchContent.Add( new Parsed.Text("<ERROR IN STITCH>" ) );
			return recoveredStitchContent;
		}

        protected List<string> BracketedParameterNames()
        {
            BeginRule ();

            if (ParseString ("(") == null)
                return (List<string>) FailRule ();

            var parameterNames = Interleave<string>(Spaced(Identifier), Exclude (String(",")));

            Expect (String (")"), "closing ')' for parameter list");

            // If no parameters, create an empty list so that this method is type safe and 
            // doesn't attempt to return the ParseSuccess object
            if (parameterNames == null) {
                parameterNames = new List<string> ();
            }

            return (List<string>) SucceedRule (parameterNames);
        }

	}
}

