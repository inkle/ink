
namespace Inklewriter.Runtime
{
	public class Divert : Runtime.Object
	{
		public Path targetPath { get; set; }

        public string variableDivertName { get; set; }
        public bool hasVariableTarget { get { return variableDivertName != null; } }

		public Divert ()
		{
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

