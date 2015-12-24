using Newtonsoft.Json;
using System.Text;

namespace Ink.Runtime
{
	internal class Divert : Runtime.Object
	{
        public Path targetPath { 
            get { 
                // Resolve any relative paths to global ones as we come across them
                if (_targetPath != null && _targetPath.isRelative) {
                    var targetObj = ResolvePath (_targetPath);
                    if (targetObj) {
                        _targetPath = targetObj.path;
                    }
                }
                return _targetPath;
            }
            set {
                _targetPath = value;
            } 
        }
        Path _targetPath;

        [JsonProperty("div")]
        [UniqueJsonIdentifier]
        public string targetPathString {
            get {
                if (targetPath == null)
                    return null;

                return CompactPathString (targetPath);
            }
            set {
                if (value == null) {
                    targetPath = null;
                } else {
                    targetPath = new Path (value);
                }
            }
        }

        [JsonProperty("vdiv")]
        [UniqueJsonIdentifier]
        public string variableDivertName { get; set; }
        public bool hasVariableTarget { get { return variableDivertName != null; } }

        public bool pushesToStack { get; set; }
        public PushPopType stackPushType;

        [JsonProperty("push")]
        public string pushTypeString {
            get {
                if (!pushesToStack)
                    return null;
                return Pop.SerialisationName (stackPushType);
            }
            set {
                if (value != null && value.Length > 0) {
                    stackPushType = Pop.SerialisedTypeFromName (value);
                    pushesToStack = true;
                } else {
                    pushesToStack = false;
                }
            }
        }

        [JsonProperty("exArgs")]
        public bool isExternal { get; set; }

        [JsonProperty("exArgs")]
        public int externalArgs { get; set; }

		public Divert ()
		{
            pushesToStack = false;
		}

        public Divert(PushPopType stackPushType)
        {
            pushesToStack = true;
            this.stackPushType = stackPushType;
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

                var sb = new StringBuilder ();

                string targetStr = targetPath.ToString ();
                int? targetLineNum = DebugLineNumberOfPath (targetPath);
                if (targetLineNum != null) {
                    targetStr = "line " + targetLineNum;
                }

                sb.Append ("Divert");
                if (pushesToStack) {
                    if (stackPushType == PushPopType.Function) {
                        sb.Append (" function");
                    } else {
                        sb.Append (" tunnel");
                    }
                }

                sb.Append (" (");
                sb.Append (targetStr);
                sb.Append (")");

                return sb.ToString ();
            }
        }
	}
}

