
namespace Inklewriter.Parsed
{
    public class VariableReference : Expression
    {
        public string name { get; protected set; }

        public VariableReference (string name)
        {
            this.name = name;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            _runtimeVarRef = new Runtime.VariableReference (name);
            container.AddContent(_runtimeVarRef);
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);


            if (!context.ResolveVariableWithName (this.name, fromNode: this)) {
               
                // No variables with the given name. Try a read count.
                var objForReadCount = context.ResolveTargetForReadCountWithName (this.name, fromNode: this);
                if (objForReadCount != null) {
                    _runtimeVarRef.pathForVisitCount = objForReadCount.runtimePath;
                }

                else {
                    Error("Unresolved variable: "+this.name+" after searching: "+this.DescriptionOfScope (), this);
                }

            }
        }

        public override string ToString ()
        {
            return name;
        }

        Runtime.VariableReference _runtimeVarRef;
    }
}

