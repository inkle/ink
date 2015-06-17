
namespace Inklewriter.Runtime
{
    public class Text : Runtime.Object
	{
		public string text { get; set; }

		public Text (string str)
		{
			text = str;
		}

		public override string ToString ()
		{
			return text;
		}
	}
}

