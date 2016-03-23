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

        internal string pathStringOnChoice {
            get {
                return CompactPathString (pathOnChoice);
            }
            set {
                pathOnChoice = new Path (value);
            }
        }

        internal bool hasCondition { get; set; }
        internal bool hasStartContent { get; set; }
        internal bool hasChoiceOnlyContent { get; set; }
        internal bool onceOnly { get; set; }
        internal bool isInvisibleDefault { get; set; }

        internal int flags {
            get {
                int flags = 0;
                if (hasCondition)         flags |= 1;
                if (hasStartContent)      flags |= 2;
                if (hasChoiceOnlyContent) flags |= 4;
                if (isInvisibleDefault)   flags |= 8;
                if (onceOnly)             flags |= 16;
                return flags;
            }
            set {
                if ((value & 1) > 0)  hasCondition = true;
                if ((value & 2) > 0)  hasStartContent = true;
                if ((value & 4) > 0)  hasChoiceOnlyContent = true;
                if ((value & 8) > 0)  isInvisibleDefault = true;
                if ((value & 16) > 0) onceOnly = true;
            }
        }

        internal Choice (bool onceOnly)
		{
            this.onceOnly = onceOnly;
		}

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

