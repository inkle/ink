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
            int? targetLineNum = DebugLineNumberOfPath (pathOnChoice);
            string targetString = pathOnChoice.ToString ();

            if (targetLineNum != null) {
                targetString = " line " + targetLineNum;
            } 

            return "Choice: '" + choiceText + "' -> " + targetString;
        }
	}
}

