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
        public bool isFunctionCall { get; set; }

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

                if (!isFunctionCall) {
                    container.AddContent (Runtime.ControlCommand.EvalStart());
                }

                foreach (var expr in arguments) {
                    expr.GenerateIntoContainer (container);
                }

                if (!isFunctionCall) {
                    container.AddContent (Runtime.ControlCommand.EvalEnd());
                }

                // If this divert is a function call, we push to the call stack
                // so we can return again
                if (isFunctionCall) {
                    container.AddContent (Runtime.ControlCommand.StackPush());
                }

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
                        Error ("target not found: '" + target.ToString () + "'. Did you mean '"+alternativePath+"'?");
                        target = alternativePath;
                    } else {
                        Error ("target not found: '" + target.ToString () + "'");
                    }
                }

                // Argument passing: Check for errors in number of arguments
                if (targetContent != null) {

                    var numArgs = 0;
                    if (arguments != null && arguments.Count > 0)
                        numArgs = arguments.Count;

                    FlowBase targetFlow = targetContent as FlowBase;

                    // No error, crikey!
                    if (numArgs == 0 && (targetFlow == null || !targetFlow.hasParameters)) {
                        return;
                    }

                    if (targetFlow == null && numArgs > 0) {
                        Error ("target needs to be a knot or stitch in order to pass arguments");
                        return;
                    } 
                        
                    if (targetFlow.parameterNames == null && numArgs > 0) {
                        Error ("target (" + targetFlow.name + ") doesn't take parameters");
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
                        Error ("to '" + targetFlow.name + "' requires " + paramCount + " arguments, "+butClause);
                        return;
                    }
                }
            }

			if (targetContent != null) {
				runtimeDivert.targetPath = targetContent.runtimePath;
			}
		}

        public override void Error (string message, Object source = null)
        {
            if (isFunctionCall) {
                base.Error ("Function call " + message, source);
            } else {
                base.Error ("Divert " + message, source);
            }

        }
            			
	}
}

