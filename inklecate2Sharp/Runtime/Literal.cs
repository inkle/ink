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

    public abstract class Literal<T> : Literal
    {
        public T value { get; set; }

        public Literal (T literalVal)
        {
            value = literalVal;
        }

        public override string ToString ()
        {
            return value.ToString();
        }
    }

    public class LiteralInt : Literal<int>
    {
        public override LiteralType literalType { get { return LiteralType.Int; } }
        public override bool isTruthy { get { return value != 0; } }

        public LiteralInt(int literalVal) : base(literalVal)
        {
        }
    }
        
}

