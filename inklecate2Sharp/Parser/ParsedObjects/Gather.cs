using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class Gather : Parsed.Object, IWeavePoint, INamedContent
    { 
        public string name { get; set; }
        public int indentationDepth { get; protected set; }

        public Runtime.Container runtimeContainer { get { return (Runtime.Container) runtimeObject; } }

        public Gather (string name, List<Parsed.Object> content, int indentationDepth)
        {
            this.name = name;
            this.indentationDepth = indentationDepth;

            AddContent (content);
        }
            
        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();
            container.name = name;

            // A gather can have null content, e.g. it's just purely a line with "-"
            if (content != null) {
                foreach (var c in content) {
                    container.AddContent (c.runtimeObject);
                }
            }

            return container;

        }

        public override void ResolveReferences (Story context)
        {
            // A gather can have null content, e.g. it's just purely a line with "-"
            if (content == null)
                return;
                
            foreach (var obj in content) {
                obj.ResolveReferences (context);
            }
        }

    }
}

