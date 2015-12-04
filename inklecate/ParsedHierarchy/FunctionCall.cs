using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    internal class FunctionCall : Expression
    {
        public string name { get { return _proxyDivert.target.firstComponent; } }
        public List<Expression> arguments { get { return _proxyDivert.arguments; } }
        public Runtime.Divert runtimeDivert { get { return _proxyDivert.runtimeDivert; } }
        public bool isChoiceCount { get { return name == "CHOICE_COUNT"; } }
        public bool isTurnsSince { get { return name == "TURNS_SINCE"; } }

        public FunctionCall (string functionName, List<Expression> arguments)
        {
            _proxyDivert = new Parsed.Divert(new Path(functionName), arguments);
            _proxyDivert.isFunctionCall = true;
            AddContent (_proxyDivert);
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            if( isChoiceCount ) {

                if (arguments.Count > 0)
                    Error ("The CHOICE_COUNT() function shouldn't take any arguments");

                container.AddContent (Runtime.ControlCommand.ChoiceCount ());

            }

            else if( isTurnsSince ) {

                var literalDivertTarget = arguments [0] as DivertTarget;
                var variableDivertTarget = arguments[0] as VariableReference;

                if (arguments.Count != 1 || (literalDivertTarget == null && variableDivertTarget == null) ) {
                    Error ("The TURNS_SINCE() function should take one argument: a divert target to the target knot, stitch, gather or choice you want to check. e.g. TURNS_SINCE(-> myKnot)");
                    return;
                }

                if( literalDivertTarget ) {
                    _turnCountDivertTarget = literalDivertTarget;
                    AddContent(_turnCountDivertTarget);

                    _turnCountDivertTarget.GenerateIntoContainer(container);
                }

                else {
                    _turnCountVariableReference = variableDivertTarget;
                    AddContent(_turnCountVariableReference);

                    _turnCountVariableReference.GenerateIntoContainer(container);

                    if( !story.countAllVisits ) {
                        Error("Attempting to get TURNS_SINCE for a variable target without -c compiler option. You need the compiler switch turned on so that it can track turn counts for everything, not just those you directly reference.");
                    }
                }


                container.AddContent (Runtime.ControlCommand.TurnsSince());

            }

            // Normal function call
            else {
                container.AddContent (_proxyDivert.runtimeObject);
            }
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            if( _turnCountDivertTarget ) {
                var divert = _turnCountDivertTarget.divert;
                var attemptingTurnCountOfVariableTarget = divert.runtimeDivert.variableDivertName != null;

                if( attemptingTurnCountOfVariableTarget ) {
                    Error("When getting the TURNS_SINCE() of a variable target, remove the '->' - i.e. it should just be TURNS_SINCE("+divert.runtimeDivert.variableDivertName+")");
                    return;
                }

                var targetObject = divert.targetContent;
                if( targetObject == null ) {
                    if( !attemptingTurnCountOfVariableTarget ) {
                        Error("Failed to find target for TURNS_SINCE: '"+divert.target+"'");
                    }
                } else {
                    targetObject.containerForCounting.turnIndexShouldBeCounted = true;
                }
            }

            else if( _turnCountVariableReference ) {
                var runtimeVarRef = _turnCountVariableReference.runtimeVarRef;
                if( runtimeVarRef.pathForCount != null ) {
                    Error("Should be TURNS_SINCE(-> "+_turnCountVariableReference.name+"). Without the '->' it expects a variable target");
                }
            }
        }

        public static bool IsValidName(string name) 
        {
            return name == "CHOICE_COUNT" || name == "TURNS_SINCE";
        }

        public override string ToString ()
        {
            var strArgs = string.Join (", ", arguments);
            return string.Format ("{0}({1})", name, strArgs);
        }
            
        Parsed.Divert _proxyDivert;
        Parsed.DivertTarget _turnCountDivertTarget;
        Parsed.VariableReference _turnCountVariableReference;
    }
}

