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

        public abstract Literal Cast(LiteralType newType);
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

        public override Literal Cast(LiteralType newType)
        {
            if (newType == literalType) {
                return this;
            }

            if (newType == LiteralType.Float) {
                return new LiteralFloat ((float)this.value);
            }

            throw new System.Exception ("Unexpected type cast of Litereal to new LiteralType");
        }
    }

    public class LiteralFloat : Literal<float>
    {
        public override LiteralType literalType { get { return LiteralType.Float; } }
        public override bool isTruthy { get { return value != 0.0f; } }

        public LiteralFloat(float literalVal) : base(literalVal)
        {
        }

        public override Literal Cast(LiteralType newType)
        {
            if (newType == literalType) {
                return this;
            }

            if (newType == LiteralType.Int) {
                return new LiteralInt ((int)this.value);
            }

            throw new System.Exception ("Unexpected type cast of Litereal to new LiteralType");
        }
    }
        
}

