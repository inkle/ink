using System;
using System.Collections.Generic;

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

            Parsed.FlowBase foundFlowForReadCount = null;

            if (context.ResolveVariableWithName (this.name, 
                out foundFlowForReadCount, 
                fromNode:this, 
                allowReadCounts:true, 
                reportErrors:true)) {

                // Resolving a read count of a piece of flow?
                // Update to use the full name of that flow rather than the local name
                if (foundFlowForReadCount != null) {
                    _runtimeVarRef.name = this.name = foundFlowForReadCount.dotSeparatedFullName;
                }
            }
        }

        Runtime.VariableReference _runtimeVarRef;
    }
}

