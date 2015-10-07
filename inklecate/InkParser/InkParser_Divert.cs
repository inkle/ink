using System.Collections.Generic;
using Inklewriter.Parsed;


namespace Inklewriter
{
    internal partial class InkParser
    {
        protected Divert Divert()
        {
            Whitespace ();

            if (ParseDivertArrow() == null)
                return null;

            // Should always have components here unless it's a divert to a gather point,
            // in which case there isn't an explicit target, do we can't require them at parse time.
            List<string> targetComponents = Parse (DotSeparatedDivertPathComponents);

            Whitespace ();

            var optionalArguments = Parse(ExpressionFunctionCallArguments);

            // Assume if there are no target components, it must be a divert to a gather point
            if (targetComponents == null) {
                var gatherDivert = new Divert ((Parsed.Object)null);
                gatherDivert.isToGather = true;
                return gatherDivert;
            } 

            // Normal Divert to a normal Path
            else {
                var targetPath = new Path (targetComponents);
                return new Divert (targetPath, optionalArguments);
            }
        }

        List<string> DotSeparatedDivertPathComponents()
        {
            return Interleave<string> (Spaced (Identifier), Exclude (String (".")));
        }

        protected string ParseDivertArrow()
        {
            return ParseString ("->");
        }
    }
}

