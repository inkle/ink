using System;

namespace Inklewriter.Runtime
{
    public class Branch : Runtime.Object
    {
        public Divert trueDivert { get; }
        public Divert falseDivert { get; }

        public Branch (Divert trueDivert, Divert falseDivert = null)
        {
            this.trueDivert = trueDivert;
            this.falseDivert = falseDivert;
        }
    }
}

