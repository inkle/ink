using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace Ink.Runtime
{
    // Order is significant for type coersion.
    // If types aren't directly compatible for an operation,
    // they're coerced to the same type, downward.
    // Higher value types "infect" an operation.
    // (This may not be the most sensible thing to do, but it's worked so far!)
    internal enum LiteralType
    {
        // Used in coersion
        Int,
        Float,
        String,

        // Not used for coersion described above
        DivertTarget,
        VariablePointer
    }

    internal abstract class Literal : Runtime.Object
    {
        public abstract LiteralType literalType { get; }
        public abstract bool isTruthy { get; }

        public abstract Literal Cast(LiteralType newType);

        public abstract object valueObject { get; }

        public static Literal Create(object val)
        {
            // Implicitly lose precision from any doubles we get passed in
            if (val is double) {
                double doub = (double)val;
                val = (float)doub;
            }

            // Implicitly convert bools into ints
            if (val is bool) {
                bool b = (bool)val;
                val = (int)(b ? 1 : 0);
            }

            if (val is int) {
                return new LiteralInt ((int)val);
            } else if (val is long) {
                return new LiteralInt ((int)(long)val);
            } else if (val is float) {
                return new LiteralFloat ((float)val);
            } else if (val is double) {
                return new LiteralFloat ((float)(double)val);
            } else if (val is string) {
                return new LiteralString ((string)val);
            } else if (val is Path) {
                return new LiteralDivertTarget ((Path)val);
            }

            return null;
        }
    }

    internal abstract class Literal<T> : Literal
    {
        [JsonProperty("v")]
        public T value { get; set; }

        public override object valueObject {
            get {
                return (object)value;
            }
        }

        public Literal (T literalVal)
        {
            value = literalVal;
        }

        public override string ToString ()
        {
            return value.ToString();
        }
    }

    internal class LiteralInt : Literal<int>
    {
        public override LiteralType literalType { get { return LiteralType.Int; } }
        public override bool isTruthy { get { return value != 0; } }

        public LiteralInt(int literalVal) : base(literalVal)
        {
        }

        public LiteralInt() : this(0) {}

        public override Literal Cast(LiteralType newType)
        {
            if (newType == literalType) {
                return this;
            }

            if (newType == LiteralType.Float) {
                return new LiteralFloat ((float)this.value);
            }

            if (newType == LiteralType.String) {
                return new LiteralString("" + this.value);
            }

            throw new System.Exception ("Unexpected type cast of Literal to new LiteralType");
        }
    }

    internal class LiteralFloat : Literal<float>
    {
        public override LiteralType literalType { get { return LiteralType.Float; } }
        public override bool isTruthy { get { return value != 0.0f; } }

        public LiteralFloat(float literalVal) : base(literalVal)
        {
        }

        public LiteralFloat() : this(0.0f) {}

        public override Literal Cast(LiteralType newType)
        {
            if (newType == literalType) {
                return this;
            }

            if (newType == LiteralType.Int) {
                return new LiteralInt ((int)this.value);
            }

            if (newType == LiteralType.String) {
                return new LiteralString("" + this.value);
            }

            throw new System.Exception ("Unexpected type cast of Literal to new LiteralType");
        }
    }

    internal class LiteralString : Literal<string>
    {
        public override LiteralType literalType { get { return LiteralType.String; } }
        public override bool isTruthy { get { return value.Length > 0; } }

        public LiteralString(string literalVal) : base(literalVal)
        {
        }

        public LiteralString() : this("") {}

        public override Literal Cast(LiteralType newType)
        {
            if (newType == literalType) {
                return this;
            }

            if (newType == LiteralType.Int) {

                int parsedInt;
                if (int.TryParse (value, out parsedInt)) {
                    return new LiteralInt (parsedInt);
                } else {
                    return null;
                }
            }

            if (newType == LiteralType.Float) {
                float parsedFloat;
                if (float.TryParse (value, out parsedFloat)) {
                    return new LiteralFloat (parsedFloat);
                } else {
                    return null;
                }
            }

            throw new System.Exception ("Unexpected type cast of Literal to new LiteralType");
        }
    }

    internal class LiteralDivertTarget : Literal<Path>
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

    // TODO: Think: Erm, I get that this contains a string, but should
    // we really derive from Literal<string>? That seems a bit misleading to me.
    internal class LiteralVariablePointer : Literal<string>
    {
        public string variableName { get { return this.value; } set { this.value = value; } }
        public override LiteralType literalType { get { return LiteralType.VariablePointer; } }
        public override bool isTruthy { get { throw new System.Exception("Shouldn't be checking the truthiness of a variable pointer"); } }

        // Where the variable is located
        // -1 = default, unknown, yet to be determined
        // 0  = in global scope
        // 1+ = callstack element index
        public int contextIndex { get; set; }

        public LiteralVariablePointer(string variableName, int contextIndex = -1) : base(variableName)
        {
            this.contextIndex = contextIndex;
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

