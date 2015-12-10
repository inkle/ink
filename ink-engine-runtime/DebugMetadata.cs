using Newtonsoft.Json;

namespace Ink.Runtime
{
	public class DebugMetadata
	{
        [JsonProperty("s")]
		public int startLineNumber;

        [JsonProperty("e")]
        public int endLineNumber;

        [JsonProperty("f")]
		public string fileName;

        [JsonProperty("n")]
        public string sourceName;

		public DebugMetadata ()
		{
		}

        public override string ToString ()
        {
            if (fileName != null) {
                return string.Format ("line {0} of {1}", startLineNumber, fileName);
            } else {
                return "line " + startLineNumber;
            }

        }
	}
}

