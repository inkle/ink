
namespace Ink.Parsed
{
    public class DivertTarget : Expression
    {
        public Divert divert;

        public DivertTarget (Divert divert)
        {
            this.divert = AddContent(divert);
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            divert.GenerateRuntimeObject();

            _runtimeDivert = (Runtime.Divert) divert.runtimeDivert;
            _runtimeDivertTargetValue = new Runtime.DivertTargetValue ();

            container.AddContent (_runtimeDivertTargetValue);
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            if( divert.isDone || divert.isEnd )
            {
                Error("Can't Can't use -> DONE or -> END as variable divert targets", this);
                return;
            }

            Parsed.Object usageContext = this;
            while (usageContext && usageContext is Expression) {

                bool badUsage = false;
                bool foundUsage = false;

                var usageParent = usageContext.parent;
                if (usageParent is BinaryExpression) {

                    // Only allowed to compare for equality

                    var binaryExprParent = usageParent as BinaryExpression;
                    if (binaryExprParent.opName != "==" && binaryExprParent.opName != "!=") {
                        badUsage = true;
                    } else {
                        if (!(binaryExprParent.leftExpression is DivertTarget || binaryExprParent.leftExpression is VariableReference)) {
                            badUsage = true;
                        }
                        if (!(binaryExprParent.rightExpression is DivertTarget || binaryExprParent.rightExpression is VariableReference)) {
                            badUsage = true;
                        }
                    }
                    foundUsage = true;
                }
                else if( usageParent is FunctionCall ) {
                    var funcCall = usageParent as FunctionCall;
                    if( !funcCall.isTurnsSince && !funcCall.isReadCount ) {
                        badUsage = true;
                    }
                    foundUsage = true;
                }
                else if (usageParent is Expression) {
                    badUsage = true;
                    foundUsage = true;
                }
                else if (usageParent is MultipleConditionExpression) {
                    badUsage = true;
                    foundUsage = true;
                } else if (usageParent is Choice && ((Choice)usageParent).condition == usageContext) {
                    badUsage = true;
                    foundUsage = true;
                } else if (usageParent is Conditional || usageParent is ConditionalSingleBranch) {
                    badUsage = true;
                    foundUsage = true;
                }

                if (badUsage) {
                    Error ("Can't use a divert target like that. Did you intend to call '" + divert.target + "' as a function: likeThis(), or check the read count: likeThis, with no arrows?", this);
                }

                if (foundUsage)
                    break;

                usageContext = usageParent;
            }

            // Example ink for this case:
            //
            //     VAR x = -> blah
            //
            // ...which means that "blah" is expected to be a literal stitch  target rather
            // than a variable name. We can't really intelligently recover from this (e.g. if blah happens to
            // contain a divert target itself) since really we should be generating a variable reference
            // rather than a concrete DivertTarget, so we list it as an error.
            if (_runtimeDivert.hasVariableTarget)
                Error ("Since '"+divert.target.dotSeparatedComponents+"' is a variable, it shouldn't be preceded by '->' here.");

            // Main resolve
            _runtimeDivertTargetValue.targetPath = _runtimeDivert.targetPath;

            // Tell hard coded (yet variable) divert targets that they also need to be counted
            // TODO: Only detect DivertTargets that are values rather than being used directly for
            // read or turn counts. Should be able to detect this by looking for other uses of containerForCounting
            var targetContent = this.divert.targetContent;
            if (targetContent != null ) {
                var target = targetContent.containerForCounting;
                if (target != null)
                {
                    // Purpose is known: used directly in TURNS_SINCE(-> divTarg)
                    var parentFunc = this.parent as FunctionCall;
                    if( parentFunc && parentFunc.isTurnsSince ) {
                        target.turnIndexShouldBeCounted = true;
                    }

                    // Unknown purpose, count everything
                    else {
                        target.visitsShouldBeCounted = true;
                        target.turnIndexShouldBeCounted = true;
                    }

                }

                // Unfortunately not possible:
                // https://github.com/inkle/ink/issues/538
                //
                // VAR func = -> double
                //
                // === function double(ref x)
                //    ~ x = x * 2
                //
                // Because when generating the parameters for a function
                // to be called, it needs to know ahead of time when
                // compiling whether to pass a variable reference or value.
                //
                var targetFlow = (targetContent as FlowBase);
                if (targetFlow != null && targetFlow.arguments != null)
                {
                    foreach(var arg in targetFlow.arguments) {
                        if(arg.isByReference)
                        {
                            Error("Can't store a divert target to a knot or function that has by-reference arguments ('"+targetFlow.identifier+"' has 'ref "+arg.identifier+"').");
                        }
                    }
                }
            }
        }

        // Equals override necessary in order to check for CONST multiple definition equality
        public override bool Equals (object obj)
        {
            var otherDivTarget = obj as DivertTarget;
            if (otherDivTarget == null) return false;

            var targetStr = this.divert.target.dotSeparatedComponents;
            var otherTargetStr = otherDivTarget.divert.target.dotSeparatedComponents;

            return targetStr.Equals (otherTargetStr);
        }

        public override int GetHashCode ()
        {
            var targetStr = this.divert.target.dotSeparatedComponents;
            return targetStr.GetHashCode ();
        }

        Runtime.DivertTargetValue _runtimeDivertTargetValue;
        Runtime.Divert _runtimeDivert;
    }
}

