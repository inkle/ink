using System.Collections.Generic;

namespace Ink.Parsed
{
	public class Knot : FlowBase
	{
        public override FlowLevel flowLevel { get { return FlowLevel.Knot; } }

        public Knot (string name, List<Parsed.Object> topLevelObjects, List<Argument> arguments, bool isFunction) : base(name, topLevelObjects, arguments, isFunction)
		{
		}

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            var parentStory = this.story;

            // Enforce rule that stitches must not have the same
            // name as any knots that exist in the story
            foreach (var stitchNamePair in subFlowsByName) {
                var stitchName = stitchNamePair.Key;

                var knotWithStitchName = parentStory.ContentWithNameAtLevel (stitchName, FlowLevel.Knot, false);
                if (knotWithStitchName) {
                    var stitch = stitchNamePair.Value;
                    var errorMsg = string.Format ("Stitch '{0}' has the same name as a knot (on {1})", stitch.name, knotWithStitchName.debugMetadata);
                    Error(errorMsg, stitch);
                }
            }
        }
            
	}
}

