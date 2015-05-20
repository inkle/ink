using System;

namespace inklecate2Sharp.Runtime
{
	public class Choice : Runtime.Object
	{
		public Path pathOnChoice { get; set; }
		public string choiceText { get; set; }

		public Choice (string choiceText)
		{
			this.choiceText = choiceText;
		}
	}
}

