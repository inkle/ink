using System;

namespace Ink.Runtime
{
    // New version of tags is dynamic - it constructs the tags
    // at runtime based on BeginTag and EndTag control commands.
    // Plain text that's in the output stream is turned into tags
    // when you do story.currentTags.
    public class LegacyTag : Runtime.Object
    {
        public string text { get; private set; }

        public LegacyTag (string tagText)
        {
            this.text = tagText;
        }

        public override string ToString ()
        {
            return "# " + text;
        }
    }
}

