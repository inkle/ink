using Newtonsoft.Json;
using System.ComponentModel;

namespace Ink.Runtime
{
	internal class Choice : Runtime.Object
	{
        internal Path pathOnChoice { get; set; }

        internal Container choiceTarget {
            get {
                return this.ResolvePath (pathOnChoice) as Container;
            }
        }

        [JsonProperty("path")]
        internal string pathStringOnChoice {
            get {
                return CompactPathString (pathOnChoice);
            }
            private set {
                pathOnChoice = new Path (value);
            }
        }

        [JsonProperty("cond")]
        internal bool hasCondition { get; set; }

        [JsonProperty("_[")]
        internal bool hasStartContent { get; set; }

        [JsonProperty("[_]")]
        internal bool hasChoiceOnlyContent { get; set; }

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

