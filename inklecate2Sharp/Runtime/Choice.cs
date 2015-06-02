using System;

namespace Inklewriter.Runtime
{
	public class Choice : Runtime.Object
	{
		public Path pathOnChoice { get; set; }
		public string choiceText { get; set; }
        public bool hasCondition { get; set; }

		public Choice (string choiceText)
		{
			this.choiceText = choiceText;
		}

        public override string ToString ()
        {
            return "Choice: '" + choiceText + "' -> " + pathOnChoice;
        }
	}
}

