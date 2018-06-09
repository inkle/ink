using System.Collections.Generic;
using System.Linq;

namespace Ink.Parsed
{
	public class Divert : Parsed.Object
	{
		public Parsed.Path target { get; protected set; }
        public Parsed.Object targetContent { get; protected set; }
        public List<Expression> arguments { get; protected set; }
		public Runtime.Divert runtimeDivert { get; protected set; }
        public bool isFunctionCall { get; set; }
        public bool isEmpty { get; set; }
        public bool isTunnel { get; set; }
        public bool isThread { get; set; }
        public bool isEnd { 
            get {
                return target != null && target.dotSeparatedComponents == "END";
            }
        }
        public bool isDone { 
            get {
                return target != null && target.dotSeparatedComponents == "DONE";
            }
        }

        public Divert (Parsed.Path target, List<Expression> arguments = null)
		{
			this.target = target;
            this.arguments = arguments;

            if (arguments != null) {
                AddContent (arguments.Cast<Parsed.Object> ().ToList ());
            }
		}

        public Divert (Parsed.Object targetContent)
        {
            this.targetContent = targetContent;
        }

		public override Runtime.Object GenerateRuntimeObject ()
		{
            // End = end flow immediately
            // Done = return from thread or instruct the flow that it's safe to exit
            if (isEnd) {
                return Runtime.ControlCommand.End ();
            }
            if (isDone) {
                return Runtime.ControlCommand.Done ();
            }

            runtimeDivert = new Runtime.Divert ();

            // Normally we resolve the target content during the
            // Resolve phase, since we expect all runtime objects to
            // be available in order to find the final runtime path for
            // the destination. However, we need to resolve the target
            // (albeit without the runtime target) early so that
            // we can get information about the arguments - whether
            // they're by reference - since it affects the code we 
            // generate here.
            ResolveTargetContent ();


            CheckArgumentValidity ();

            // Passing arguments to the knot
            bool requiresArgCodeGen = arguments != null && arguments.Count > 0;
            if ( requiresArgCodeGen || isFunctionCall || isTunnel || isThread ) {

                var container = new Runtime.Container ();

                // Generate code for argument evaluation
                // This argument generation is coded defensively - it should
                // attempt to generate the code for all the parameters, even if
                // they don't match the expected arguments. This is so that the
                // parameter objects themselves are generated correctly and don't
                // get into a state of attempting to resolve references etc
                // without being generated.
                if (requiresArgCodeGen) {

                    // Function calls already in an evaluation context
                    if (!isFunctionCall) {
                        container.AddContent (Runtime.ControlCommand.EvalStart());
                    }

                    List<FlowBase.Argument> targetArguments = null;
                    if( targetContent )
                        targetArguments = (targetContent as FlowBase).arguments;

                    for (var i = 0; i < arguments.Count; ++i) {
                        Expression argToPass = arguments [i];
                        FlowBase.Argument argExpected = null; 
                        if( targetArguments != null && i < targetArguments.Count ) 
                            argExpected = targetArguments [i];

                        // Pass by reference: argument needs to be a variable reference
                        if (argExpected != null && argExpected.isByReference) {

                            var varRef = argToPass as VariableReference;
                            if (varRef == null) {
                                Error ("Expected variable name to pass by reference to 'ref " + argExpected.name + "' but saw " + argToPass.ToString ());
                                break;
                            }

                            // Check that we're not attempting to pass a read count by reference
                            var targetPath = new Path(varRef.path);
                            Parsed.Object targetForCount = targetPath.ResolveFromContext (this);
                            if (targetForCount != null) {
                                Error ("can't pass a read count by reference. '" + targetPath.dotSeparatedComponents+"' is a knot/stitch/label, but '"+target.dotSeparatedComponents+"' requires the name of a VAR to be passed.");
                                break;
                            }

                            var varPointer = new Runtime.VariablePointerValue (varRef.name);
                            container.AddContent (varPointer);
                        } 

                        // Normal value being passed: evaluate it as normal
                        else {
                            argToPass.GenerateIntoContainer (container);
                        }
                    }

                    // Function calls were already in an evaluation context
                    if (!isFunctionCall) {
                        container.AddContent (Runtime.ControlCommand.EvalEnd());
                    }
                }
                    

                // Starting a thread? A bit like a push to the call stack below... but not.
                // It sort of puts the call stack on a thread stack (argh!) - forks the full flow.
                if (isThread) {
                    container.AddContent(Runtime.ControlCommand.StartThread());
                }

                // If this divert is a function call, tunnel, we push to the call stack
                // so we can return again
                else if (isFunctionCall || isTunnel) {
                    runtimeDivert.pushesToStack = true;
                    runtimeDivert.stackPushType = isFunctionCall ? Runtime.PushPopType.Function : Runtime.PushPopType.Tunnel;
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


        // When the divert is to a target that's actually a variable name
        // rather than an explicit knot/stitch name, try interpretting it
        // as such by getting the variable name.
        public string PathAsVariableName()
        {
            return target.firstComponent;
        }
            

        void ResolveTargetContent()
        {
            if (isEmpty || isEnd) {
                return;
            }

            if (targetContent == null) {

                // Is target of this divert a variable name that will be de-referenced
                // at runtime? If so, there won't be any further reference resolution
                // we can do at this point.
                var variableTargetName = PathAsVariableName ();
                if (variableTargetName != null) {
                    var flowBaseScope = ClosestFlowBase ();
                    var resolveResult = flowBaseScope.ResolveVariableWithName (variableTargetName, fromNode: this);
                    if (resolveResult.found) {

                        // Make sure that the flow was typed correctly, given that we know that this
                        // is meant to be a divert target
                        if (resolveResult.isArgument) {
                            var argument = resolveResult.ownerFlow.arguments.Where (a => a.name == variableTargetName).First();
                            if ( !argument.isDivertTarget ) {
                                Error ("Since '" + argument.name + "' is used as a variable divert target (on "+this.debugMetadata+"), it should be marked as: -> " + argument.name, resolveResult.ownerFlow);
                            }
                        }

                        runtimeDivert.variableDivertName = variableTargetName;
                        return;

                    }
                }

                targetContent = target.ResolveFromContext (this);
            }
        }

        public override void ResolveReferences(Story context)
		{
            if (isEmpty || isEnd || isDone) {
                return;
            }

            if (targetContent) {
                runtimeDivert.targetPath = targetContent.runtimePath;
            }

            // Resolve children (the arguments)
            base.ResolveReferences (context);

            // May be null if it's a built in function (e.g. TURNS_SINCE)
            // or if it's a variable target.
            var targetFlow = targetContent as FlowBase;
            if (targetFlow) {
                if (!targetFlow.isFunction && this.isFunctionCall) {
                    base.Error (targetFlow.name + " hasn't been marked as a function, but it's being called as one. Do you need to delcare the knot as '== function " + targetFlow.name + " =='?");
                } else if (targetFlow.isFunction && !this.isFunctionCall && !(this.parent is DivertTarget)) {
                    base.Error (targetFlow.name + " can't be diverted to. It can only be called as a function since it's been marked as such: '" + targetFlow.name + "(...)'");
                }
            } 

            // Check validity of target content
            bool targetWasFound = targetContent != null;
            bool isBuiltIn = false;
            bool isExternal = false;

            if (target.numberOfComponents == 1 ) {

                // BuiltIn means TURNS_SINCE, CHOICE_COUNT, RANDOM or SEED_RANDOM
                isBuiltIn = FunctionCall.IsBuiltIn (target.firstComponent);

                // Client-bound function?
                isExternal = context.IsExternal (target.firstComponent);

                if (isBuiltIn || isExternal) {
                    if (!isFunctionCall) {
                        base.Error (target.firstComponent + " must be called as a function: ~ " + target.firstComponent + "()");
                    }
                    if (isExternal) {
                        runtimeDivert.isExternal = true;
                        if( arguments != null )
                            runtimeDivert.externalArgs = arguments.Count;
                        runtimeDivert.pushesToStack = false;
                        runtimeDivert.targetPath = new Runtime.Path (this.target.firstComponent);
                        CheckExternalArgumentValidity (context);
                    }
                    return;
                }
            }

            // Variable target?
            if (runtimeDivert.variableDivertName != null) {
                return;
            }
                  
            if( !targetWasFound && !isBuiltIn && !isExternal )
                Error ("target not found: '" + target + "'");
		}

        // Returns false if there's an error
        void CheckArgumentValidity()
        {
            if (isEmpty) 
                return;

            // Argument passing: Check for errors in number of arguments
            var numArgs = 0;
            if (arguments != null && arguments.Count > 0)
                numArgs = arguments.Count;

            // Missing content?
            // Can't check arguments properly. It'll be due to some
            // other error though, so although there's a problem and 
            // we report false, we don't need to report a specific error.
            // It may also be because it's a valid call to an external
            // function, that we check at the resolve stage.
            if (targetContent == null) {
                return;
            }

            FlowBase targetFlow = targetContent as FlowBase;

            // No error, crikey!
            if (numArgs == 0 && (targetFlow == null || !targetFlow.hasParameters)) {
                return;
            }

            if (targetFlow == null && numArgs > 0) {
                Error ("target needs to be a knot or stitch in order to pass arguments");
                return;
            }

            if (targetFlow.arguments == null && numArgs > 0) {
                Error ("target (" + targetFlow.name + ") doesn't take parameters");
                return;
            }

            if( this.parent is DivertTarget ) {
                if (numArgs > 0)
                    Error ("can't store arguments in a divert target variable");
                return;
            }

            var paramCount = targetFlow.arguments.Count;
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

            // Light type-checking for divert target arguments
            for (int i = 0; i < paramCount; ++i) {
                FlowBase.Argument flowArg = targetFlow.arguments [i];
                Parsed.Expression divArgExpr = arguments [i];

                // Expecting a divert target as an argument, let's do some basic type checking
                if (flowArg.isDivertTarget) {

                    // Not passing a divert target or any kind of variable reference?
                    var varRef = divArgExpr as VariableReference;
                    if (!(divArgExpr is DivertTarget) && varRef == null ) {
                        Error ("Target '" + targetFlow.name + "' expects a divert target for the parameter named -> " + flowArg.name + " but saw " + divArgExpr, divArgExpr);
                    } 

                    // Passing 'a' instead of '-> a'? 
                    // i.e. read count instead of divert target
                    else if (varRef != null) {

                        // Unfortunately have to manually resolve here since we're still in code gen
                        var knotCountPath = new Path(varRef.path);
                        Parsed.Object targetForCount = knotCountPath.ResolveFromContext (varRef);
                        if (targetForCount != null) {
                            Error ("Passing read count of '" + knotCountPath.dotSeparatedComponents + "' instead of a divert target. You probably meant '" + knotCountPath + "'");
                        }
                    }
                }
            }
                
            if (targetFlow == null) {
                Error ("Can't call as a function or with arguments unless it's a knot or stitch");
                return;
            }

            return;
        }

        void CheckExternalArgumentValidity(Story context)
        {
            string externalName = target.firstComponent;
            ExternalDeclaration external = null;
            var found = context.externals.TryGetValue(externalName, out external);
            System.Diagnostics.Debug.Assert (found, "external not found");

            int externalArgCount = external.argumentNames.Count;
            int ownArgCount = 0;
            if (arguments != null) {
                ownArgCount = arguments.Count;
            }

            if (ownArgCount != externalArgCount) {
                Error ("incorrect number of arguments sent to external function '" + externalName + "'. Expected " + externalArgCount + " but got " + ownArgCount);
            }
        }

        public override void Error (string message, Object source = null, bool isWarning = false)
        {
            // Could be getting an error from a nested Divert
            if (source != this && source) {
                base.Error (message, source);
                return;
            }

            if (isFunctionCall) {
                base.Error ("Function call " + message, source, isWarning);
            } else {
                base.Error ("Divert " + message, source, isWarning);
            }
        }

        public override string ToString ()
        {
            if (target != null)
                return target.ToString ();
            else
                return "-> <empty divert>";
        }

	}
}

