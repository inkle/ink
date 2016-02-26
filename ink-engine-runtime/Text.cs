using Newtonsoft.Json;

namespace Ink.Runtime
{
    public class Text : Runtime.Object
	{
		public string text { get; set; }

        public bool isNewline {
            get {
                return text == "\n";
            }
        }

        // TODO: Cache this?
        public bool isSpaces {
            get {
                foreach (var c in text) {
                    if (c != ' ' && c != '\t')
                        return false;
                }
                return true;
            }
        }

        public bool isNonWhitespace {
            get {
                return !isNewline && !isSpaces;
            }
        }

		public Text (string str)
		{
			text = str;
		}

        // Require default constructor for serialisation
        public Text() : this(null) {}

        public override string ToString ()
		{
			return text;
		}
	}
}

