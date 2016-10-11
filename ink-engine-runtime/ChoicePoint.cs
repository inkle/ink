using System.ComponentModel;

namespace Ink.Runtime
{
    /// <summary>
    /// The ChoicePoint represents the point within the Story where
    /// a Choice instance gets generated. The distinction is made
    /// because the text of the Choice can be dynamically generated.
    /// </summary>
	internal class ChoicePoint : Runtime.Object
	{
        internal Path pathOnChoice {
            get {
                // Resolve any relative paths to global ones as we come across them
                if (_pathOnChoice != null && _pathOnChoice.isRelative) {
                    var choiceTargetObj = choiceTarget;
                    if (choiceTargetObj) {
                        _pathOnChoice = choiceTargetObj.path;
                    }
                }
                return _pathOnChoice;
            }
            set {
                _pathOnChoice = value;
            }
        }
        Path _pathOnChoice;

        internal Container choiceTarget {
            get {
                return this.ResolvePath (_pathOnChoice) as Container;
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
                hasCondition = (value & 1) > 0;
                hasStartContent = (value & 2) > 0;
                hasChoiceOnlyContent = (value & 4) > 0;
                isInvisibleDefault = (value & 8) > 0;
                onceOnly = (value & 16) > 0;
            }
        }

        internal ChoicePoint (bool onceOnly)
		{
            this.onceOnly = onceOnly;
		}

        public ChoicePoint() : this(true) {}

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

