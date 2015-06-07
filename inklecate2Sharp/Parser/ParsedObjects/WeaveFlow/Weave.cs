using System;
using System.Collections.Generic;
using System.Linq;

namespace Inklewriter.Parsed
{
    // Used by the FlowBase when constructing the weave flow from
    // a flat list of content objects.
    public class Weave : Parsed.Object
    {
        // Containers can be chained as multiple gather points
        // get created as the same indentation level.
        // rootContainer is always the first in the chain, while
        // currentContainer is the latest.
        public Runtime.Container rootContainer { 
            get {
                if (_rootContainer == null) {
                    GenerateRuntimeObject ();
                }

                return _rootContainer;
            }
        }
        public Runtime.Container currentContainer { get; private set; }
        public int baseIndentIndex { get; }

        public List<IWeavePoint> looseEnds;

        public List<LooseEndToResolve> looseEndReferencesToResolve;
        public class LooseEndToResolve
        {
            public Runtime.Divert divert;
            public Gather targetGather;
        }
            
        public Weave(List<Parsed.Object> cont, int indentIndex=0) 
        {
            baseIndentIndex = indentIndex;

            AddContent (cont);

            ConstructWeaveHierarchyFromIndentation ();
        }

        void ConstructWeaveHierarchyFromIndentation()
        {
            // Find nested indentation and convert to a proper object hierarchy
            // (i.e. indented content is replaced with a Weave object that contains
            // that nested content)
            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];

                // Choice or Gather
                if (obj is IWeavePoint) {
                    var weavePoint = (IWeavePoint)obj;
                    var weaveIndentIdx = weavePoint.indentationDepth - 1;

                    // Inner level indentation - recurse
                    if (weaveIndentIdx > baseIndentIndex) {

                        // Step through content until indent jumps out again
                        int innerWeaveStartIdx = contentIdx;
                        while (contentIdx < content.Count) {
                            var innerWeaveObj = content [contentIdx] as IWeavePoint;
                            if (innerWeaveObj != null) {
                                var innerIndentIdx = innerWeaveObj.indentationDepth - 1;
                                if (innerIndentIdx <= baseIndentIndex) {
                                    break;
                                }
                            }

                            contentIdx++;
                        }

                        int weaveContentCount = contentIdx - innerWeaveStartIdx;

                        var weaveContent = content.GetRange (innerWeaveStartIdx, weaveContentCount);
                        content.RemoveRange (innerWeaveStartIdx, weaveContentCount);

                        var weave = new Weave (weaveContent, weaveIndentIdx);
                        InsertContent (innerWeaveStartIdx, weave);

                        // contentIdx is already incremented at this point
                        continue;
                    }

                } 

                contentIdx++;
            }
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            _rootContainer = currentContainer = new Runtime.Container();
            looseEnds = new List<IWeavePoint> ();

            looseEndReferencesToResolve = new List<LooseEndToResolve> ();

            // TODO: Gather loose ends
            //result.gatheredLooseEndDelegate = OnLooseEndGathered;

            // Iterate through content for the block at this level of indentation
            //  - Normal content is nested under Choices and Gathers
            //  - Blocks that are further indented cause recursion
            //  - Keep track of loose ends so that they can be diverted to Gathers
            foreach(var obj in content) {

                // Choice or Gather
                if (obj is IWeavePoint) {
                    AddWeavePoint ((IWeavePoint)obj);
                } 

                // Nested weave
                else if (obj is Weave) {
                    var weave = (Weave)obj;
                    AddNestedBlock (weave);
                    looseEndReferencesToResolve.AddRange (weave.looseEndReferencesToResolve);
                }

                // Normal content
                else {
                    AddRuntimeContent (obj.runtimeObject);
                }
            }

