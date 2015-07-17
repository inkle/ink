using System.Text;
using Newtonsoft.Json;

namespace Inklewriter.Runtime
{
    public class Branch : Runtime.Object
    {
        [JsonProperty("true")]
		public Divert trueDivert { get; private set; }

        [JsonProperty("false")]
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

