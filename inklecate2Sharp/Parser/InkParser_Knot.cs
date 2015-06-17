using System.Collections.Generic;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
        protected class FlowDecl
        {
            public string name;
            public List<string> parameters;
        }

		protected Knot KnotDefinition()
		{
            var knotDecl = Parse(KnotDeclaration);
            if (knotDecl == null)
                return null;

			Expect(EndOfLine, "end of line after knot name definition", recoveryRule: SkipToNextLine);

			ParseRule innerKnotStatements = () => StatementsAtLevel (StatementLevel.Knot);

            var content = Expect (innerKnotStatements, "at least one line within the knot", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;
			 
            return new Knot (knotDecl.name, content, knotDecl.parameters);
		}

        protected FlowDecl KnotDeclaration()
        {
            Whitespace ();

            if (KnotTitleEquals () == null)
                return null;

            Whitespace ();

            string knotName = Parse(Identifier);
            if (knotName == null)
                return null;

            Whitespace ();

            List<string> parameterNames = Parse (BracketedParameterNames);

            Whitespace ();

            // Optional equals after name
            Parse(KnotTitleEquals);

            return new FlowDecl () { name = knotName, parameters = parameterNames };
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
            var decl = Parse(StitchDeclaration);
            if (decl == null)
                return null;

			Expect(EndOfLine, "end of line after stitch name", recoveryRule: SkipToNextLine);

			ParseRule innerStitchStatements = () => StatementsAtLevel (StatementLevel.Stitch);

            var content = Expect(innerStitchStatements, "at least one line within the stitch", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;

            return new Stitch (decl.name, content, decl.parameters);
		}

        protected FlowDecl StitchDeclaration()
        {
            Whitespace ();

            // Single "=" to define a stitch
            if (ParseString ("=") == null)
                return null;

            // If there's more than one "=", that's actually a knot definition (or divert), so this rule should fail
            if (ParseString ("=") != null)
                return null;

            Whitespace ();

            string stitchName = Parse(Identifier);
            if (stitchName == null)
                return null;

            Whitespace ();

            List<string> parameterNames = Parse(BracketedParameterNames);

            Whitespace ();

            return new FlowDecl () { name = stitchName, parameters = parameterNames };
        }


		protected object KnotStitchNoContentRecoveryRule()
		{
			var recoveredStitchContent = new List<Parsed.Object>();
			recoveredStitchContent.Add( new Parsed.Text("<ERROR IN STITCH>" ) );
			return recoveredStitchContent;
		}

        protected List<string> BracketedParameterNames()
        {
            if (ParseString ("(") == null)
                return null;

            var parameterNames = Interleave<string>(Spaced(Identifier), Exclude (String(",")));

            Expect (String (")"), "closing ')' for parameter list");

            // If no parameters, create an empty list so that this method is type safe and 
            // doesn't attempt to return the ParseSuccess object
            if (parameterNames == null) {
                parameterNames = new List<string> ();
            }

            return parameterNames;
        }

	}
}

