using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    internal class VariableAssignment : Parsed.Object
    {
        public string variableName { get; protected set; }
        public Expression expression { get; protected set; }

        public bool isGlobalDeclaration { get; set; }
        public bool isNewProceduralDeclaration { get; set; }

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
            }

            var container = new Runtime.Container ();

            // The expression's runtimeObject is actually another nested container
            container.AddContent (expression.runtimeObject);

            container.AddContent (new Runtime.VariableAssignment (variableName, isNewProceduralDeclaration));

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            if (!this.isNewProceduralDeclaration) {
                if (!context.ResolveVariableWithName (this.variableName, fromNode:this)) {
                    Error ("variable could not be found to assign to: '" + this.variableName + "'", this);
                }
            }

            if (IsReservedKeyword (variableName)) {
                Error ("cannot use '" + variableName + "' as a variable since it's a reserved ink keyword");
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

