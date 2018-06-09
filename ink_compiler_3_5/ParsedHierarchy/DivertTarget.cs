﻿
namespace Ink.Parsed
{
    internal class DivertTarget : Expression
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

            // Example ink for this class:
            //
            //     VAR x = -> blah
            //
            // ...which means that "blah" is expected to be a literal stitch  target rather
            // than a variable name. We can't really intelligently recover from this (e.g. if blah happens to
            // contain a divert target itself) since really we should be generating a variable reference
            // rather than a concrete DivertTarget, so we list it as an error.
            if (_runtimeDivert.hasVariableTarget)
                Error ("Since '"+divert.target.dotSeparatedComponents+"' is a variable, it shouldn't be preceded by '->' here.");

            _runtimeDivertTargetValue.targetPath = _runtimeDivert.targetPath;
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

