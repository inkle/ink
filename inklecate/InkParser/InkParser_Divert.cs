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
            //  -> div -> div ->->   (etc)

            bool hasInitialTunnelOnwards = false;
            bool hasFinalTunnelOnwards = false;

            // Look at the arrows and diverts
            for (int i = 0; i < arrowsAndDiverts.Count; ++i) {
                bool isArrow = (i % 2) == 0;

                // Arrow string
                if (isArrow) {
                    string arrow = arrowsAndDiverts [i] as string;
                    if (arrow == "->->") {
                        if (i == 0) {
                            hasInitialTunnelOnwards = true;
                        } else if (i == arrowsAndDiverts.Count - 1) {
                            hasFinalTunnelOnwards = true;
                        } else {
                            Error ("Tunnel onwards '->->' must only come at the begining or the start of a divert");
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

            // ->-> (with optional override divert)
            if (hasInitialTunnelOnwards) {
                if (arrowsAndDiverts.Count > 2) {
                    Error ("Tunnel onwards '->->' must either be on its own or followed by a single target");
                }

                var tunnelOnwards = new TunnelOnwards ();

                // Optional override target to divert to after tunnel onwards?
                // Replace divert with the tunnel onwards to that target.
                if (arrowsAndDiverts.Count > 1) {
                    var overrideDivert = diverts [0] as Parsed.Divert;
                    tunnelOnwards.overrideReturnPath = overrideDivert.target;
                    diverts.RemoveAt (0);
                }

                diverts.Add (tunnelOnwards);
            }

            // Single ->
            else if (diverts.Count == 0 && arrowsAndDiverts.Count == 1) {
                var gatherDivert = new Divert ((Parsed.Object)null);
                gatherDivert.isToGather = true;
                diverts.Add (gatherDivert);
            }

            // Divert that terminates in ->->
            else if (hasFinalTunnelOnwards) {
                diverts.Add (new TunnelOnwards ());
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

