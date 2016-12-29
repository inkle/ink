using System.ComponentModel;
using System.Collections.Generic;

namespace Ink.Runtime
{
    // Order is significant for type coersion.
    // If types aren't directly compatible for an operation,
    // they're coerced to the same type, downward.
    // Higher value types "infect" an operation.
    // (This may not be the most sensible thing to do, but it's worked so far!)
    internal enum ValueType
    {
        // Used in coersion
        Int,
        Float,
        Set,
        String,

        // Not used for coersion described above
        DivertTarget,
        VariablePointer
    }

    internal abstract class Value : Runtime.Object
    {
        public abstract ValueType valueType { get; }
        public abstract bool isTruthy { get; }

        public abstract Value Cast(ValueType newType);

        public abstract object valueObject { get; }

        public static Value Create(object val)
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
                return new IntValue ((int)val);
            } else if (val is long) {
                return new IntValue ((int)(long)val);
            } else if (val is float) {
                return new FloatValue ((float)val);
            } else if (val is double) {
                return new FloatValue ((float)(double)val);
            } else if (val is string) {
                return new StringValue ((string)val);
            } else if (val is Path) {
                return new DivertTargetValue ((Path)val);
            } else if (val is Dictionary<string, int>) {
                return new SetValue ((Dictionary < string, int > )val);
            }

