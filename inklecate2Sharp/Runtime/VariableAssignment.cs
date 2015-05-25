using System;

namespace Inklewriter.Runtime
{
    public class VariableAssignment : Runtime.Object
    {
        public string variableName { get; protected set; }
        public Expression expression { get; protected set; }
        public bool isNewDeclaration { get; protected set; }

        public VariableAssignment (string variableName, Expression assignedExpression, bool isNewDeclaration)
        {
            this.variableName = variableName;
            this.expression = assignedExpression;
            this.isNewDeclaration = isNewDeclaration;
        }
    }
}

