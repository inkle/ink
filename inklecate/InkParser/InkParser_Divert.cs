using System.Collections.Generic;
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

            var arrowStr = Parse (DivertArrow);

            FlowLevel baseFlowLevel = FlowLevel.Knot;

            switch (arrowStr) {
            case null:
                return null;

            case knotDivertArrow:
                baseFlowLevel = FlowLevel.Knot;
                break;

            case stitchDivertArrow:
                baseFlowLevel = FlowLevel.Stitch;
                break;

            case weavePointDivertArrow:
            case weavePointDivertAltArrow:
                baseFlowLevel = FlowLevel.WeavePoint;
                break;
            }

            List<string> components;
            if (baseFlowLevel == FlowLevel.WeavePoint)
                components = Parse (DotSeparatedDivertPathComponents);
            else
                components = (List<string>) Expect (DotSeparatedDivertPathComponents, "divert target following '"+arrowStr+"'");

            Whitespace ();

            var optionalArguments = Parse(ExpressionFunctionCallArguments);

            // Weave point explicit gather
            if (baseFlowLevel == FlowLevel.WeavePoint && components == null) {
                var gatherDivert = new Divert ((Parsed.Object)null);
                gatherDivert.isToGather = true;
                return gatherDivert;
            }

            // Normal divert
            else {
                var targetPath = new Path (baseFlowLevel, components);
                return new Divert (targetPath, optionalArguments);
            }
        }

        List<string> DotSeparatedDivertPathComponents()
        {
            return Interleave<string> (Spaced (Identifier), Exclude (String (".")));
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

