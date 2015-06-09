using System;
using System.Text;

namespace Inklewriter.Runtime
{
    public class Branch : Runtime.Object
    {
        public Divert trueDivert { get; }
        public Divert falseDivert { get; }

        public Branch (Divert trueDivert = null, Divert falseDivert = null)
        {
            this.trueDivert = trueDivert;
            this.falseDivert = falseDivert;
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            sb.Append ("Branch: ");
            if (trueDivert != null) {
                sb.AppendFormat ("(true: {0})", trueDivert);
            }
            if (falseDivert != null) {
                sb.AppendFormat ("(false: {0})", falseDivert);
            }
            return sb.ToString ();
        }
    }
}

