using System.Collections.Generic;
using Inklewriter.Parsed;


namespace Inklewriter
{
    internal partial class InkParser
    {
        const string knotDivertArrow = "==>";
        const string stitchDivertArrow = "=>";
        const string weavePointDivertArrow = "->";
        const string weavePointDivertAltArrow = "-->";

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

        protected string DivertArrow()
        {
            return OneOf(String(knotDivertArrow), String(stitchDivertArrow), String(weavePointDivertArrow), String(weavePointDivertAltArrow)) as string;
        }
    }
}

