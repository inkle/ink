namespace Ink.Runtime
{
    public class Text : Runtime.Object
	{
        // Immutable
        public string text { get; private set; }

        public bool isNewline { get; private set; }
        public bool isInlineWhitespace { get; private set; }
        public bool isNonWhitespace {
            get {
                return !isNewline && !isInlineWhitespace;
            }
        }

		public Text (string str)
		{
            text = str;

            // Classify whitespace status
            isNewline = text == "\n";
            isInlineWhitespace = true;
            foreach (var c in text) {
                if (c != ' ' && c != '\t') {
                    isInlineWhitespace = false;
                    break;
                }
            }
		}

        public override string ToString ()
		{
			return text;
		}
	}
}

