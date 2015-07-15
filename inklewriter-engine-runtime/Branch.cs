using System.Text;

namespace Inklewriter.Runtime
{
    public class Branch : Runtime.Object
    {
		public Divert trueDivert { get; private set; }
		public Divert falseDivert { get; private set; }

        public Branch (Divert trueDivert = null, Divert falseDivert = null)
        {
            this.trueDivert = trueDivert;
            this.falseDivert = falseDivert;

            if (trueDivert) {
                trueDivert.parent = this;
            }
            if (falseDivert) {
                falseDivert.parent = this;
            }
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            sb.Append ("Branch: ");
            if (trueDivert) {
                sb.AppendFormat ("(true: {0})", trueDivert);
            }
            if (falseDivert) {
                sb.AppendFormat ("(false: {0})", falseDivert);
            }
            return sb.ToString ();
        }
    }
}

