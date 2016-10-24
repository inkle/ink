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
            diverts = Parse(ParseMultiTunnel);
            if (diverts == null)
            {
                var res = (Object)OneOf(ParseTunnelOnwardsWithOverride, ParseTunnelOnwards, ParseNormalDivert,ParseGather);
                if (res == null)
                    return null;
                diverts = new List<Object>();
                diverts.Add(res);
            }
            return diverts;
        }

        protected Divert ParseNormalDivert()
        {
            //  -> div               -- normal divert
            if (ParseString("->") == null)
                return null;

            return Parse(DivertIdentifierWithArguments);
        }

        protected Divert ParseGather()
        {
            //  ->                   -- explicit gather
            if (ParseString("->") == null)
                return null;

            var div = new Divert(null);
            div.isToGather = true;
            return div;
        }

        protected TunnelOnwards ParseTunnelOnwards()
        {
            //  ->->                 -- tunnel onwards
            if (ParseString("->->") == null)
                return null;

            return new TunnelOnwards();
        }

        protected TunnelOnwards ParseTunnelOnwardsWithOverride()
        {
            //  ->-> div             -- tunnel onwards, followed by override divert
            if (ParseString("->->") == null)
                return null;

            var tunnelOnwards = new TunnelOnwards();

            var div = Parse<Divert>(DivertIdentifierWithArguments);

            if (div == null)
                return null;

            // Override target to divert to after tunnel onwards?
            // Replace divert with the tunnel onwards to that target.
            tunnelOnwards.overrideReturnPath = div.target;
            return tunnelOnwards;
        }

        protected List<Object> ParseMultiTunnel()
        {
            //  -> div ->            -- normal tunnel
            // also parses everything that starts with -> div ->
            if (ParseString("->") == null)
                return null;
            var div = Parse<Divert>(DivertIdentifierWithArguments);
            if (div == null)
                return null;
            if (ParseString("->") == null)
                return null;
            div.isTunnel = true;
            var continuedParse = Parse(ParseTunnelContinue);
            var ret = new List<Object>();
            ret.Add(div);
            if(continuedParse != null)
                ret.AddRange(continuedParse);
            return ret;
        }

        protected List<Object> ParseTunnelContinue()
        {
            System.Func<Object, SpecificParseRule<List<Object>>> helper = x => () =>
              {
                  var ret = new List<Object>();
                  ret.Add(x);
                  return ret;
              };

            //  -> div -> div ->->   (etc)
            SpecificParseRule<List<Object>> divThenTunnelOnwards =
                Require(DivertIdentifierWithArguments, x =>
                    Require<TunnelOnwards, List<Object>>(ParseTunnelOnwards, y =>
                        () => {
                            var ls = new List<Object>();
                            x.isTunnel = true;
                            ls.Add(x);
                            ls.Add(y);
                            return ls;
                        }));
 
            System.Func<Divert, SpecificParseRule<List<Object>>> furtherTunnelingHelper = 
                x => Require<List<Object>,List<Object>>(helper(x), ls => 
                    () => {
                        x.isTunnel = true;
                        var cont = ParseTunnelContinue();
                        if (cont != null)
                        {
                            ls.AddRange(cont);
                        }
                        return ls;
                    });
            //  -> div ->->          -- tunnel then tunnel continue
            // only parsing 1 -> since the other is already parsed for tunnel detection
            SpecificParseRule<List<Object>> tunnelOnwards = () =>
            {
                if (ParseString("->") == null)
                    return null;
                return helper(new TunnelOnwards())();
            };
            //  -> div -> div ->     -- tunnel then tunnel
            var furtherTunneling = Require(DivertIdentifierWithArguments, x => Require<string,List<Object>>(() => ParseString("->"), y => furtherTunnelingHelper(x)));
            //  -> div -> div        -- tunnel then divert
            var endingDiv = Require(DivertIdentifierWithArguments, helper);
            // note: order is important, since furtherTunneling also works for endingDiv, so we need to do most specific first, most general last
            return (List<Object>)OneOf(() => tunnelOnwards(), () => divThenTunnelOnwards(), () => furtherTunneling(), () => endingDiv());
        }
        

        protected Divert StartThread()
        {
            Whitespace ();

            if (ParseThreadArrow() == null)
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
            return (Divert)OneOf(ParseNormalDivert, ParseGather);
        }

        List<string> DotSeparatedDivertPathComponents()
        {
            return Interleave<string> (Spaced (Identifier), Exclude (String (".")));
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

