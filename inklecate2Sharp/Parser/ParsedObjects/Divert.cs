using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	public class Divert : Parsed.Object
	{
		public Parsed.Path target { get; protected set; }
        public Parsed.Object targetContent { get; protected set; }
        public List<Expression> arguments { get; protected set; }
		public Runtime.Divert runtimeDivert { get; protected set; }

        public Divert (Parsed.Path target, List<Expression> arguments = null)
		{
			this.target = target;

            if (arguments != null) {
                foreach (var expr in arguments) {
                    expr.parent = this;
                }
            }
            this.arguments = arguments;
		}

        public Divert (Parsed.Object targetContent)
        {
            this.targetContent = targetContent;
        }

		public override Runtime.Object GenerateRuntimeObject ()
		{
            runtimeDivert = new Runtime.Divert ();

            // Passing arguments to the knot
            if (arguments != null && arguments.Count > 0) {

                var container = new Runtime.Container ();

                var expressionsRuntimeContainer = Expression.GenerateRuntimeExpressions (arguments.ToArray ());

                container.AddContent (expressionsRuntimeContainer);

                // Jump into the "function" (knot/stitch)
                container.AddContent (runtimeDivert);

                return container;
            } 

            // Simple divert
            else {
                return runtimeDivert;
            }
			
		}

        public override void ResolveReferences(Story context)
		{
            if (targetContent == null) {
                targetContent = target.ResolveFromContext (this);

                if (targetContent == null) {

                    bool foundAlternative = false;
                    Path alternativePath = target.debugSuggestedAlternative;
                    if (alternativePath != null) {
                        targetContent = alternativePath.ResolveFromContext (this);
                        if (targetContent != null) {
                            foundAlternative = true;
                        }
                    }

                    if (foundAlternative) {
                        Error ("Divert: target not found: '" + target.ToString () + "'. Did you mean '"+alternativePath+"'?");
                        target = alternativePath;
                    } else {
                        Error ("Divert: target not found: '" + target.ToString () + "'");
                    }
                }

                // Argument passing: Check for errors in number of arguments
                if (targetContent != null) {

                    var numArgs = 0;
                    if (arguments != null && arguments.Count > 0)
                        numArgs = arguments.Count;

                    if (!(targetContent is FlowBase)) {
                        Error ("Divert target needs to be a knot or stitch in order to pass arguments");
                        return;
                    } 

                    var targetFlow = (FlowBase)targetContent;
                    if (targetFlow.parameterNames == null && numArgs > 0) {
                        Error ("Divert's target (" + targetFlow.name + ") doesn't take parameters");
                        return;
                    }

                    var paramCount = targetFlow.parameterNames.Count;
                    if (paramCount != numArgs) {
                        string butClause;
                        if (numArgs == 0) {
                            butClause = "but there weren't any passed to it";
                        } else if (numArgs < paramCount) {
                            butClause = "but only got " + numArgs;
                        } else {
                            butClause = "but got " + numArgs;
                        }
                        Error ("Divert to '" + targetFlow.name + "' requires " + paramCount + " arguments, "+butClause);
                        return;
                    }
                }
            }

			if (targetContent != null) {
				runtimeDivert.targetPath = targetContent.runtimePath;
			}
		}
            			
	}
}

