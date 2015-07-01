using Inklewriter.Parsed;

namespace Inklewriter
{
    public partial class InkParser
    {
        const string knotDivertArrow = "==>";
        const string stitchDivertArrow = "=>";
        const string weavePointDivertArrow = "->";
        const string weavePointDivertAltArrow = "-->";
        const string weavePointExplicitGather = "<explicit-gather>";

        protected Divert Divert()
        {
            Whitespace ();

            var knotName = ParseDivertTargetWithArrow (knotDivertArrow);
            var stitchName = ParseDivertTargetWithArrow (stitchDivertArrow);
            var weavePointName = ParseDivertTargetWithArrow (weavePointDivertArrow);
            if (knotName == null && stitchName == null && weavePointName == null) {
                return null;
            }

            Whitespace ();

            var optionalArguments = Parse(ExpressionFunctionCallArguments);

            // Weave point explicit gather
            if (weavePointName == weavePointExplicitGather) {
                var gatherDivert = new Divert (null);
                gatherDivert.isToGather = true;
                return gatherDivert;
            }

            // Normal divert
            else {
                Path targetPath = Path.To(knotName, stitchName, weavePointName);
                return new Divert (targetPath, optionalArguments);
            }
        }

        string ParseDivertTargetWithArrow(string arrowStr)
        {
            Whitespace ();

            string parsedArrowResult = ParseString (arrowStr);

            // Allow both -> and --> for weaves
            if (parsedArrowResult == null && arrowStr == weavePointDivertArrow) {
                parsedArrowResult = ParseString (weavePointDivertAltArrow);
            }

            if (parsedArrowResult == null) {
                return null;
            }

            Whitespace ();

            string targetName = null;

            // Weave arrows without a target mean "explicit gather"
            if (arrowStr == weavePointDivertArrow) {
                targetName = Identifier ();
                if (targetName == null) {
                    targetName = weavePointExplicitGather;
                }
            } else {
                targetName = (string) Expect(Identifier, "name of target to divert to");
            }

            return targetName;
        }

        protected string DivertArrow()
        {
            return OneOf(String(knotDivertArrow), String(stitchDivertArrow), String(weavePointDivertArrow), String(weavePointDivertAltArrow)) as string;
        }
    }
}

