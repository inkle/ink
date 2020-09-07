using System;

namespace Ink.Runtime
{
    public class DebugMetadata
    {
        public int startLineNumber = 0;
        public int endLineNumber = 0;
        public int startCharacterNumber = 0;
        public int endCharacterNumber = 0;
        public string fileName = null;
        public string sourceName = null;

        public DebugMetadata ()
        {
        }

        // Currently only used in VariableReference in order to
        // merge the debug metadata of a Path.Of.Indentifiers into
        // one single range.
        public DebugMetadata Merge(DebugMetadata dm)
        {
            var newDebugMetadata = new DebugMetadata();

            // These are not supposed to be differ between 'this' and 'dm'.
            newDebugMetadata.fileName = fileName;
            newDebugMetadata.sourceName = sourceName;

            if (startLineNumber < dm.startLineNumber)
            {
                newDebugMetadata.startLineNumber = startLineNumber;
                newDebugMetadata.startCharacterNumber = startCharacterNumber;
            }
            else if (startLineNumber > dm.startLineNumber)
            {
                newDebugMetadata.startLineNumber = dm.startLineNumber;
                newDebugMetadata.startCharacterNumber = dm.startCharacterNumber;
            }
            else
            {
                newDebugMetadata.startLineNumber = startLineNumber;
                newDebugMetadata.startCharacterNumber = Math.Min(startCharacterNumber, dm.startCharacterNumber);
            }

            if (endLineNumber > dm.endLineNumber)
            {
                newDebugMetadata.endLineNumber = endLineNumber;
                newDebugMetadata.endCharacterNumber = endCharacterNumber;
            }
            else if (endLineNumber < dm.endLineNumber)
            {
                newDebugMetadata.endLineNumber = dm.endLineNumber;
                newDebugMetadata.endCharacterNumber = dm.endCharacterNumber;
            }
            else
            {
                newDebugMetadata.endLineNumber = endLineNumber;
                newDebugMetadata.endCharacterNumber = Math.Max(endCharacterNumber, dm.endCharacterNumber);
            }

            return newDebugMetadata;
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

