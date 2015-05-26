using System;

namespace Inklewriter.Runtime
{
    public class VariableReference : Runtime.Object
    {
        public string name { get; protected set; }

        public VariableReference (string name)
        {
            this.name = name;
        }

        public override string ToString ()
        {
            return name;
        }
    }
}

