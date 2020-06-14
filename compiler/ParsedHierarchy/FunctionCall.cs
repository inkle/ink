using System.Collections.Generic;

namespace Ink.Parsed
{
    public class FunctionCall : Expression
    {
        public string name { get { return _proxyDivert.target.firstComponent; } }
        public Divert proxyDivert { get { return _proxyDivert; } }
        public List<Expression> arguments { get { return _proxyDivert.arguments; } }
        public Runtime.Divert runtimeDivert { get { return _proxyDivert.runtimeDivert; } }
        public bool isChoiceCount { get { return name == "CHOICE_COUNT"; } }
        public bool isTurns { get { return name == "TURNS"; } }
        public bool isTurnsSince { get { return name == "TURNS_SINCE"; } }
        public bool isRandom { get { return name == "RANDOM"; } }
        public bool isSeedRandom { get { return name == "SEED_RANDOM"; } }
        public bool isListRange { get { return name == "LIST_RANGE"; } }
        public bool isListRandom { get { return name == "LIST_RANDOM"; } }
        public bool isReadCount { get { return name == "READ_COUNT"; } }

        public bool shouldPopReturnedValue;

        public FunctionCall (Identifier functionName, List<Expression> arguments)
        {
            _proxyDivert = new Parsed.Divert(new Path(functionName), arguments);
            _proxyDivert.isFunctionCall = true;
            AddContent (_proxyDivert);
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            var foundList = story.ResolveList (name);

            bool usingProxyDivert = false;

            if (isChoiceCount) {

                if (arguments.Count > 0)
                    Error ("The CHOICE_COUNT() function shouldn't take any arguments");

                container.AddContent (Runtime.ControlCommand.ChoiceCount ());

            } else if (isTurns) {

                if (arguments.Count > 0)
                    Error ("The TURNS() function shouldn't take any arguments");

                container.AddContent (Runtime.ControlCommand.Turns ());

            } else if (isTurnsSince || isReadCount) {

                var divertTarget = arguments [0] as DivertTarget;
                var variableDivertTarget = arguments [0] as VariableReference;

                if (arguments.Count != 1 || (divertTarget == null && variableDivertTarget == null)) {
                    Error ("The " + name + "() function should take one argument: a divert target to the target knot, stitch, gather or choice you want to check. e.g. TURNS_SINCE(-> myKnot)");
                    return;
                }

                if (divertTarget) {
                    _divertTargetToCount = divertTarget;
                    AddContent (_divertTargetToCount);

                    _divertTargetToCount.GenerateIntoContainer (container);
                } else {
                    _variableReferenceToCount = variableDivertTarget;
                    AddContent (_variableReferenceToCount);

                    _variableReferenceToCount.GenerateIntoContainer (container);
                }

                if (isTurnsSince)
                    container.AddContent (Runtime.ControlCommand.TurnsSince ());
                else
                    container.AddContent (Runtime.ControlCommand.ReadCount ());

            } else if (isRandom) {
                if (arguments.Count != 2)
                    Error ("RANDOM should take 2 parameters: a minimum and a maximum integer");

                // We can type check single values, but not complex expressions
                for (int arg = 0; arg < arguments.Count; arg++) {
                    if (arguments [arg] is Number) {
                        var num = arguments [arg] as Number;
                        if (!(num.value is int)) {
                            string paramName = arg == 0 ? "minimum" : "maximum";
                            Error ("RANDOM's " + paramName + " parameter should be an integer");
                        }
                    }

                    arguments [arg].GenerateIntoContainer (container);
                }

                container.AddContent (Runtime.ControlCommand.Random ());

            } else if (isSeedRandom) {
                if (arguments.Count != 1)
                    Error ("SEED_RANDOM should take 1 parameter - an integer seed");

                var num = arguments [0] as Number;
                if (num && !(num.value is int)) {
                    Error ("SEED_RANDOM's parameter should be an integer seed");
                }

                arguments [0].GenerateIntoContainer (container);

                container.AddContent (Runtime.ControlCommand.SeedRandom ());

            } else if (isListRange) {
                if (arguments.Count != 3)
                    Error ("LIST_RANGE should take 3 parameters - a list, a min and a max");

                for (int arg = 0; arg < arguments.Count; arg++)
                    arguments [arg].GenerateIntoContainer (container);

                container.AddContent (Runtime.ControlCommand.ListRange ());

            } else if( isListRandom ) {
                if (arguments.Count != 1)
                    Error ("LIST_RANDOM should take 1 parameter - a list");

                arguments [0].GenerateIntoContainer (container);

                container.AddContent (Runtime.ControlCommand.ListRandom ());

            } else if (Runtime.NativeFunctionCall.CallExistsWithName (name)) {

                var nativeCall = Runtime.NativeFunctionCall.CallWithName (name);

                if (nativeCall.numberOfParameters != arguments.Count) {
                    var msg = name + " should take " + nativeCall.numberOfParameters + " parameter";
                    if (nativeCall.numberOfParameters > 1)
                        msg += "s";
                    Error (msg);
                }

                for (int arg = 0; arg < arguments.Count; arg++)
                    arguments [arg].GenerateIntoContainer (container);

                container.AddContent (Runtime.NativeFunctionCall.CallWithName (name));
            } else if (foundList != null) {
                if (arguments.Count > 1)
                    Error ("Can currently only construct a list from one integer (or an empty list from a given list definition)");

                // List item from given int
                if (arguments.Count == 1) {
                    container.AddContent (new Runtime.StringValue (name));
                    arguments [0].GenerateIntoContainer (container);
                    container.AddContent (Runtime.ControlCommand.ListFromInt ());
                }

                // Empty list with given origin.
                else {
                    var list = new Runtime.InkList ();
                    list.SetInitialOriginName (name);
                    container.AddContent (new Runtime.ListValue (list));
                }
            }

            // Normal function call
            else {
                container.AddContent (_proxyDivert.runtimeObject);
                usingProxyDivert = true;
            }

            // Don't attempt to resolve as a divert if we're not doing a normal function call
            if( !usingProxyDivert ) content.Remove (_proxyDivert);

            // Function calls that are used alone on a tilda-based line:
            //  ~ func()
            // Should tidy up any returned value from the evaluation stack,
            // since it's unused.
            if (shouldPopReturnedValue)
                container.AddContent (Runtime.ControlCommand.PopEvaluatedValue ());
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            // If we aren't using the proxy divert after all (e.g. if
            // it's a native function call), but we still have arguments,
            // we need to make sure they get resolved since the proxy divert
            // is no longer in the content array.
            if (!content.Contains(_proxyDivert) && arguments != null) {
                foreach (var arg in arguments)
                    arg.ResolveReferences (context);
            }

            if( _divertTargetToCount ) {
                var divert = _divertTargetToCount.divert;
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

            else if( _variableReferenceToCount ) {
                var runtimeVarRef = _variableReferenceToCount.runtimeVarRef;
                if( runtimeVarRef.pathForCount != null ) {
                    Error("Should be "+name+"(-> "+_variableReferenceToCount.name+"). Usage without the '->' only makes sense for variable targets.");
                }
            }
        }

        public static bool IsBuiltIn(string name)
        {
            if (Runtime.NativeFunctionCall.CallExistsWithName (name))
                return true;

            return name == "CHOICE_COUNT"
                || name == "TURNS_SINCE"
                || name == "TURNS"
                || name == "RANDOM"
                || name == "SEED_RANDOM"
                || name == "LIST_VALUE"
                || name == "LIST_RANDOM"
                || name == "READ_COUNT";
        }

        public override string ToString ()
        {
            var strArgs = string.Join (", ", arguments.ToStringsArray());
            return string.Format ("{0}({1})", name, strArgs);
        }

        Parsed.Divert _proxyDivert;
        Parsed.DivertTarget _divertTargetToCount;
        Parsed.VariableReference _variableReferenceToCount;
    }
}