            return null;
        }

        public override Object Copy()
        {
            return Create (valueObject);
        }
    }

    internal abstract class Value<T> : Value
    {
        public T value { get; set; }

        public override object valueObject {
            get {
                return (object)value;
            }
        }

        public Value (T val)
        {
            value = val;
        }

        public override string ToString ()
        {
            return value.ToString();
        }
    }

    internal class IntValue : Value<int>
    {
        public override ValueType valueType { get { return ValueType.Int; } }
        public override bool isTruthy { get { return value != 0; } }

        public IntValue(int intVal) : base(intVal)
        {
        }

        public IntValue() : this(0) {}

        public override Value Cast(ValueType newType)
        {
            if (newType == valueType) {
                return this;
            }

            if (newType == ValueType.Float) {
                return new FloatValue ((float)this.value);
            }

            if (newType == ValueType.String) {
                return new StringValue("" + this.value);
            }

            throw new System.Exception ("Unexpected type cast of Value to new ValueType");
        }
    }

    internal class FloatValue : Value<float>
    {
        public override ValueType valueType { get { return ValueType.Float; } }
        public override bool isTruthy { get { return value != 0.0f; } }

        public FloatValue(float val) : base(val)
        {
        }

        public FloatValue() : this(0.0f) {}

        public override Value Cast(ValueType newType)
        {
            if (newType == valueType) {
                return this;
            }

            if (newType == ValueType.Int) {
                return new IntValue ((int)this.value);
            }

            if (newType == ValueType.String) {
                return new StringValue("" + this.value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            throw new System.Exception ("Unexpected type cast of Value to new ValueType");
        }
    }

    internal class StringValue : Value<string>
    {
        public override ValueType valueType { get { return ValueType.String; } }
        public override bool isTruthy { get { return value.Length > 0; } }

        public bool isNewline { get; private set; }
        public bool isInlineWhitespace { get; private set; }
        public bool isNonWhitespace {
            get {
                return !isNewline && !isInlineWhitespace;
            }
        }

        public StringValue(string str) : base(str)
        {
            // Classify whitespace status
            isNewline = value == "\n";
            isInlineWhitespace = true;
            foreach (var c in value) {
                if (c != ' ' && c != '\t') {
                    isInlineWhitespace = false;
                    break;
                }
            }
        }

        public StringValue() : this("") {}

        public override Value Cast(ValueType newType)
        {
            if (newType == valueType) {
                return this;
            }

            if (newType == ValueType.Int) {

                int parsedInt;
                if (int.TryParse (value, out parsedInt)) {
                    return new IntValue (parsedInt);
                } else {
                    return null;
                }
            }

            if (newType == ValueType.Float) {
                float parsedFloat;
                if (float.TryParse (value, System.Globalization.NumberStyles.Float ,System.Globalization.CultureInfo.InvariantCulture, out parsedFloat)) {
                    return new FloatValue (parsedFloat);
                } else {
                    return null;
                }
            }

            throw new System.Exception ("Unexpected type cast of Value to new ValueType");
        }
    }

    internal class DivertTargetValue : Value<Path>
    {
        public Path targetPath { get { return this.value; } set { this.value = value; } }
        public override ValueType valueType { get { return ValueType.DivertTarget; } }
        public override bool isTruthy { get { throw new System.Exception("Shouldn't be checking the truthiness of a divert target"); } }
            
        public DivertTargetValue(Path targetPath) : base(targetPath)
        {
        }

        public DivertTargetValue() : base(null)
        {}

        public override Value Cast(ValueType newType)
        {
            if (newType == valueType)
                return this;
            
            throw new System.Exception ("Unexpected type cast of Value to new ValueType");
        }

        public override string ToString ()
        {
            return "DivertTargetValue(" + targetPath + ")";
        }
    }

    // TODO: Think: Erm, I get that this contains a string, but should
    // we really derive from Value<string>? That seems a bit misleading to me.
    internal class VariablePointerValue : Value<string>
    {
        public string variableName { get { return this.value; } set { this.value = value; } }
        public override ValueType valueType { get { return ValueType.VariablePointer; } }
        public override bool isTruthy { get { throw new System.Exception("Shouldn't be checking the truthiness of a variable pointer"); } }

        // Where the variable is located
        // -1 = default, unknown, yet to be determined
        // 0  = in global scope
        // 1+ = callstack element index + 1 (so that the first doesn't conflict with special global scope)
        public int contextIndex { get; set; }

        public VariablePointerValue(string variableName, int contextIndex = -1) : base(variableName)
        {
            this.contextIndex = contextIndex;
        }

        public VariablePointerValue() : this(null)
        {
        }

        public override Value Cast(ValueType newType)
        {
            if (newType == valueType)
                return this;

            throw new System.Exception ("Unexpected type cast of Value to new ValueType");
        }

        public override string ToString ()
        {
            return "VariablePointerValue(" + variableName + ")";
        }

        public override Object Copy()
        {
            return new VariablePointerValue (variableName, contextIndex);
        }
    }

    // Helper class purely to make it less unweildly to type Dictionary<string, int> all the time.
    internal class SetDictionary : Dictionary<string, int> {
        public SetDictionary () { }
        public SetDictionary (Dictionary<string, int> otherDict) : base (otherDict) { }

        public SetDictionary UnionWith (SetDictionary otherDict)
        {
            var union = new SetDictionary (this);
            foreach (var kv in otherDict)
                union.Add(kv.Key, kv.Value);
            return union;
        }

        public SetDictionary IntersectWith (SetDictionary otherDict)
        {
            var intersection = new SetDictionary ();
            foreach (var kv in this) {
                if (otherDict.ContainsKey (kv.Key))
                    intersection.Add (kv.Key, kv.Value);
            }
            return intersection;
        }

        public SetDictionary Without (SetDictionary setToRemove)
        {
            var result = new SetDictionary (this);
            foreach (var kv in setToRemove)
                result.Remove (kv.Key);
            return result;
        }
    }

    internal class SetValue : Value<SetDictionary>
    {
        public override ValueType valueType {
            get {
                return ValueType.Set;
            }
        }

        // Story has to set this so that the value knows its origin,
        // necessary for certain operations (e.g. interacting with ints)
        public Set singleOriginSet;

        // Runtime sets may reference items from different origin sets
        public string singleOriginSetName {
            get {
                string name = null;

                foreach (var fullNamedItem in value) {
                    var setName = fullNamedItem.Key.Split ('.') [0];

                    // First name - take it as the assumed single origin name
                    if (name == null)
                        name = setName;

                    // A different one than one we've already had? No longer
                    // single origin.
                    else if (name != setName)
                        return null;
                }

                return name;
            }
        }

        // Truthy if it contains any non-zero items
        public override bool isTruthy {
            get {
                foreach (var kv in value) {
                    int setItemIntValue = kv.Value;
                    if (setItemIntValue != 0)
                        return true;
                }
                return false;
            }
        }

        public KeyValuePair<string, int> maxItem {
            get {
                var max = new KeyValuePair<string, int>(null, int.MinValue);
                foreach (var kv in value) {
                    if (kv.Value > max.Value)
                        max = kv;
                }
                return max;
            }
        }
                
        public override Value Cast (ValueType newType)
        {
            if (newType == ValueType.Int) {
                var max = maxItem;
                if( max.Key == null )
                    return new IntValue (0);
                else
                    return new IntValue (max.Value);
            }

            else if (newType == ValueType.Float) {
                var max = maxItem;
                if (max.Key == null)
                    return new FloatValue (0.0f);
                else
                    return new FloatValue ((float)max.Value);
            }

            else if (newType == ValueType.String) {
                var max = maxItem;
                if (max.Key == null)
                    return new StringValue ("");
                else
                    return new StringValue (max.Key);
            }

            if (newType == valueType)
                return this;

            throw new System.Exception ("Unexpected type cast of Value to new ValueType");
        }

        public SetValue () : base(null) {
            value = new SetDictionary ();
        }

        public SetValue (SetDictionary dict) : base (null)
        {
            value = new SetDictionary (dict);
        }

        public SetValue (Dictionary<string, int> dict) : base (null)
        {
            value = new SetDictionary (dict);
        }

        public SetValue (string singleItemName, int singleValue) : base (null)
        {
            value = new SetDictionary {
                {singleItemName, singleValue}
            };
        }

        public override string ToString ()
        {
            var ordered =  new List<KeyValuePair<string, int>> ();
            ordered.AddRange (value);
            ordered.Sort((x, y) => x.Value.CompareTo(y.Value));

            var sb = new System.Text.StringBuilder ();
            for (int i = 0; i < ordered.Count; i++) {
                if (i > 0)
                    sb.Append (", ");
                
                var fullItemPath = ordered [i].Key;
                var nameParts = fullItemPath.Split ('.');
                var itemName = nameParts [nameParts.Length - 1];
                sb.Append (itemName);
            }

            return sb.ToString ();
        }
    }
        
}

