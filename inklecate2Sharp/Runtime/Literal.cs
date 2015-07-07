
namespace Inklewriter.Runtime
{
    public enum LiteralType
    {
        Int,
        Float,
        DivertTarget,
        VariablePointer
    }

    public abstract class Literal : Runtime.Object
    {
        public abstract LiteralType literalType { get; }
        public abstract bool isTruthy { get; }

        public abstract Literal Cast(LiteralType newType);

        public static Literal Create(object val)
        {
            if (val is int) {
                return new LiteralInt ((int)val);
            } else if (val is float) {
                return new LiteralFloat ((float)val);
            } else if (val is Path) {
                return new LiteralDivertTarget ((Path)val);
            }

            return null;
        }
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

            throw new System.Exception ("Unexpected type cast of Literal to new LiteralType");
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

            throw new System.Exception ("Unexpected type cast of Literal to new LiteralType");
        }
    }

    public class LiteralDivertTarget : Literal<Path>
    {
        public Path targetPath { get { return this.value; } set { this.value = value; } }
        public override LiteralType literalType { get { return LiteralType.DivertTarget; } }
        public override bool isTruthy { get { throw new System.Exception("Shouldn't be checking the truthiness of a divert target"); } }

        public LiteralDivertTarget(Path targetPath) : base(targetPath)
        {
        }

        public LiteralDivertTarget() : base(null)
        {}

        public override Literal Cast(LiteralType newType)
        {
            if (newType == literalType)
                return this;
            
            throw new System.Exception ("Unexpected type cast of Literal to new LiteralType");
        }

        public override string ToString ()
        {
            return "LiteralDivertTarget(" + targetPath + ")";
        }
    }

    public class LiteralVariablePointer : Literal<string>
    {
        public string variableName { get { return this.value; } set { this.value = value; } }
        public override LiteralType literalType { get { return LiteralType.VariablePointer; } }
        public override bool isTruthy { get { throw new System.Exception("Shouldn't be checking the truthiness of a variable pointer"); } }
        public int resolvedCallstackElementIndex { get; set; }

        public LiteralVariablePointer(string variableName) : base(variableName)
        {
            resolvedCallstackElementIndex = -1;
        }

        public LiteralVariablePointer() : this(null)
        {
        }

        public override Literal Cast(LiteralType newType)
        {
            if (newType == literalType)
                return this;

            throw new System.Exception ("Unexpected type cast of Literal to new LiteralType");
        }

        public override string ToString ()
        {
            return "LiteralVariablePointer(" + variableName + ")";
        }
    }
        
}

