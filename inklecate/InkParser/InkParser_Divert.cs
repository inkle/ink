using System.Collections.Generic;
using Ink.Parsed;


namespace Ink
{
    internal partial class InkParser
    {
        protected List<Parsed.Object> MultiStepTunnelDivert()
        {
            Whitespace ();

            var arrowsAndDiverts = Interleave<object> (
                ParseDivertArrowOrTunnelOnwards,
                DivertIdentifierWithArguments);
            
            if (arrowsAndDiverts == null)
                return null;

            var diverts = new List<Parsed.Object> ();

            // Divert arrow only:
            // ->
            // ->->
            // (with no target)
            if (arrowsAndDiverts.Count == 1) {

                // Single:
                // ->
                // Assume if there are no target components, it must be a divert to a gather point
                if ( (string) arrowsAndDiverts [0] == "->") {
                    var gatherDivert = new Divert ((Parsed.Object)null);
                    gatherDivert.isToGather = true;
                    diverts.Add (gatherDivert);
                } 

                // Double: (tunnel onwards)
                // ->->
                else {
                    diverts.Add (new TunnelOnwards());
                }

            }

            // Possible patterns:
            //  -> div               -- normal divert
            //  -> div ->            -- normal tunnel
            //  -> div ->->          -- tunnel then tunnel continue
            //  -> div -> div        -- tunnel then divert
            //  -> div -> div ->     -- tunnel then tunnel
            //  -> div -> div ->->   (etc)
            else {

                bool hasFinalTunnelOnwards = false;

                // Look at the arrows and diverts
                for (int i = 0; i < arrowsAndDiverts.Count; ++i) {
                    bool isArrow = (i % 2) == 0;

                    // Arrow string
                    if (isArrow) {
                        string arrow = arrowsAndDiverts [i] as string;
                        if (arrow == "->->") {
                            if (i == arrowsAndDiverts.Count - 1) {
                                hasFinalTunnelOnwards = true;
                            } else {
                                Error ("Unexpected content after a '->->' tunnel onwards");
                            }
                        }
                    }

                    // Divert
                    else {

                        var divert = arrowsAndDiverts [i] as Divert;

                        // More to come? (further arrows) Must be tunnelling.
                        if (i < arrowsAndDiverts.Count - 1) {
                            divert.isTunnel = true;
                        }

                        diverts.Add (divert);
                    }
                }

                if (hasFinalTunnelOnwards)
                    diverts.Add(new TunnelOnwards());
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

            var divert = diverts [0] as Divert;
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

        protected string ParseDivertArrowOrTunnelOnwards()
        {
            int numArrows = 0;
            while (ParseString ("->") != null)
                numArrows++;

            if (numArrows == 0)
                return null;

            else if (numArrows == 1)
                return "->";

            else if (numArrows == 2)
                return "->->";
            
            else {
                Error ("Unexpected number of arrows in divert. Should only have '->' or '->->'");
                return "->->";
            }
        }

        protected string ParseDivertArrow()
        {
            return ParseString ("->");
        }
    }
}

