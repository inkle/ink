using System;
using System.Collections.Generic;
using System.Linq;

namespace Inklewriter.Parsed
{
	public class Path
	{
        public string knotName { get { return TryGetTargetAtLevel (FlowLevel.Knot); } }
        public string stitchName { get { return TryGetTargetAtLevel (FlowLevel.Stitch); } }
        public string weavePointName { get { return TryGetTargetAtLevel (FlowLevel.WeavePoint); } }
		public string ambiguousName { get; }

        public FlowLevel firstAddressedLevel { 
            get { 
                if (targetAtLevels.Count > 0) {
                    return targetAtLevels.First().Key;
                } else {
                    return FlowLevel.Knot;
                }
            }
        }

        public FlowLevel lastAddressedLevel {
            get { 
                if (targetAtLevels.Count > 0) {
                    return targetAtLevels.Last().Key;
                } else {
                    return FlowLevel.Knot;
                }
            }
        }

        // TODO: Upgrade to provide more alternatives for weave points
        // of the form DebugResolveAlternatives
        public Path debugSuggestedAlternative { 
            get {
                if (this.knotName != null) {
                    return Path.To (stitchName: this.knotName);
                } else if (this.stitchName != null) {
                    return Path.To (knotName: this.stitchName);
                }
                return null;
            }
        }

        public static Path To(string knotName = null, string stitchName = null, string weavePointName = null)
        {
            return new Path(knotName, stitchName, weavePointName);
        }

        public static Path ToAmbiguous(string name)
        {
            return new Path(ambiguousName: name);
        }

        // -----

        protected Path(string knotName = null, string stitchName = null, string weavePointName = null, string ambiguousName = null)
		{
            targetAtLevels = new SortedDictionary<FlowLevel, string> ();

            if (knotName != null)
                targetAtLevels.Add (FlowLevel.Knot, knotName);

            if (stitchName != null)
                targetAtLevels.Add (FlowLevel.Stitch, stitchName);

            if (weavePointName != null)
                targetAtLevels.Add (FlowLevel.WeavePoint, weavePointName);

			this.ambiguousName = ambiguousName;
		}
            
		public override string ToString ()
		{
            if (ambiguousName != null) {
                return "-?-> " + ambiguousName;
            }

            var components = new List<string> (); 

            foreach (var levelNamePair in targetAtLevels) {
                switch (levelNamePair.Key) {
                case FlowLevel.Knot:       components.Add ("==>"); break;
                case FlowLevel.Stitch:     components.Add ("=>");  break;
                case FlowLevel.WeavePoint: components.Add ("->");  break;
                }
                components.Add (levelNamePair.Value);
            }

            if (components.Count > 0) {
                return string.Join (" ", components);
            }

			return "<Unknown path>";
		}

        protected string TryGetTargetAtLevel(FlowLevel level)
        {
            string targetName = null;
            if (targetAtLevels.TryGetValue (level, out targetName)) {
                return targetName;
            } else {
                return null;
            }
        }

        public Parsed.Object ResolveFromContext(Parsed.Object context)
        {
            var startContext = context;

            // Find closest FlowBase in ancestry
            // (e.g. if we're in a Text, work our way up to the first Stitch etc)
            while(context != null && !(context is FlowBase)) {
                context = context.parent;
            }

            if (context == null || !(context is FlowBase)) {
                Console.WriteLine ("ERROR when resolving path: could not find a FlowBase when searching ancestry from " + startContext);
                return null;
            }

            var flowContext = (FlowBase)context;

            if (ambiguousName != null) {
                return ResolveAmbiguousFromContext (flowContext);
            }

            // Work our way up to the base level that we we search in
            // (subtract 1 since if the first addressed level is a knot, we
            //  want to search within a Story - the level up)
            while (this.firstAddressedLevel-1 < flowContext.flowLevel) {
                flowContext = (FlowBase) flowContext.parent;
            }

            // The foreach loop drill further into the path, e.g. for
            // multiple path components: ==> knot => stitch -> gather,
            // going further into the content.
            Parsed.Object content = null;
            foreach (var levelStringPair in targetAtLevels) {

                FlowLevel pathComponentLevel = levelStringPair.Key;
                string nameAtLevel = levelStringPair.Value;

                // Both knots and stories may contain e.g. stitches, so
                // we may still need to loop upwards to find the container
                // that owns the content type we're looping for

                do {
                    var foundContent = flowContext.ContentWithNameAtLevel (nameAtLevel, pathComponentLevel);

                    // Not found, keep searching upward for a FlowBase that contains
                    // content at this level flow level (e.g. try searching in a Story
                    // for a Stitch rather than in a Knot)
                    if (foundContent == null) {
                        flowContext = (FlowBase) flowContext.parent;
                    } 

                    // Found. 
                    else {
                        content = foundContent;

                        // If we continue to dig deeper, we now need to search within
                        // this content that we just found
                        if( content is FlowBase ) {
                            flowContext = (FlowBase)content;
                        }
                        break;
                    }
                        
                } while(flowContext != null && flowContext.flowLevel < pathComponentLevel);

            }

            return content;
        }

        Parsed.Object ResolveAmbiguousFromContext(FlowBase context)
        {
            do {
                var foundContent = context.ContentWithNameAtLevel(this.ambiguousName);
                if( foundContent != null ) {
                    return foundContent;
                } else {
                    context = (FlowBase) context.parent;
                }
            } while(context != null);

            return null;
        }

        SortedDictionary<FlowLevel, string> targetAtLevels;
	}
}

