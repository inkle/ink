
namespace Ink.Parsed
{
    public class Gather : Parsed.Object, IWeavePoint, INamedContent
    { 
        public string name { get; set; }
        public int indentationDepth { get; protected set; }

        public Runtime.Container runtimeContainer { get { return (Runtime.Container) runtimeObject; } }

        public Gather (string name, int indentationDepth)
        {
            this.name = name;
            this.indentationDepth = indentationDepth;
        }
            
        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();
            container.name = name;

            if (this.story.countAllVisits) {
                container.visitsShouldBeCounted = true;
            }

            container.countingAtStartOnly = true;

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
            base.ResolveReferences (context);

            if( name != null && name.Length > 0 )
                context.CheckForNamingCollisions (this, name, Story.SymbolType.SubFlowAndWeave);
        }
    }
}

