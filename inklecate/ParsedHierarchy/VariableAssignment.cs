using System.Collections.Generic;

namespace Ink.Parsed
{
    internal class VariableAssignment : Parsed.Object
    {
        public string variableName { get; protected set; }
        public Expression expression { get; protected set; }
        public ListDefinition listDefinition { get; protected set; }

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

        public VariableAssignment (string variableName, ListDefinition listDef)
        {
            this.variableName = variableName;

            if( listDef )
                this.listDefinition = AddContent(listDef);

            // List definitions are always global
            isGlobalDeclaration = true;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            FlowBase newDeclScope = null;
            if (isGlobalDeclaration) {
                newDeclScope = story;
            } else if(isNewTemporaryDeclaration) {
                newDeclScope = ClosestFlowBase ();
            }

            if( newDeclScope )
                newDeclScope.TryAddNewVariableDeclaration (this);

            // Global declarations don't generate actual procedural
            // runtime objects, but instead add a global variable to the story itself.
            // The story then initialises them all in one go at the start of the game.
            if( isGlobalDeclaration )
                return null;

            var container = new Runtime.Container ();

            // The expression's runtimeObject is actually another nested container
            if( expression != null )
                container.AddContent (expression.runtimeObject);
            else if( listDefinition != null )
                container.AddContent (listDefinition.runtimeObject);

            container.AddContent (new Runtime.VariableAssignment (variableName, isNewTemporaryDeclaration));

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            VariableAssignment varDecl = null;
            if (this.isNewTemporaryDeclaration && story.variableDeclarations.TryGetValue(variableName, out varDecl) ) {
                if (varDecl.isGlobalDeclaration) {
                    Error ("global variable '" + variableName + "' already exists with the same name (declared on " + varDecl.debugMetadata + ")");
                    return;
                }
            }

            if (this.isGlobalDeclaration) {
                var variableReference = expression as VariableReference;
                if (variableReference && !variableReference.isConstantReference && !variableReference.isListItemReference) {
                    Error ("global variable assignments cannot refer to other variables, only literal values, constants and list items");
                }       
            }

            if (IsReservedKeyword (variableName)) {
                Error ("cannot use '" + variableName + "' as a variable since it's a reserved ink keyword");
                return;
            }

            if (!this.isNewTemporaryDeclaration) {
                if (!context.ResolveVariableWithName (this.variableName, fromNode:this).found) {
                    if (story.constants.ContainsKey (variableName)) {
                        Error ("Can't re-assign to a constant (do you need to use VAR when declaring '" + this.variableName + "'?)", this);
                    } else {
                        Error ("Variable could not be found to assign to: '" + this.variableName + "'", this);
                    }

                }
            }
        }

        // TODO: Move this somewhere more general?
        public static bool IsReservedKeyword(string name)
        {
            return _reservedKeywords.Contains (name);
        }

        static HashSet<string> _reservedKeywords = new HashSet<string>(new string[] { 
            "true", "false",
            "not",
            "return",
            "else"
        });
    }
}

