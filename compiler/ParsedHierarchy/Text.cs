
namespace Ink.Parsed
{
	public class Text : Parsed.Object
	{
		public string text { get; set; }

		public Text (string str)
		{
			text = str;
		}

		public override Runtime.Object GenerateRuntimeObject ()
		{
			return new Runtime.StringValue(this.text);
		}

        public override string ToString ()
        {
            return this.text;
        }
	}
}

