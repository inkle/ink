using System;

namespace inklecate2Sharp.Parsed
{
	public class Text : Parsed.Object
	{
		public string content { get; set; }

		public Text (string str)
		{
			content = str;
		}

		public override Runtime.Object GenerateRuntimeObject ()
		{
			return new Runtime.Text(this.content);
		}
	}
}

