//using System.Collections.Generic;

namespace Ink.Parsed
{
    public class ConstantDeclaration : Parsed.Object
    {
        public string constantName { get; protected set; }
        public Expression expression { get; protected set; }

        public ConstantDeclaration (string name, Expression assignedExpression)
        {
            this.constantName = name;

            // Defensive programming in case parsing of assignedExpression failed
            if( assignedExpression )
                this.expression = AddContent(assignedExpression);
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            // Global declarations don't generate actual procedural
            // runtime objects, but instead add a global variable to the story itself.
            // The story then initialises them all in one go at the start of the game.
            return null;
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            context.CheckForNamingCollisions (this, constantName, Story.SymbolType.Var);
        }

        public override string typeName {
            get {
                return "Constant";
            }
        }
            
    }
}

