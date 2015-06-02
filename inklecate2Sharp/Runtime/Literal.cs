using System;

namespace Inklewriter.Runtime
{
    public enum LiteralType
    {
        Int,
        Float
    }

    public abstract class Literal : Runtime.Object
    {
        public abstract LiteralType literalType { get; }
        public abstract bool isTruthy { get; }
    }

    public class LiteralInt : Literal
    {
        public int value { get; set; }
        public override LiteralType literalType { get { return LiteralType.Int; } }
        public override bool isTruthy { get { return value != 0; } }

        public LiteralInt (int literalVal)
        {
            value = literalVal;
        }

        public override string ToString ()
        {
            return value.ToString();
        }
    }
        
}

