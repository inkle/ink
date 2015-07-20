using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	internal class Knot : FlowBase
	{
        public override FlowLevel flowLevel { get { return FlowLevel.Knot; } }

        public Knot (string name, List<Parsed.Object> topLevelObjects, List<Argument> arguments) : base(name, topLevelObjects, arguments)
		{
		}
            
	}
}

