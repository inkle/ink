using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	public class Stitch : FlowBase
	{ 
        public override FlowLevel flowLevel { get { return FlowLevel.Stitch; } }

        public Stitch (string name, List<Parsed.Object> topLevelObjects, List<Argument> arguments) : base(name, topLevelObjects, arguments)
		{
		}
	}
}

