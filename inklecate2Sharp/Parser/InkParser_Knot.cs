using System;
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

        protected FlowDecl KnotDeclaration()
        {
            BeginRule ();

            Whitespace ();

            if (KnotTitleEquals () == null) {
                return (FlowDecl) FailRule ();
            }

            Whitespace ();

            string knotName = Identifier();
            if (knotName == null)
                return (FlowDecl) FailRule ();

            Whitespace ();

            List<string> parameterNames = BracketedParameterNames ();

            Whitespace ();


            // Optional equals after name
            KnotTitleEquals ();

            var decl = new FlowDecl () { name = knotName, parameters = parameterNames };
            return (FlowDecl) SucceedRule (decl);
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

            var decl = StitchDeclaration ();
            if (decl == null) {
                return FailRule ();
            }

			Expect(EndOfLine, "end of line after stitch name", recoveryRule: SkipToNextLine);

			ParseRule innerStitchStatements = () => StatementsAtLevel (StatementLevel.Stitch);

			var content = Expect(innerStitchStatements, "at least one line within the stitch", recoveryRule: KnotStitchNoContentRecoveryRule) as List<Parsed.Object>;

            Stitch stitch = new Stitch (decl.name, content, decl.parameters);

			return SucceedRule (stitch);
		}

        protected FlowDecl StitchDeclaration()
        {
            BeginRule ();

            Whitespace ();

            // Single "=" to define a stitch
            if (ParseString ("=") == null)
                return (FlowDecl) FailRule ();

            // If there's more than one "=", that's actually a knot definition (or divert), so this rule should fail
            if (ParseString ("=") != null)
                return (FlowDecl) FailRule ();

            Whitespace ();

            string stitchName = Identifier ();
            if (stitchName == null)
                return (FlowDecl)FailRule ();

            Whitespace ();

            List<string> parameterNames = BracketedParameterNames ();

            Whitespace ();

            var decl = new FlowDecl () { name = stitchName, parameters = parameterNames };
            return (FlowDecl) SucceedRule (decl);
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

