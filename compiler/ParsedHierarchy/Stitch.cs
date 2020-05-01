using System.Collections.Generic;

namespace Ink.Parsed
{
	public class Stitch : FlowBase
	{ 
        public override FlowLevel flowLevel { get { return FlowLevel.Stitch; } }

        public Stitch (string name, List<Parsed.Object> topLevelObjects, List<Argument> arguments, bool isFunction) : base(name, topLevelObjects, arguments, isFunction)
		{
		}
	}
}

