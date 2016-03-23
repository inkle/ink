using System.Text;

namespace Ink.Runtime
{
    internal class Branch : Runtime.Object
    {
        public Divert trueDivert { 
            get {
                return _trueDivert;
            } 
            set {
                SetChild (ref _trueDivert, value);
            } 
        }
        Divert _trueDivert;

        public Divert falseDivert { 
            get {
                return _falseDivert;
            } 
            set {
                SetChild (ref _falseDivert, value);
            } 
        }
        Divert _falseDivert;

        public Branch (Divert trueDivert = null, Divert falseDivert = null)
        {
            this.trueDivert = trueDivert;
            this.falseDivert = falseDivert;
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

