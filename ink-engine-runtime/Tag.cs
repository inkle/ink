using System;

namespace Ink.Runtime
{
    public class Tag : Runtime.Object
    {
        public string text { get; private set; }

        public Tag (string tagText)
        {
            this.text = tagText;
        }

        public override string ToString ()
        {
            return "# " + text;
        }
    }
}