            return _rootContainer;
        }

        // Found gather point:
        //  - gather any loose ends
        //  - set the gather as the main container to dump new content in
        public void StartGather(Gather gather)
        {
            var gatherContainer = gather.runtimeContainer;

            if (gather.name == null) {
                // Use disallowed character so it's impossible to have a name collision
                gatherContainer.name = "g-" + _unnamedGatherCount;
                _unnamedGatherCount++;
            }

            // Consume loose ends: divert them to this gather
            foreach (IWeavePoint looseEnd in looseEnds) {

                // Skip gather loose ends that are at the same level
                // since they'll be handled by the auto-enter code below
                // that only jumps into the gather if (current runtime choices == 0)
                if (looseEnd is Gather) {
                    var prevGather = (Gather)looseEnd;
                    if (prevGather.indentationDepth == gather.indentationDepth) {
                        continue;
                    }
                }

                var divert = new Runtime.Divert ();
                looseEnd.runtimeContainer.AddContent (divert);

                // Pass back knowledge of this loose end being diverted
                // to the FlowBase so that it can maintain a list of them,
                // and resolve the divert references later
                looseEndReferencesToResolve.Add (new LooseEndToResolve{ divert = divert, targetGather = gather });
            }
            looseEnds.RemoveRange (0, looseEnds.Count);

            // Finally, add this gather to the main content, but only accessible
            // by name so that it isn't stepped into automatically, but only via
            // a divert from a loose end.
            // However, at runtime, we detect whether there are no choices that
            // have been generated at this point, and if so, divert straight
            // into the gather.

            // (num choices == 0)?
            currentContainer.AddContent(Runtime.ControlCommand.EvalStart());
            currentContainer.AddContent(Runtime.ControlCommand.ChoiceCount());
            currentContainer.AddContent (new Runtime.LiteralInt (0));
            currentContainer.AddContent (Runtime.NativeFunctionCall.CallWithName ("=="));
            currentContainer.AddContent(Runtime.ControlCommand.EvalEnd());

            // Branch into gather if true
            var autoEnterDivert = new Runtime.Divert ();
            currentContainer.AddContent (new Runtime.Branch (autoEnterDivert));

            // Ensure that the divert and gather have their references resolved
            looseEndReferencesToResolve.Add (new LooseEndToResolve{ divert = autoEnterDivert, targetGather = gather });

            // Gather content itself is accessible by name only
            currentContainer.AddToNamedContentOnly (gatherContainer);

            // Replace the current container itself
            currentContainer = gatherContainer;
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

            // Keep track of loose ends
            addContentToPreviousWeavePoint = false; // default
            if (WeavePointHasLooseEnd (weavePoint)) {
                looseEnds.Add (weavePoint);

                // If choice has an explicit gather divert ("->") then
                var looseChoice = weavePoint as Choice;
                if (looseChoice != null && !looseChoice.explicitGather) {
                    addContentToPreviousWeavePoint = true;
                }
            }
            previousWeavePoint = weavePoint;
        }

        // Add nested block at a greater indentation level
        public void AddNestedBlock(Weave nestedResult)
        {
            // Add this inner block to current container
            // (i.e. within the main container, or within the last defined Choice/Gather)
            AddRuntimeContent (nestedResult.rootContainer);

            // Append the indented block's loose ends to our own
            looseEnds.AddRange (nestedResult.looseEnds);

            // Now there's a deeper indentation level, the previous weave point doesn't
            // count as a loose end (since it will have content to go to)
            if (previousWeavePoint != null) {
                looseEnds.Remove (previousWeavePoint);
                addContentToPreviousWeavePoint = false;
            }
        }

        // Normal content gets added into the latest Choice or Gather by default,
        // unless there hasn't been one yet.
        public void AddRuntimeContent(Runtime.Object content)
        {
            if (addContentToPreviousWeavePoint) {
                previousWeavePoint.runtimeContainer.AddContent (content);
            } else {
                currentContainer.AddContent (content);
            }
        }

        public override void ResolveReferences(Story context)
        {
            base.ResolveReferences (context);

            foreach(var looseEnd in looseEndReferencesToResolve) {
                looseEnd.divert.targetPath = looseEnd.targetGather.runtimePath;
            }
        }
            
        bool WeavePointHasLooseEnd(IWeavePoint weavePoint)
        {
            // Simple choice with explicit divert 
            // definitely doesn't have a loose end
            if (weavePoint is Choice) {
                var choice = (Choice)weavePoint;
                if (choice.explicitPath != null) {
                    return false;
                }
                if (choice.explicitGather) {
                    return true;
                }
            }

            // No content, and no simple divert above, must be a loose end.
            // (content list on Choices gets created on demand, hence how
            // it could be null)
            if (weavePoint.content == null) {
                return true;
            }

            // Detect a divert object within a weavePoint's main content
            // Work backwards since we're really interested in the end,
            // although it doesn't actually make a difference!
            else {
                for (int i = weavePoint.content.Count - 1; i >= 0; --i) {
                    var innerDivert = weavePoint.content [i] as Divert;
                    if (innerDivert != null && !innerDivert.isToGather) {
                        return false;
                    }
                }

                return true;
            }
        }



        // Keep track of previous weave point (Choice or Gather)
        // at the current indentation level:
        //  - to add ordinary content to be nested under it
        //  - to add nested content under it when it's indented
        //  - to remove it from the list of loose ends when it has
        //    indented content since it's no longer a loose end
        IWeavePoint previousWeavePoint = null;
        bool addContentToPreviousWeavePoint = false;

        int _unnamedGatherCount;


        Runtime.Container _rootContainer;
    }
}

