using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class ContentList : Parsed.Object
    {
        public ContentList (List<Parsed.Object> objects)
        {
            AddContent (objects);
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();
            foreach (var obj in content) {
                container.AddContent (obj.runtimeObject);
            }
            return container;
        }
    }
}

