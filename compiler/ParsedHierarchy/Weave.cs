using System.Collections.Generic;

namespace Ink.Parsed
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
        Runtime.Container currentContainer { get; set; }

		public int baseIndentIndex { get; private set; }

        // Loose ends are:
        //  - Choices or Gathers that need to be joined up
        //  - Explicit Divert to gather points (i.e. "->" without a target)
        public List<IWeavePoint> looseEnds;

        public List<GatherPointToResolve> gatherPointsToResolve;
        public class GatherPointToResolve
        {
            public Runtime.Divert divert;
            public Runtime.Object targetRuntimeObj;
        }

        public Parsed.Object lastParsedSignificantObject
        {
            get {
                if (content.Count == 0) return null;

                // Don't count extraneous newlines or VAR/CONST declarations,
                // since they're "empty" statements outside of the main flow.
                Parsed.Object lastObject = null;
                for (int i = content.Count - 1; i >= 0; --i) {
                    lastObject = content [i];

                    var lastText = lastObject as Parsed.Text;
                    if (lastText && lastText.text == "\n") {
                        continue;
                    }

                    if (IsGlobalDeclaration (lastObject))
                        continue;
                    
                    break;
                }

                var lastWeave = lastObject as Weave;
                if (lastWeave)
                    lastObject = lastWeave.lastParsedSignificantObject;
                
                return lastObject;
            }
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

        public void ResolveWeavePointNaming ()
        {
            var namedWeavePoints = FindAll<IWeavePoint> (w => !string.IsNullOrEmpty (w.name));

            _namedWeavePoints = new Dictionary<string, IWeavePoint> ();

            foreach (var weavePoint in namedWeavePoints) {

                // Check for weave point naming collisions
                IWeavePoint existingWeavePoint;
                if (_namedWeavePoints.TryGetValue (weavePoint.name, out existingWeavePoint)) {
                    var typeName = existingWeavePoint is Gather ? "gather" : "choice";
                    var existingObj = (Parsed.Object)existingWeavePoint;

                    Error ("A " + typeName + " with the same label name '" + weavePoint.name + "' already exists in this context on line " + existingObj.debugMetadata.startLineNumber, (Parsed.Object)weavePoint);
                }

                _namedWeavePoints [weavePoint.name] = weavePoint;
            }
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

                        // Continue iteration from this point
                        contentIdx = innerWeaveStartIdx;
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

                // Non-weave point
                else {

                    // Nested weave
                    if (obj is Weave) {
                        var weave = (Weave)obj;
                        AddRuntimeForNestedWeave (weave);
                        gatherPointsToResolve.AddRange (weave.gatherPointsToResolve);
                    }

                    // Other object
                    // May be complex object that contains statements - e.g. a multi-line conditional
                    else {
                        AddGeneralRuntimeContent (obj.runtimeObject);
                    }
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
            // Determine whether this Gather should be auto-entered:
            //  - It is auto-entered if there were no choices in the last section
            //  - A section is "since the previous gather" - so reset now
            bool autoEnter = !hasSeenChoiceInSection;
            hasSeenChoiceInSection = false;

            var gatherContainer = gather.runtimeContainer;

            if (gather.name == null) {
                // Use disallowed character so it's impossible to have a name collision
                gatherContainer.name = "g-" + _unnamedGatherCount;
                _unnamedGatherCount++;
            }
                
            // Auto-enter: include in main content
            if (autoEnter) {
                currentContainer.AddContent (gatherContainer);
            } 

            // Don't auto-enter:
            // Add this gather to the main content, but only accessible
            // by name so that it isn't stepped into automatically, but only via
            // a divert from a loose end.
            else {
                _rootContainer.AddToNamedContentOnly (gatherContainer);
            }

            // Consume loose ends: divert them to this gather
            foreach (IWeavePoint looseEndWeavePoint in looseEnds) {

                var looseEnd = (Parsed.Object)looseEndWeavePoint;

                // Skip gather loose ends that are at the same level
                // since they'll be handled by the auto-enter code below
                // that only jumps into the gather if (current runtime choices == 0)
                if (looseEnd is Gather) {
                    var prevGather = (Gather)looseEnd;
                    if (prevGather.indentationDepth == gather.indentationDepth) {
                        continue;
                    }
                }

                Runtime.Divert divert = null;

                if (looseEnd is Parsed.Divert) {
                    divert = (Runtime.Divert) looseEnd.runtimeObject;
                } else {
                    divert = new Runtime.Divert ();
                    var looseWeavePoint = looseEnd as IWeavePoint;
                    looseWeavePoint.runtimeContainer.AddContent (divert);
                }
                   
                // Pass back knowledge of this loose end being diverted
                // to the FlowBase so that it can maintain a list of them,
                // and resolve the divert references later
                gatherPointsToResolve.Add (new GatherPointToResolve{ divert = divert, targetRuntimeObj = gatherContainer });
            }
            looseEnds.Clear ();

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

                // Gathers that contain choices are no longer loose ends
                // (same as when weave points get nested content)
                if (previousWeavePoint is Gather) {
                    looseEnds.Remove (previousWeavePoint);
                }

                // Add choice point content
                var choice = (Choice)weavePoint;
                currentContainer.AddContent (choice.runtimeObject);

                // Add choice's inner content to self
                choice.innerContentContainer.name = "c-" + _choiceCount;
                currentContainer.AddToNamedContentOnly (choice.innerContentContainer);
                _choiceCount++;

                hasSeenChoiceInSection = true;
            }

            // Keep track of loose ends
            addContentToPreviousWeavePoint = false; // default
            if (WeavePointHasLooseEnd (weavePoint)) {
                looseEnds.Add (weavePoint);


                var looseChoice = weavePoint as Choice;
                if (looseChoice) {
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
            // Content is allowed to evaluate runtimeObject to null
            // (e.g. AuthorWarning, which doesn't make it into the runtime)
            if (content == null)
                return;
            
            if (addContentToPreviousWeavePoint) {
                previousWeavePoint.runtimeContainer.AddContent (content);
            } else {
                currentContainer.AddContent (content);
            }
        }

        void PassLooseEndsToAncestors()
        {
            if (looseEnds.Count == 0) return;

            // Search for Weave ancestor to pass loose ends to for gathering.
            // There are two types depending on whether the current weave
            // is separated by a conditional or sequence.
            //  - An "inner" weave is one that is directly connected to the current
            //    weave - i.e. you don't have to pass through a conditional or
            //    sequence to get to it. We're allowed to pass all loose ends to
            //    one of these.
            //  - An "outer" weave is one that is outside of a conditional/sequence
            //    that the current weave is nested within. We're only allowed to
            //    pass gathers (i.e. 'normal flow') loose ends up there, not normal
            //    choices. The rule is that choices have to be diverted explicitly
            //    by the author since it's ambiguous where flow should go otherwise.
            //
            // e.g.:
            //
            //   - top                       <- e.g. outer weave
            //   {true:
            //       * choice                <- e.g. inner weave
            //         * * choice 2
            //             more content      <- e.g. current weave
            //       * choice 2
            //   }
            //   - more of outer weave
            //
            Weave closestInnerWeaveAncestor = null;
            Weave closestOuterWeaveAncestor = null;

            // Find inner and outer ancestor weaves as defined above.
            bool nested = false;
            for (var ancestor = this.parent; ancestor != null; ancestor = ancestor.parent)
            {

                // Found ancestor?
                var weaveAncestor = ancestor as Weave;
                if (weaveAncestor != null)
                {
                    if (!nested && closestInnerWeaveAncestor == null)
                        closestInnerWeaveAncestor = weaveAncestor;

                    if (nested && closestOuterWeaveAncestor == null)
                        closestOuterWeaveAncestor = weaveAncestor;
                }


                // Weaves nested within Sequences or Conditionals are
                // "sealed" - any loose ends require explicit diverts.
                if (ancestor is Sequence || ancestor is Conditional)
                    nested = true;
            }

            // No weave to pass loose ends to at all?
            if (closestInnerWeaveAncestor == null && closestOuterWeaveAncestor == null)
                return;

            // Follow loose end passing logic as defined above
            for (int i = looseEnds.Count - 1; i >= 0; i--) {
                var looseEnd = looseEnds[i];

                bool received = false;

                // This weave is nested within a conditional or sequence:
                //  - choices can only be passed up to direct ancestor ("inner") weaves
                //  - gathers can be passed up to either, but favour the closer (inner) weave
                //    if there is one
                if(nested) {
                    if( looseEnd is Choice && closestInnerWeaveAncestor != null) {
                        closestInnerWeaveAncestor.ReceiveLooseEnd(looseEnd);
                        received = true;
                    }

                    else if( !(looseEnd is Choice) ) {
                        var receivingWeave = closestInnerWeaveAncestor ?? closestOuterWeaveAncestor;
                        if(receivingWeave != null) {
                            receivingWeave.ReceiveLooseEnd(looseEnd);
                            received = true;
                        }
                    }
                }

                // No nesting, all loose ends can be safely passed up
                else {
                    closestInnerWeaveAncestor.ReceiveLooseEnd(looseEnd);
                    received = true;
                }

                if(received) looseEnds.RemoveAt(i);
            }
        }

        void ReceiveLooseEnd(IWeavePoint childWeaveLooseEnd)
        {
            looseEnds.Add(childWeaveLooseEnd);
        }

        public override void ResolveReferences(Story context)
        {
            base.ResolveReferences (context);

            // Check that choices nested within conditionals and sequences are terminated
            if( looseEnds != null && looseEnds.Count > 0 ) {
                var isNestedWeave = false;
                for (var ancestor = this.parent; ancestor != null; ancestor = ancestor.parent)
                {
                    if (ancestor is Sequence || ancestor is Conditional)
                    {
                        isNestedWeave = true;
                        break;
                    }
                }
                if (isNestedWeave)
                {
                    ValidateTermination(BadNestedTerminationHandler);
                }
            }

            foreach(var gatherPoint in gatherPointsToResolve) {
                gatherPoint.divert.targetPath = gatherPoint.targetRuntimeObj.path;
            }
                
            CheckForWeavePointNamingCollisions ();
        }

        public IWeavePoint WeavePointNamed(string name)
        {
            if (_namedWeavePoints == null)
                return null;

            IWeavePoint weavePointResult = null;
            if (_namedWeavePoints.TryGetValue (name, out weavePointResult))
                return weavePointResult;

            return null;
        }

        // Global VARs and CONSTs are treated as "outside of the flow"
        // when iterating over content that follows loose ends
        bool IsGlobalDeclaration (Parsed.Object obj)
        {

            var varAss = obj as VariableAssignment;
            if (varAss && varAss.isGlobalDeclaration && varAss.isDeclaration)
                return true;
            
            var constDecl = obj as ConstantDeclaration;
            if (constDecl)
                return true;

            return false;
        }

        // While analysing final loose ends, we look to see whether there
        // are any diverts etc which choices etc divert from
        IEnumerable<Parsed.Object> ContentThatFollowsWeavePoint (IWeavePoint weavePoint)
        {
            var obj = (Parsed.Object)weavePoint;

            // Inner content first (e.g. for a choice)
            if (obj.content != null) {
                foreach (var contentObj in obj.content) {

                    // Global VARs and CONSTs are treated as "outside of the flow"
                    if (IsGlobalDeclaration (contentObj)) continue;

                    yield return contentObj;
                }
            }


            var parentWeave = obj.parent as Weave;
            if (parentWeave == null) {
                throw new System.Exception ("Expected weave point parent to be weave?");
            }

            var weavePointIdx = parentWeave.content.IndexOf (obj);

            for (int i = weavePointIdx+1; i < parentWeave.content.Count; i++) {
                var laterObj = parentWeave.content [i];

                // Global VARs and CONSTs are treated as "outside of the flow"
                if (IsGlobalDeclaration (laterObj)) continue;

                // End of the current flow
                if (laterObj is IWeavePoint) 
                    break;

                // Other weaves will be have their own loose ends
                if (laterObj is Weave)
                    break;

                yield return laterObj;
            }
        }

        public delegate void BadTerminationHandler (Parsed.Object terminatingObj);
        public void ValidateTermination (BadTerminationHandler badTerminationHandler)
        {
            // Don't worry if the last object in the flow is a "TODO",
            // even if there are other loose ends in other places
            if (lastParsedSignificantObject is AuthorWarning) {
                return;
            }

            // By now, any sub-weaves will have passed loose ends up to the root weave (this).
            // So there are 2 possible situations:
            //  - There are loose ends from somewhere in the flow.
            //    These aren't necessarily "real" loose ends - they're weave points
            //    that don't connect to any lower weave points, so we just
            //    have to check that they terminate properly.
            //  - This weave is just a list of content with no actual weave points,
            //    so we just need to check that the list of content terminates.

            bool hasLooseEnds = looseEnds != null && looseEnds.Count > 0;

            if (hasLooseEnds) {
                foreach (var looseEnd in looseEnds) {
                    var looseEndFlow = ContentThatFollowsWeavePoint (looseEnd);
                    ValidateFlowOfObjectsTerminates (looseEndFlow, (Parsed.Object)looseEnd, badTerminationHandler);
                }
            }

            // No loose ends... is there any inner weaving at all?
            // If not, make sure the single content stream is terminated correctly
            else {

                // If there's any actual weaving, assume that content is 
                // terminated correctly since we would've had a loose end otherwise
                foreach (var obj in content) {
                    if (obj is IWeavePoint) return;
                }

                // Straight linear flow? Check it terminates
                ValidateFlowOfObjectsTerminates (content, this, badTerminationHandler);
            }
        }

        void BadNestedTerminationHandler(Parsed.Object terminatingObj)
        {
            Conditional conditional = null;
            for (var ancestor = terminatingObj.parent; ancestor != null; ancestor = ancestor.parent) {
                if( ancestor is Sequence || ancestor is Conditional ) {
                    conditional = ancestor as Conditional;
                    break;
                }
            }

            var errorMsg = "Choices nested in conditionals or sequences need to explicitly divert afterwards.";

            // Tutorialise proper choice syntax if this looks like a single choice within a condition, e.g.
            // { condition:
            //      * choice
            // }
            if (conditional != null) {
                var numChoices = conditional.FindAll<Choice>().Count;
                if( numChoices == 1 ) {
                    errorMsg = "Choices with conditions should be written: '* {condition} choice'. Otherwise, "+ errorMsg.ToLower();
                }
            }

            Error(errorMsg, terminatingObj);
        }

        void ValidateFlowOfObjectsTerminates (IEnumerable<Parsed.Object> objFlow, Parsed.Object defaultObj, BadTerminationHandler badTerminationHandler)
        {
            bool terminated = false;
            Parsed.Object terminatingObj = defaultObj;
            foreach (var flowObj in objFlow) {

                var divert = flowObj.Find<Divert> (d => !d.isThread && !d.isTunnel && !d.isFunctionCall && !(d.parent is DivertTarget));
                if (divert != null) {
                    terminated = true;
                }

                if (flowObj.Find<TunnelOnwards> () != null) {
                    terminated = true;
                    break;
                }

                terminatingObj = flowObj;
            }


            if (!terminated) {

                // Author has left a note to self here - clearly we don't need
                // to leave them with another warning since they know what they're doing.
                if (terminatingObj is AuthorWarning) {
                    return;
                }

                badTerminationHandler (terminatingObj);
            }
        }
            
        bool WeavePointHasLooseEnd(IWeavePoint weavePoint)
        {
            // No content, must be a loose end.
            if (weavePoint.content == null) return true;

            // If a weave point is diverted from, it doesn't have a loose end.
            // Detect a divert object within a weavePoint's main content
            // Work backwards since we're really interested in the end,
            // although it doesn't actually make a difference!
            // (content after a divert will simply be inaccessible)
            for (int i = weavePoint.content.Count - 1; i >= 0; --i) {
                var innerDivert = weavePoint.content [i] as Divert;
                if (innerDivert) {
                    bool willReturn = innerDivert.isThread || innerDivert.isTunnel || innerDivert.isFunctionCall;
                    if (!willReturn) return false;
                }
            }

            return true;
        }

        // Enforce rule that weave points must not have the same
        // name as any stitches or knots upwards in the hierarchy
        void CheckForWeavePointNamingCollisions()
        {
            if (_namedWeavePoints == null)
                return;
            
            var ancestorFlows = new List<FlowBase> ();
            foreach (var obj in this.ancestry) {
                var flow = obj as FlowBase;
                if (flow)
                    ancestorFlows.Add (flow);
                else
                    break;
            }


            foreach (var namedWeavePointPair in _namedWeavePoints) {
                var weavePointName = namedWeavePointPair.Key;
                var weavePoint = (Parsed.Object) namedWeavePointPair.Value;

                foreach(var flow in ancestorFlows) {

                    // Shallow search
                    var otherContentWithName = flow.ContentWithNameAtLevel (weavePointName);

                    if (otherContentWithName && otherContentWithName != weavePoint) {
                        var errorMsg = string.Format ("{0} '{1}' has the same label name as a {2} (on {3})", 
                            weavePoint.GetType().Name, 
                            weavePointName, 
                            otherContentWithName.GetType().Name, 
                            otherContentWithName.debugMetadata);

                        Error(errorMsg, (Parsed.Object) weavePoint);
                    }

                }
            }
        }

        // Keep track of previous weave point (Choice or Gather)
        // at the current indentation level:
        //  - to add ordinary content to be nested under it
        //  - to add nested content under it when it's indented
        //  - to remove it from the list of loose ends when
        //     - it has indented content since it's no longer a loose end
        //     - it's a gather and it has a choice added to it
        IWeavePoint previousWeavePoint = null;
        bool addContentToPreviousWeavePoint = false;

        // Used for determining whether the next Gather should auto-enter
        bool hasSeenChoiceInSection = false;

        int _unnamedGatherCount;

        int _choiceCount;


        Runtime.Container _rootContainer;
        Dictionary<string, IWeavePoint> _namedWeavePoints;
    }
}

