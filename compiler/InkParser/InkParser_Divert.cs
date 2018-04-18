using System.Collections.Generic;
using Ink.Parsed;


namespace Ink
{
    internal partial class InkParser
    {
        protected List<Parsed.Object> MultiDivert()
        {
            Whitespace ();

            List<Parsed.Object> diverts = null;

            // Try single thread first
            var threadDivert = Parse(StartThread);
            if (threadDivert) {
                diverts = new List<Object> ();
                diverts.Add (threadDivert);
                return diverts;
            }

            // Normal diverts and tunnels
            var arrowsAndDiverts = Interleave<object> (
                ParseDivertArrowOrTunnelOnwards,
                DivertIdentifierWithArguments);
            
            if (arrowsAndDiverts == null)
                return null;

            diverts = new List<Parsed.Object> ();

            // Possible patterns:
            //  ->                   -- explicit gather
            //  ->->                 -- tunnel onwards
            //  -> div               -- normal divert
            //  ->-> div             -- tunnel onwards, followed by override divert
            //  -> div ->            -- normal tunnel
            //  -> div ->->          -- tunnel then tunnel continue
            //  -> div -> div        -- tunnel then divert
            //  -> div -> div ->     -- tunnel then tunnel
            //  -> div -> div ->->   
            //  -> div -> div ->-> div    (etc)

            // Look at the arrows and diverts
            for (int i = 0; i < arrowsAndDiverts.Count; ++i) {
                bool isArrow = (i % 2) == 0;

                // Arrow string
                if (isArrow) {
                    
                    // Tunnel onwards
                    if ((string)arrowsAndDiverts [i] == "->->") {

                        bool tunnelOnwardsPlacementValid = (i == 0 || i == arrowsAndDiverts.Count - 1 || i == arrowsAndDiverts.Count - 2);
                        if (!tunnelOnwardsPlacementValid)
                            Error ("Tunnel onwards '->->' must only come at the begining or the start of a divert");

                        var tunnelOnwards = new TunnelOnwards ();
                        if (i < arrowsAndDiverts.Count - 1) {
                            var tunnelOnwardDivert = arrowsAndDiverts [i+1] as Parsed.Divert;
                            tunnelOnwards.divertAfter = tunnelOnwardDivert;
                        }

                        diverts.Add (tunnelOnwards);

                        // Not allowed to do anything after a tunnel onwards.
                        // If we had anything left it would be caused in the above Error for
                        // the positioning of a ->->
                        break;
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

            // Single -> (used for default choices)
            if (diverts.Count == 0 && arrowsAndDiverts.Count == 1) {
                var gatherDivert = new Divert ((Parsed.Object)null);
                gatherDivert.isEmpty = true;
                diverts.Add (gatherDivert);

                if (!_parsingChoice)
                    Error ("Empty diverts (->) are only valid on choices");
            }

            return diverts;
        }

        protected Divert StartThread()
        {
            Whitespace ();

            if (ParseThreadArrow() == null)
                return null;

            Whitespace ();

            var divert = Expect(DivertIdentifierWithArguments, "target for new thread", () => new Divert(null)) as Divert;
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
            var diverts = Parse (MultiDivert);
            if (diverts == null)
                return null;

            if (diverts.Count != 1) {
                Error ("Expected just one single divert");
            }

            var singleDivert = diverts [0];
            if (singleDivert is TunnelOnwards) {
                return null;
            }

            var divert = diverts [0] as Divert;
            if (divert.isTunnel) {
                Error ("Didn't expect tunnel, but a normal divert");

                // Convert to normal divert to continue parsing
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

        protected string ParseThreadArrow()
        {
            return ParseString ("<-");
        }
    }
}

