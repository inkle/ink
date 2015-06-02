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
            assignedExpression.parent = this;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // The expression's runtimeObject is actually another nested container
            container.AddContent (expression.runtimeObject);

            container.AddContent (new Runtime.VariableAssignment (variableName, isNewDeclaration));

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            expression.ResolveReferences (context);
        }
    }
}

