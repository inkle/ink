using Newtonsoft.Json;
using System.ComponentModel;

namespace Ink.Runtime
{
	public class Choice : Runtime.Object
	{
        [JsonProperty("path")]
		internal Path pathOnChoice { get; set; }

        [JsonProperty("hasCond")]
        internal bool hasCondition { get; set; }

        [JsonProperty("once")]
        [DefaultValue(true)]
        internal bool onceOnly { get; set; }

        [JsonProperty("default")]
        internal bool isInvisibleDefault { get; set; }

        internal Choice (bool onceOnly)
		{
            this.onceOnly = onceOnly;
		}

        // Require default constructor for serialisation
        public Choice() : this(true) {}

        public override string ToString ()
        {
            int? targetLineNum = DebugLineNumberOfPath (pathOnChoice);
            string targetString = pathOnChoice.ToString ();

            if (targetLineNum != null) {
                targetString = " line " + targetLineNum;
            } 

            return "Choice: -> " + targetString;
        }
	}
}

