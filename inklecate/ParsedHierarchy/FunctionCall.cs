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
            // Built in function
            if (name == "choice_count") {
                if (arguments.Count > 0)
                    Error ("The choice_count() function shouldn't take any arguments");

                container.AddContent (Runtime.ControlCommand.ChoiceCount());
            } 

            // Normal function call
            else {
                container.AddContent (_proxyDivert.runtimeObject);
            }
        }

        public override string ToString ()
        {
            var strArgs = string.Join (", ", arguments);
            return string.Format ("{0}({1})", name, strArgs);
        }

        Parsed.Divert _proxyDivert;
    }
}

