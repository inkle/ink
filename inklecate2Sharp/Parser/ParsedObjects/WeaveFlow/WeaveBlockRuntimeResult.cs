using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    // Used exclusively by the FlowBase when constructing the weave flow from
    // a flat list of content objects.
    // "Result" isn't quite accurate - it contains most of the logic!
    public class WeaveBlockRuntimeResult
    {
        // Containers can be chained as multiple gather points
        // get created as the same indentation level.
        // rootContainer is always the first in the chain, while
        // currentContainer is the latest.
        public Runtime.Container rootContainer { get; }
        public Runtime.Container currentContainer { get; private set; }

        public List<IWeavePoint> looseEnds { get; }

        public delegate void LooseEndDelegate (Runtime.Divert divert, Gather gather);
        public LooseEndDelegate gatheredLooseEndDelegate { get; set; }

        public WeaveBlockRuntimeResult() {
            rootContainer = currentContainer = new Runtime.Container();
            looseEnds = new List<IWeavePoint> ();
        }

        // Found gather point:
        //  - gather any loose ends
        //  - set the gather as the main container to dump new content in
        public void StartGather(Gather gather)
        {
            var gatherContainer = gather.runtimeContainer;
            gatherContainer.name = "gather" + _gatherCount;
            _gatherCount++;

            // Consume loose ends: divert them to this gather
            foreach (IWeavePoint looseEnd in looseEnds) {

                if (looseEnd.hasLooseEnd) {
                    var divert = new Runtime.Divert ();
                    looseEnd.runtimeContainer.AddContent (divert);

                    // Pass back knowledge of this loose end being diverted
                    // to the FlowBase so that it can maintain a list of them,
                    // and resolve the divert references later
                    gatheredLooseEndDelegate (divert, gather);
                }
            }
            looseEnds.RemoveRange (0, looseEnds.Count);

            // Finally, add this gather to the main content, but only accessible
            // by name so that it isn't stepped into automatically, but only via
            // a divert from a loose end
            if (currentContainer.content.Count == 0) {
                currentContainer.AddContent (gatherContainer);
            } else {
                currentContainer.AddToNamedContentOnly (gatherContainer);
            }

            // Replace the current container itself
            currentContainer = gatherContainer;

            _latestLooseGather = gather;
        }

        public void AddWeavePoint(IWeavePoint weavePoint)
        {
            // Current level Gather
            if (weavePoint is Gather) {
                StartGather ((Gather)weavePoint);
            } 

            // Current level choice
            else if (weavePoint is Choice) {
                currentContainer.AddContent (((Choice)weavePoint).runtimeObject);
            }

            // TODO: Do further analysis on this weavePoint to determine whether
            // it really is a loose end (e.g. does it end in a divert)
            if (weavePoint.hasLooseEnd) {

                looseEnds.Add (weavePoint);

                // A gather stops becoming a loose end itself 
                // once it gets a choice
                if (_latestLooseGather != null && weavePoint is Choice) {
                    looseEnds.Remove (_latestLooseGather);
                }
            }

            previousWeavePoint = weavePoint;
        }

        // Add nested block at a greater indentation level
        public void AddNestedBlock(WeaveBlockRuntimeResult nestedResult)
        {
            // Add this inner block to current container
            // (i.e. within the main container, or within the last defined Choice/Gather)
            AddContent (nestedResult.rootContainer);

            // Append the indented block's loose ends to our own
            looseEnds.AddRange (nestedResult.looseEnds);

            // Now there's a deeper indentation level, the previous weave point doesn't
            // count as a loose end (since it will have content to go to)
            if (previousWeavePoint != null) {
                looseEnds.Remove (previousWeavePoint);
            }
        }

        // Normal content gets added into the latest Choice or Gather by default,
        // unless there hasn't been one yet.
        public void AddContent(Runtime.Object content)
        {
            if (previousWeavePoint != null) {
                previousWeavePoint.runtimeContainer.AddContent (content);
            } else {
                currentContainer.AddContent (content);
            }
        }

        // Keep track of previous weave point (Choice or Gather)
        // at the current indentation level:
        //  - to add ordinary content to be nested under it
        //  - to add nested content under it when it's indented
        //  - to remove it from the list of loose ends when it has
        //    indented content since it's no longer a loose end
        IWeavePoint previousWeavePoint = null;

        Gather _latestLooseGather;
        int _gatherCount;
    }
}

