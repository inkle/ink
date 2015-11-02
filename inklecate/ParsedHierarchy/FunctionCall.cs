using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    internal class FunctionCall : Expression
    {
        public string name { get { return _proxyDivert.target.firstComponent; } }
        public List<Expression> arguments { get { return _proxyDivert.arguments; } }
        public Runtime.Divert runtimeDivert { get { return _proxyDivert.runtimeDivert; } }

        public FunctionCall (string functionName, List<Expression> arguments)
        {
            _proxyDivert = new Parsed.Divert(new Path(functionName), arguments);
            _proxyDivert.isFunctionCall = true;
            AddContent (_proxyDivert);
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            switch (name) {
            case "CHOICE_COUNT":
                if (arguments.Count > 0)
                    Error ("The CHOICE_COUNT() function shouldn't take any arguments");

                container.AddContent (Runtime.ControlCommand.ChoiceCount ());
                break;

            case "TURNS_SINCE":
                if (arguments.Count != 1 || !(arguments [0] is VariableReference) ) {
                    Error ("The TURNS_SINCE() function should take one argument: the path to the target knot, stitch, gather or choice you want to check");
                    return;
                }
                _turnCountTargetReference = arguments [0] as VariableReference;
                _turnCountTargetReference.isTurnCount = true;
                _turnCountTargetReference.GenerateIntoContainer (container);
                break;

            // Normal function call
            default:
                container.AddContent (_proxyDivert.runtimeObject);
                break;
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
        Parsed.VariableReference _turnCountTargetReference;
    }
}

