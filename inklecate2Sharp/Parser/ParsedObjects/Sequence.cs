using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public enum SequenceType
    {
        Stopping, // default
        Cycle,
        Shuffle,
        Once
    }

    public class Sequence : Parsed.Object
    {

        public List<ContentList> sequenceElements;
        public SequenceType sequenceType;

        public Sequence (List<ContentList> sequenceElements, SequenceType sequenceType)
        {
            this.sequenceType = sequenceType;
            this.sequenceElements = sequenceElements;

            foreach (var sequenceContentList in sequenceElements) {
                AddContent (sequenceContentList);
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

            _sequenceDivertsToResove = new List<SequenceDivertToResolve> ();

            // Get sequence read count
            container.AddContent (Runtime.ControlCommand.EvalStart ());
            container.AddContent (Runtime.ControlCommand.SequenceCount ());

            // Chosen sequence index:
            //  - Stopping: take the MIN(read count, num elements - 1)
            if (sequenceType == SequenceType.Stopping) {
                container.AddContent (new Runtime.LiteralInt (sequenceElements.Count - 1));
                container.AddContent (Runtime.NativeFunctionCall.CallWithName ("MIN"));
            } 

            // - Cycle: take (read count % num elements)
            else if (sequenceType == SequenceType.Cycle) {
                container.AddContent (new Runtime.LiteralInt (sequenceElements.Count));
                container.AddContent (Runtime.NativeFunctionCall.CallWithName ("%"));
            }

            // Once: allow sequence count to be unbounded
            else if (sequenceType == SequenceType.Once) {
                // Do nothing - the sequence count will simply prevent
                // any content being referenced when it goes out of bounds
            }

            // Not implemented
            else {
                throw new System.NotImplementedException ();
            }

            container.AddContent (Runtime.ControlCommand.EvalEnd ());

            // Will append the increment at the end, we're just generating it for now,
            // since we'll use it as a divert point to return to.
            var postSequenceIncrement = Runtime.ControlCommand.SequenceIncrement ();

            var elIndex = 0;
            foreach (var el in sequenceElements) {

                // This sequence element:
                //  if( chosenIndex == this index ) divert to this sequence element
                // duplicate chosen sequence index, since it'll be consumed by "=="
                container.AddContent (Runtime.ControlCommand.EvalStart ());
                container.AddContent (Runtime.ControlCommand.Duplicate ()); 
                container.AddContent (new Runtime.LiteralInt (elIndex));
                container.AddContent (Runtime.NativeFunctionCall.CallWithName ("=="));
                container.AddContent (Runtime.ControlCommand.EvalEnd ());

                // Divert branch for this sequence element
                var sequenceDivert = new Runtime.Divert ();
                container.AddContent (new Runtime.Branch (sequenceDivert));

                // Generate content for this sequence element
                var contentContainerForSequenceBranch = (Runtime.Container) el.runtimeObject;
                contentContainerForSequenceBranch.name = "s" + elIndex;

                // When sequence element is complete, divert back to end of sequence
                var seqBranchCompleteDivert = new Runtime.Divert ();
                contentContainerForSequenceBranch.AddContent (seqBranchCompleteDivert);
                container.AddToNamedContentOnly (contentContainerForSequenceBranch);

                // Save the diverts for reference resolution later (in ResolveReferences)
                AddDivertToResolve (sequenceDivert, contentContainerForSequenceBranch);
                AddDivertToResolve (seqBranchCompleteDivert, postSequenceIncrement);

                elIndex++;
            }

            container.AddContent (postSequenceIncrement);

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

