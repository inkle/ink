using System.Collections.Generic;
using Ink.Parsed;


namespace Ink
{
    public partial class InkParser
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

            EndTagIfNecessary(diverts);

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

            List<Identifier> targetComponents = Parse (DotSeparatedDivertPathComponents);
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

            // Ideally we'd report errors if we get the
            // wrong kind of divert, but unfortunately we
            // have to hack around the fact that sequences use
            // a very similar syntax.
            // i.e. if you have a multi-divert at the start
            // of a sequence, it initially tries to parse it
            // as a divert target (part of an expression of
            // a conditional) and gives errors. So instead
            // we just have to blindly reject it as a single
            // divert, and give a slightly less nice error
            // when you DO use a multi divert as a divert taret.

            if (diverts.Count != 1) {
                return null;
            }

            var singleDivert = diverts [0];
            if (singleDivert is TunnelOnwards) {
                return null;
            }

            var divert = diverts [0] as Divert;
            if (divert.isTunnel) {
                return null;
            }

            return divert;
        }

        List<Identifier> DotSeparatedDivertPathComponents()
        {
            return Interleave<Identifier> (Spaced (IdentifierWithMetadata), Exclude (String (".")));
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

