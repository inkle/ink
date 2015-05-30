using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class Gather : Parsed.Object, IWeavePoint
    { 
        public string name { get; protected set; }
        public List<Parsed.Object> content;
        public int indentationDepth { get; protected set; }
        public bool hasLooseEnd { get { return true; } }

        public Gather (string name, List<Parsed.Object> content, int indentationDepth)
        {
            this.name = name;
            this.content = content;
            this.indentationDepth = indentationDepth;
        }

        public void AddNestedContent(Parsed.Object obj)
        {
            content.Add (obj);
            obj.parent = this;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            foreach (var c in content) {
                container.AddContent (c.runtimeObject);
            }

            return container;

        }

        public override void ResolveReferences (Story context)
        {
            foreach (var obj in content) {
                obj.ResolveReferences (context);
            }
        }

    }
}

