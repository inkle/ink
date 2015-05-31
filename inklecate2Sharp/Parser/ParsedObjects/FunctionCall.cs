using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class FunctionCall : Expression
    {
        public string name { get { return _proxyDivert.target.ambiguousName; } }
        public List<Expression> arguments { get { return _proxyDivert.arguments; } }
        public Runtime.Divert runtimeDivert { get { return _proxyDivert.runtimeDivert; } }

        public FunctionCall (string functionName, List<Expression> arguments)
        {
            _proxyDivert = new Parsed.Divert(Path.ToAmbiguous(functionName), arguments);
            _proxyDivert.parent = this;
            _proxyDivert.isFunctionCall = true;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            container.AddContent (_proxyDivert.runtimeObject);
        }

        public override void ResolveReferences (Story context)
        {
            _proxyDivert.ResolveReferences (context);
        }

        Parsed.Divert _proxyDivert;
    }
}

