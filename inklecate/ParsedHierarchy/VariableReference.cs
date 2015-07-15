using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class VariableReference : Expression
    {
        public string name { 
            get {
                if (path != null && path.Count == 1)
                    return path [0];
                else
                    return null;
            } 
        }
        
        public List<string> path;

        public VariableReference (List<string> path)
        {
            this.path = path;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            _runtimeVarRef = new Runtime.VariableReference (name);
            container.AddContent(_runtimeVarRef);
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            // Is it a read count?
            var parsedPath = new Path (path);
            var targetForReadCount = parsedPath.ResolveFromContext (this);
            if (targetForReadCount) {
                _runtimeVarRef.pathForVisitCount = targetForReadCount.runtimePath;
                return;
            }

            if (!context.ResolveVariableWithName (this.name, fromNode: this)) {

                var forcedResolveObject = parsedPath.ResolveFromContext (this, forceSearchAnywhere:true);
                if (forcedResolveObject) {
                    var suggestedDotSepReadPath = forcedResolveObject.PathRelativeTo (this).dotSeparatedComponents;
                    Error ("Unresolved variable: " + this.ToString () + ". Did you mean '" + suggestedDotSepReadPath + "' (as a read count)?");
                } else {
                    Error("Unresolved variable: "+this.ToString()+" after searching: "+this.descriptionOfScope, this);
                }


            }
        }

        public override string ToString ()
        {
            return string.Join(".", path.ToArray());
        }

        Runtime.VariableReference _runtimeVarRef;
    }
}

