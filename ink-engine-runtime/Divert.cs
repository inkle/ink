using Newtonsoft.Json;

namespace Ink.Runtime
{
	internal class Divert : Runtime.Object
	{
		public Path targetPath { get; set; }

        [JsonProperty("div")]
        [UniqueJsonIdentifier]
        public string targetPathString {
            get {
                if (targetPath == null)
                    return null;
                
                return targetPath.componentsString;
            }
            set {
                if (value == null)
                    targetPath = null;
                else 
                    targetPath = new Path (value);
            }
        }

        [JsonProperty("vdiv")]
        [UniqueJsonIdentifier]
        public string variableDivertName { get; set; }
        public bool hasVariableTarget { get { return variableDivertName != null; } }

		public Divert ()
		{
		}

        public override bool Equals (object obj)
        {
            var otherDivert = obj as Divert;
            if (otherDivert) {
                if (this.hasVariableTarget == otherDivert.hasVariableTarget) {
                    if (this.hasVariableTarget) {
                        return this.variableDivertName == otherDivert.variableDivertName;
                    } else {
                        return this.targetPath.Equals(otherDivert.targetPath);
                    }
                }
            }
            return false;
        }

        public override int GetHashCode ()
        {
            if (hasVariableTarget) {
                const int variableTargetSalt = 12345;
                return variableDivertName.GetHashCode() + variableTargetSalt;
            } else {
                const int pathTargetSalt = 54321;
                return targetPath.GetHashCode() + pathTargetSalt;
            }
        }

        public override string ToString ()
        {
            if (hasVariableTarget) {
                return "Divert(variable: " + variableDivertName + ")";
            }
            else if (targetPath == null) {
                return "Divert(null)";
            } else {
                string targetStr = targetPath.ToString ();
                int? targetLineNum = DebugLineNumberOfPath (targetPath);
                if (targetLineNum != null) {
                    targetStr = " line " + targetLineNum;
                }

                return "Divert(" + targetStr + ")";
            }
        }
	}
}

