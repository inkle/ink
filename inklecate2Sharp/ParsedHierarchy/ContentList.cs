using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class ContentList : Parsed.Object
    {
        public Runtime.Container runtimeContainer {
            get {
                return (Runtime.Container) this.runtimeObject;
            }
        }

        public ContentList (List<Parsed.Object> objects)
        {
            AddContent (objects);
        }

        public ContentList()
        {
        }

        public void TrimTrailingWhitespace()
        {
            for (int i = this.content.Count - 1; i >= 0; --i) {
                var text = this.content [i] as Text;
                if (text == null)
                    break;

                var trimmedText = text.text.TrimEnd (' ', '\t');
                if (trimmedText.Length == 0)
                    this.content.RemoveAt (i);
                else
                    break;
            }
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();
            if (content != null) {
                foreach (var obj in content) {
                    container.AddContent (obj.runtimeObject);
                }
            }
            return container;
        }
    }
}

