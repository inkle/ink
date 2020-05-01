using System.Text;

namespace Ink.Runtime
{
	public class Divert : Runtime.Object
	{
        public Path targetPath { 
            get { 
                // Resolve any relative paths to global ones as we come across them
                if (_targetPath != null && _targetPath.isRelative) {
                    var targetObj = targetPointer.Resolve();
                    if (targetObj) {
                        _targetPath = targetObj.path;
                    }
                }
                return _targetPath;
            }
            set {
                _targetPath = value;
                _targetPointer = Pointer.Null;
            } 
        }
        Path _targetPath;

        public Pointer targetPointer {
            get {
                if (_targetPointer.isNull) {
                    var targetObj = ResolvePath (_targetPath).obj;

                    if (_targetPath.lastComponent.isIndex) {
                        _targetPointer.container = targetObj.parent as Container;
                        _targetPointer.index = _targetPath.lastComponent.index;
                    } else {
                        _targetPointer = Pointer.StartOf (targetObj as Container);
                    }
                }
                return _targetPointer;
            }
        }
        Pointer _targetPointer;
        

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
            
        public string variableDivertName { get; set; }
        public bool hasVariableTarget { get { return variableDivertName != null; } }

        public bool pushesToStack { get; set; }
        public PushPopType stackPushType;

        public bool isExternal { get; set; }
        public int externalArgs { get; set; }

        public bool isConditional { get; set; }

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

                if (isConditional)
                    sb.Append ("?");

                if (pushesToStack) {
                    if (stackPushType == PushPopType.Function) {
                        sb.Append (" function");
                    } else {
                        sb.Append (" tunnel");
                    }
                }

                sb.Append (" -> ");
                sb.Append (targetPathString);

                sb.Append (" (");
                sb.Append (targetStr);
                sb.Append (")");

                return sb.ToString ();
            }
        }
	}
}

