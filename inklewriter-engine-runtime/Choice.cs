using Newtonsoft.Json;
using System.ComponentModel;

namespace Inklewriter.Runtime
{
	public class Choice : Runtime.Object
	{
        [JsonProperty("path")]
		public Path pathOnChoice { get; set; }

        [JsonProperty("txt")]
		public string choiceText { get; set; }

        [JsonProperty("hasCond")]
        public bool hasCondition { get; set; }

        [JsonProperty("once")]
        [DefaultValue(true)]
        public bool onceOnly { get; set; }

        [JsonProperty("default")]
        public bool isInvisibleDefault { get; set; }

        public Choice (string choiceText, bool onceOnly)
		{
			this.choiceText = choiceText;
            this.onceOnly = onceOnly;
		}

        // Require default constructor for serialisation
        public Choice() : this(null, true) {}

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

