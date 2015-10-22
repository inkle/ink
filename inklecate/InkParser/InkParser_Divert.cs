using System.Collections.Generic;
using Inklewriter.Parsed;


namespace Inklewriter
{
    internal partial class InkParser
    {
        protected List<Divert> MultiStepTunnelDivert()
        {
            Whitespace ();

            var arrowsAndDiverts = Interleave<object> (ParseDivertArrow, DivertIdentifierWithArguments);
            if (arrowsAndDiverts == null)
                return null;

            var diverts = new List<Divert> ();

            // Divert arrow only: "->"
            // Assume if there are no target components, it must be a divert to a gather point
            if (arrowsAndDiverts.Count == 1) {

                // Check whether we actually just accidentally parsed the onwards operator (->->)
                if (ParseString ("->") != null) {
                    return null;
                }

                var gatherDivert = new Divert ((Parsed.Object)null);
                gatherDivert.isToGather = true;
                diverts.Add (gatherDivert);
            }

            // Possible patterns:
            //  -> div               -- normal divert
            //  -> div ->            -- normal tunnel
            //  -> div -> div        -- tunnel then divert
            //  -> div -> div ->     -- tunnel then tunnel
            //  -> div -> div -> div -- tunnel then tunnel then divert
            // (etc)
            else {

                // Extract the diverts rather than the arrow strings
                for (int divIdx = 1; divIdx < arrowsAndDiverts.Count; divIdx += 2) {
                    var currentDivert = arrowsAndDiverts [divIdx] as Divert;

                    // More to come? (further arrows)
                    if (divIdx < arrowsAndDiverts.Count - 1) {
                        currentDivert.isTunnel = true;
                    }

                    diverts.Add (currentDivert);
                }

            }

            return diverts;
        }

        protected Divert StartThread()
        {
            Whitespace ();

            if (ParseString ("<-") == null)
                return null;

            Whitespace ();

            var divert = Expect(DivertIdentifierWithArguments, "Expected target for new thread") as Divert;
            divert.isThread = true;

            return divert;
        }

        protected Divert DivertIdentifierWithArguments()
        {
            Whitespace ();

            List<string> targetComponents = Parse (DotSeparatedDivertPathComponents);
            if (targetComponents == null)
                return null;

            Whitespace ();

            var optionalArguments = Parse(ExpressionFunctionCallArguments);

            Whitespace ();

            var targetPath = new Path (targetComponents);
            return new Divert (targetPath, optionalArguments);
        }

        protected Divert SingleDivert()
        {
            var diverts = Parse (MultiStepTunnelDivert);
            if (diverts == null)
                return null;

            if (diverts.Count != 1) {
                Error ("Expected just one single divert");
            }

            var divert = diverts [0];
            if (divert.isTunnel) {
                Error ("Didn't expect tunnel, but a normal divert");
                divert.isTunnel = false;
            }

            return divert;
        }

        List<string> DotSeparatedDivertPathComponents()
        {
            return Interleave<string> (Spaced (Identifier), Exclude (String (".")));
        }

        protected string ParseDivertArrow()
        {
            return ParseString ("->");
        }

        protected Parsed.TunnelOnwards TunnelOnwards()
        {
            Whitespace ();

            if ( ParseString ("->->") == null ) {
                return null;
            }

            return new TunnelOnwards ();
        }
    }
}

