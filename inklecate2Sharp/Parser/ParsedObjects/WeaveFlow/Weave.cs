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

        public List<GatherPointToResolve> gatherPointsToResolve;
        public class GatherPointToResolve
        {
            public Runtime.Divert divert;
            public Runtime.Object targetRuntimeObj;
        }
            
        public Weave(List<Parsed.Object> cont, int indentIndex=-1) 
        {
            if (indentIndex == -1) {
                baseIndentIndex = DetermineBaseIndentationFromContent (cont);
            } else {
                baseIndentIndex = indentIndex;
            }

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

        // When the indentation wasn't told to us at construction time using
        // a choice point with a known indentation level, we may be told to
        // determine the indentation level by incrementing from our closest ancestor.
        public int DetermineBaseIndentationFromContent(List<Parsed.Object> contentList)
        {
            foreach (var obj in contentList) {
                if (obj is IWeavePoint) {
                    return ((IWeavePoint)obj).indentationDepth - 1;
                }
            }

            // No weave points, so it doesn't matter
            return 0;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            _rootContainer = currentContainer = new Runtime.Container();
            looseEnds = new List<IWeavePoint> ();

            gatherPointsToResolve = new List<GatherPointToResolve> ();

            // Iterate through content for the block at this level of indentation
            //  - Normal content is nested under Choices and Gathers
            //  - Blocks that are further indented cause recursion
            //  - Keep track of loose ends so that they can be diverted to Gathers
            foreach(var obj in content) {

                // Choice or Gather
                if (obj is IWeavePoint) {
                    AddRuntimeForWeavePoint ((IWeavePoint)obj);
                } 

                // Nested weave
                else if (obj is Weave) {
                    var weave = (Weave)obj;
                    AddRuntimeForNestedWeave (weave);
                    gatherPointsToResolve.AddRange (weave.gatherPointsToResolve);
                }

                // Normal content
                else {
                    AddGeneralRuntimeContent (obj.runtimeObject);
                }
            }

            // Pass any loose ends up the hierarhcy
            PassLooseEndsToAncestors();

            return _rootContainer;
        }

        // Found gather point:
        //  - gather any loose ends
        //  - set the gather as the main container to dump new content in
        void AddRuntimeForGather(Gather gather)
        {
            var gatherContainer = gather.runtimeContainer;

            if (gather.name == null) {
                // Use disallowed character so it's impossible to have a name collision
                gatherContainer.name = "g-" + _unnamedGatherCount;
                _unnamedGatherCount++;
            }

            // Add this gather to the main content, but only accessible
            // by name so that it isn't stepped into automatically, but only via
            // a divert from a loose end.
            // However, at runtime, we detect whether there are no choices that
            // have been generated at this point, and if so, divert straight
            // into the gather.

            // (num choices == 0)?
            var gatherAutoDivertEvalStart = Runtime.ControlCommand.EvalStart();
            currentContainer.AddContent(gatherAutoDivertEvalStart);
            currentContainer.AddContent(Runtime.ControlCommand.ChoiceCount());
            currentContainer.AddContent (new Runtime.LiteralInt (0));
            currentContainer.AddContent (Runtime.NativeFunctionCall.CallWithName ("=="));
            currentContainer.AddContent(Runtime.ControlCommand.EvalEnd());

            // Branch into gather if true
            var autoEnterDivert = new Runtime.Divert ();
            currentContainer.AddContent (new Runtime.Branch (autoEnterDivert));

            // Ensure that the divert and gather have their references resolved
            gatherPointsToResolve.Add (new GatherPointToResolve{ divert = autoEnterDivert, targetRuntimeObj = gatherContainer });

            // Gather content itself is accessible by name only
            currentContainer.AddToNamedContentOnly (gatherContainer);

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
                gatherPointsToResolve.Add (new GatherPointToResolve{ divert = divert, targetRuntimeObj = gatherAutoDivertEvalStart });
            }
            looseEnds.RemoveRange (0, looseEnds.Count);

            // Replace the current container itself
            currentContainer = gatherContainer;
        }

        void AddRuntimeForWeavePoint(IWeavePoint weavePoint)
        {
            // Current level Gather
            if (weavePoint is Gather) {
                AddRuntimeForGather ((Gather)weavePoint);
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
        public void AddRuntimeForNestedWeave(Weave nestedResult)
        {
            // Add this inner block to current container
            // (i.e. within the main container, or within the last defined Choice/Gather)
            AddGeneralRuntimeContent (nestedResult.rootContainer);

            // Now there's a deeper indentation level, the previous weave point doesn't
            // count as a loose end (since it will have content to go to)
            if (previousWeavePoint != null) {
                looseEnds.Remove (previousWeavePoint);
                addContentToPreviousWeavePoint = false;
            }
        }

        // Normal content gets added into the latest Choice or Gather by default,
        // unless there hasn't been one yet.
        void AddGeneralRuntimeContent(Runtime.Object content)
        {
            if (addContentToPreviousWeavePoint) {
                previousWeavePoint.runtimeContainer.AddContent (content);
            } else {
                currentContainer.AddContent (content);
            }
        }

        void PassLooseEndsToAncestors()
        {
            if (looseEnds.Count > 0) {

                var weaveAncestor = closestWeaveAncestor;
                if (weaveAncestor != null) {
                    weaveAncestor.ReceiveLooseEnds (looseEnds);
                    looseEnds = null;
                }
            }

            if (looseEnds != null && looseEnds.Count > 0) {
                // TODO: When we *require* a return statement, show this error
                //Error ("unresolved loose ends");
            }
        }

        public void ReceiveLooseEnds(List<IWeavePoint> childWeaveLooseEnds)
        {
            looseEnds.AddRange (childWeaveLooseEnds);
        }

        public override void ResolveReferences(Story context)
        {
            base.ResolveReferences (context);

            foreach(var gatherPoint in gatherPointsToResolve) {
                gatherPoint.divert.targetPath = gatherPoint.targetRuntimeObj.path;
            }
        }

        Weave closestWeaveAncestor {
            get {
                var ancestor = this.parent;
                while (ancestor != null && !(ancestor is Weave)) {
                    ancestor = ancestor.parent;
                }
                return (Weave)ancestor;
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

