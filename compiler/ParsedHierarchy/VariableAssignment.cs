using System.Collections.Generic;

namespace Ink.Parsed
{
    public class VariableAssignment : Parsed.Object
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

            if (listDef) {
                this.listDefinition = AddContent (listDef);
                this.listDefinition.variableAssignment = this;
            }

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

            _runtimeAssignment = new Runtime.VariableAssignment(variableName, isNewTemporaryDeclaration);
            container.AddContent (_runtimeAssignment);

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            // List definitions are checked for conflicts separately
            if( this.isDeclaration && listDefinition == null )
                context.CheckForNamingCollisions (this, variableName, this.isGlobalDeclaration ? Story.SymbolType.Var : Story.SymbolType.Temp);

            // Initial VAR x = [intialValue] declaration, not re-assignment
            if (this.isGlobalDeclaration) {
                var variableReference = expression as VariableReference;
                if (variableReference && !variableReference.isConstantReference && !variableReference.isListItemReference) {
                    Error ("global variable assignments cannot refer to other variables, only literal values, constants and list items");
                }       
            }

            if (!this.isNewTemporaryDeclaration) {
                var resolvedVarAssignment = context.ResolveVariableWithName(this.variableName, fromNode: this);
                if (!resolvedVarAssignment.found) {
                    if (story.constants.ContainsKey (variableName)) {
                        Error ("Can't re-assign to a constant (do you need to use VAR when declaring '" + this.variableName + "'?)", this);
                    } else {
                        Error ("Variable could not be found to assign to: '" + this.variableName + "'", this);
                    }
                }

                // A runtime assignment may not have been generated if it's the initial global declaration,
                // since these are hoisted out and handled specially in Story.ExportRuntime.
                if( _runtimeAssignment != null ) 
                    _runtimeAssignment.isGlobal = resolvedVarAssignment.isGlobal;
            }
        }


        public override string typeName {
            get {
                if (isNewTemporaryDeclaration) return "temp";
                else if (isGlobalDeclaration) return "VAR";
                else return "variable assignment";
            }
        }

        Runtime.VariableAssignment _runtimeAssignment;
    }
}

