using System.ComponentModel;

namespace Ink.Runtime
{
    // The value to be assigned is popped off the evaluation stack, so no need to keep it here
    internal class VariableAssignment : Runtime.Object
    {
        public string variableName { get; protected set; }
        public bool isNewDeclaration { get; protected set; }
        public bool isGlobal { get; set; }

        public VariableAssignment (string variableName, bool isNewDeclaration)
        {
            this.variableName = variableName;
            this.isNewDeclaration = isNewDeclaration;
        }

        // Require default constructor for serialisation
        public VariableAssignment() : this(null, false) {}

        public override string ToString ()
        {
            return "VarAssign to " + variableName;
        }
    }
}

