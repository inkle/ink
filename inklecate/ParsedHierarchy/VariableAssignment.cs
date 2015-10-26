using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    internal class VariableAssignment : Parsed.Object
    {
        public string variableName { get; protected set; }
        public Expression expression { get; protected set; }

        public bool isGlobalDeclaration { get; set; }
        public bool isNewTemporaryDeclaration { get; set; }

        public bool isDeclaration {
            get {
                return isGlobalDeclaration || isNewTemporaryDeclaration;
            }
        }

        public VariableAssignment (string variableName, Expression assignedExpression)
        {
            this.variableName = variableName;

            // Defensive programming in case parsing of assignedExpression failed
            if( assignedExpression )
                this.expression = AddContent(assignedExpression);
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            // Global declarations don't generate actual procedural
            // runtime objects, but instead add a global variable to the story itself
            if (isGlobalDeclaration) {
                story.TryAddNewVariableDeclaration (this);
                return null;
            } else if(isNewTemporaryDeclaration) {
                ClosestFlowBase ().TryAddNewTemporaryDeclaration (this);
            }

            var container = new Runtime.Container ();

            // The expression's runtimeObject is actually another nested container
            container.AddContent (expression.runtimeObject);

            container.AddContent (new Runtime.VariableAssignment (variableName, isNewTemporaryDeclaration));

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            Expression existingGlobalExpr = null;
            if (this.isNewTemporaryDeclaration && story.globalVariables.TryGetValue(variableName, out existingGlobalExpr) ) {
                Error ("global variable '"+variableName+"' already exists with the same name (declared on " + existingGlobalExpr.debugMetadata + ")");
                return;
            }

            if (IsReservedKeyword (variableName)) {
                Error ("cannot use '" + variableName + "' as a variable since it's a reserved ink keyword");
                return;
            }

            if (!this.isNewTemporaryDeclaration) {
                if (!context.ResolveVariableWithName (this.variableName, fromNode:this)) {
                    Error ("variable could not be found to assign to: '" + this.variableName + "'", this);
                }
            }
        }

        // TODO: Move this somewhere more general?
        bool IsReservedKeyword(string name)
        {
            return _reservedKeywords.Contains (name);
        }

        static HashSet<string> _reservedKeywords = new HashSet<string>(new string[] { 
            "true", "false",
            "on", "off",
            "yes", "no",
            "return",
            "else"
        });
    }
}

