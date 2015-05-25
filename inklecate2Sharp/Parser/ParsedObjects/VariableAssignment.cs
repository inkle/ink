using System;

namespace Inklewriter.Parsed
{
    public class VariableAssignment : Parsed.Object
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

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var runtimeExpr   = (Runtime.Expression)expression.runtimeObject;
            var runtimeVarAss = new Runtime.VariableAssignment (variableName, runtimeExpr, isNewDeclaration);
            return runtimeVarAss;
        }
    }
}

