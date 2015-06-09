using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class Sequence : Parsed.Object
    {
        public List<List<Parsed.Object>> sequenceElements;

        public Sequence (List<List<Parsed.Object>> sequenceElements)
        {
            foreach (var sequenceContentList in sequenceElements) {
                AddContent (sequenceContentList);
            }
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            return new Runtime.Text ("Hello world");
        }
    }
}

