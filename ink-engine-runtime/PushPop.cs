using System;
using System.Collections.Generic;

namespace Ink.Runtime
{
    internal enum PushPopType 
    {
        Tunnel,
        Function
    }

    internal class Pop : Runtime.Object
    {
        public PushPopType type;

        public Pop (PushPopType type)
        {
            this.type = type;
        }

        // For serialisation only
        public Pop()
        {
            this.type = PushPopType.Tunnel;
        }

        public override string ToString ()
        {
            return string.Format ("Pop {0}", this.type);
        }
    }
}

