
namespace Inklewriter.Runtime
{
	public class Choice : Runtime.Object
	{
		public Path pathOnChoice { get; set; }
		public string choiceText { get; set; }
        public bool hasCondition { get; set; }
        public bool onceOnly { get; set; }
        public bool isInvisibleDefault { get; set; }

        public Choice (string choiceText, bool onceOnly)
		{
			this.choiceText = choiceText;
            this.onceOnly = onceOnly;
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

