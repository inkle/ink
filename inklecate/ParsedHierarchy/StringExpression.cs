using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Parsed
{
    internal class StringExpression : Parsed.Expression
    {
        public bool isSingleString {
            get {
                if (content.Count != 1)
                    return false;

                var c = content [0];
                if (!(c is Text))
                    return false;

                return true;
            }
        }

        public StringExpression (List<Parsed.Object> content)
        {
            AddContent (content);
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            container.AddContent (Runtime.ControlCommand.BeginString());

            foreach (var c in content) {
                container.AddContent (c.runtimeObject);
            }
                
            container.AddContent (Runtime.ControlCommand.EndString());
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            foreach (var c in content) {
                sb.Append (c.ToString ());
            }
            return sb.ToString ();
        }
    }
}

