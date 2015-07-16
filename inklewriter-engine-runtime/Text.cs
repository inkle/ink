
namespace Inklewriter.Runtime
{
    public class Text : Runtime.Object
	{
		public string text { get; set; }

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

