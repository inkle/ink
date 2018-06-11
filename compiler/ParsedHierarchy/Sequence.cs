using System.Collections.Generic;

namespace Ink.Parsed
{
    public enum SequenceType
    {
        Stopping, // default
        Cycle,
        Shuffle,
        Once
    }

    internal class Sequence : Parsed.Object
    {

        public List<Parsed.Object> sequenceElements;
        public SequenceType sequenceType;

        public Sequence (List<ContentList> elementContentLists, SequenceType sequenceType)
        {
            this.sequenceType = sequenceType;
            this.sequenceElements = new List<Parsed.Object> ();

            foreach (var elementContentList in elementContentLists) {

                var contentObjs = elementContentList.content;

                Parsed.Object seqElObject = null;

                // Don't attempt to create a weave for the sequence element 
                // if the content list is empty. Weaves don't like it!
                if (contentObjs == null || contentObjs.Count == 0)
                    seqElObject = elementContentList;
                else
                    seqElObject = new Weave (contentObjs);
                
                this.sequenceElements.Add (seqElObject);
                AddContent (seqElObject);
            }
        }

        // Generate runtime code that looks like:
        //   chosenIndex = MIN(sequence counter, num elements) e.g. for "Stopping"
        //   if chosenIndex == 0, divert to s0
        //   if chosenIndex == 1, divert to s1  [etc]
        //   increment sequence
        //
        //   - s0:
        //      <content for sequence element>
        //      divert back to increment point
        //   - s1:
        //      <content for sequence element>
        //      divert back to increment point
        //
        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();
            container.visitsShouldBeCounted = true;
            container.countingAtStartOnly = true;

            _sequenceDivertsToResove = new List<SequenceDivertToResolve> ();

            // Get sequence read count
            container.AddContent (Runtime.ControlCommand.EvalStart ());
            container.AddContent (Runtime.ControlCommand.VisitIndex ());

            // Chosen sequence index:
            //  - Stopping: take the MIN(read count, num elements - 1)
            if (sequenceType == SequenceType.Stopping) {
                container.AddContent (new Runtime.IntValue (sequenceElements.Count - 1));
                container.AddContent (Runtime.NativeFunctionCall.CallWithName ("MIN"));
            } 

            // - Cycle: take (read count % num elements)
            else if (sequenceType == SequenceType.Cycle) {
                container.AddContent (new Runtime.IntValue (sequenceElements.Count));
                container.AddContent (Runtime.NativeFunctionCall.CallWithName ("%"));
            }

            // Once: allow sequence count to be unbounded
            else if (sequenceType == SequenceType.Once) {
                // Do nothing - the sequence count will simply prevent
                // any content being referenced when it goes out of bounds
            } 

            // Shuffle
            else if (sequenceType == SequenceType.Shuffle) {
                // This one's a bit more complex! Choose the index at runtime.
                container.AddContent (new Runtime.IntValue (sequenceElements.Count));
                container.AddContent (Runtime.ControlCommand.SequenceShuffleIndex ());
            }

            // Not implemented
            else {
                throw new System.NotImplementedException ();
            }

            container.AddContent (Runtime.ControlCommand.EvalEnd ());

            // Create point to return to when sequence is complete
            var postSequenceNoOp = Runtime.ControlCommand.NoOp ();

            var elIndex = 0;
            foreach (var el in sequenceElements) {

                // This sequence element:
                //  if( chosenIndex == this index ) divert to this sequence element
                // duplicate chosen sequence index, since it'll be consumed by "=="
                container.AddContent (Runtime.ControlCommand.EvalStart ());
                container.AddContent (Runtime.ControlCommand.Duplicate ()); 
                container.AddContent (new Runtime.IntValue (elIndex));
                container.AddContent (Runtime.NativeFunctionCall.CallWithName ("=="));
                container.AddContent (Runtime.ControlCommand.EvalEnd ());

                // Divert branch for this sequence element
                var sequenceDivert = new Runtime.Divert ();
                sequenceDivert.isConditional = true;
                container.AddContent (sequenceDivert);

                // Generate content for this sequence element
                var contentContainerForSequenceBranch = (Runtime.Container) el.runtimeObject;
                contentContainerForSequenceBranch.name = "s" + elIndex;
                contentContainerForSequenceBranch.InsertContent (Runtime.ControlCommand.PopEvaluatedValue (), 0);

                // When sequence element is complete, divert back to end of sequence
                var seqBranchCompleteDivert = new Runtime.Divert ();
                contentContainerForSequenceBranch.AddContent (seqBranchCompleteDivert);
                container.AddToNamedContentOnly (contentContainerForSequenceBranch);

                // Save the diverts for reference resolution later (in ResolveReferences)
                AddDivertToResolve (sequenceDivert, contentContainerForSequenceBranch);
                AddDivertToResolve (seqBranchCompleteDivert, postSequenceNoOp);

                elIndex++;
            }

            // If all Once-only branches are done, then we need to pop the eval stack
            // for the visit index that went unused (normally popped in each branch).
            if( sequenceType == SequenceType.Once )
                container.AddContent (Runtime.ControlCommand.PopEvaluatedValue ());

            container.AddContent (postSequenceNoOp);

            return container;
        }

        void AddDivertToResolve(Runtime.Divert divert, Runtime.Object targetContent)
        {
            _sequenceDivertsToResove.Add( new SequenceDivertToResolve() { 
                divert = divert, 
                targetContent = targetContent
            });
        }

        public override void ResolveReferences(Story context)
        {
            base.ResolveReferences (context);

            foreach (var toResolve in _sequenceDivertsToResove) {
                toResolve.divert.targetPath = toResolve.targetContent.path;
            }
        }

        class SequenceDivertToResolve
        {
            public Runtime.Divert divert;
            public Runtime.Object targetContent;
        }
        List<SequenceDivertToResolve> _sequenceDivertsToResove;
    }
}

