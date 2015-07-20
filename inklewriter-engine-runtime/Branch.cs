using System.Text;
using Newtonsoft.Json;

namespace Inklewriter.Runtime
{
    internal class Branch : Runtime.Object
    {
        [JsonProperty("true")]
        public Divert trueDivert { 
            get {
                return _trueDivert;
            } 
            private set {
                SetChild (ref _trueDivert, value);
            } 
        }
        Divert _trueDivert;

        [JsonProperty("false")]
        public Divert falseDivert { 
            get {
                return _falseDivert;
            } 
            private set {
                SetChild (ref _falseDivert, value);
            } 
        }
        Divert _falseDivert;

        public Branch (Divert trueDivert = null, Divert falseDivert = null)
        {
            this.trueDivert = trueDivert;
            this.falseDivert = falseDivert;
        }

        // Default constructor for serialisation only
        public Branch()
        { }

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

