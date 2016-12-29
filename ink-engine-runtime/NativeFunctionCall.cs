using System;
using System.Collections.Generic;

namespace Ink.Runtime
{
    internal class NativeFunctionCall : Runtime.Object
    {
        public const string Add      = "+";
        public const string Subtract = "-";
        public const string Divide   = "/";
        public const string Multiply = "*";
        public const string Mod      = "%";
        public const string Negate   = "~";

        public const string Equal    = "==";
        public const string Greater  = ">";
        public const string Less     = "<";
        public const string GreaterThanOrEquals = ">=";
        public const string LessThanOrEquals = "<=";
        public const string NotEquals   = "!=";
        public const string Not      = "!";

        public const string And      = "&&";
        public const string Or       = "||";

        public const string Min      = "MIN";
        public const string Max      = "MAX";

        public static NativeFunctionCall CallWithName(string functionName)
        {
            return new NativeFunctionCall (functionName);
        }

        public static bool CallExistsWithName(string functionName)
        {
            GenerateNativeFunctionsIfNecessary ();
            return _nativeFunctions.ContainsKey (functionName);
        }
            
        public string name { 
            get { 
                return _name;
            } 
            protected set {
                _name = value;
                if( !_isPrototype )
                    _prototype = _nativeFunctions [_name];
            }
        }
        string _name;

        public int numberOfParameters { 
            get {
                if (_prototype) {
                    return _prototype.numberOfParameters;
                } else {
                    return _numberOfParameters;
                }
            }
            protected set {
                _numberOfParameters = value;
            }
        }

        int _numberOfParameters;

        public Runtime.Object Call(List<Runtime.Object> parameters)
        {
            if (_prototype) {
                return _prototype.Call(parameters);
            }

            if (numberOfParameters != parameters.Count) {
                throw new System.Exception ("Unexpected number of parameters");
            }

            foreach (var p in parameters) {
                if (p is Void)
                    throw new StoryException ("Attempting to perform operation on a void value. Did you forget to 'return' a value from a function you called here?");
            }

            // Special case: Set-Int operation returns a Set (e.g. "alpha" + 1 = "beta")
            if (parameters.Count == 2 && parameters [0] is SetValue && parameters [1] is IntValue)
                return CallSetIntOperation (parameters);

            var coercedParams = CoerceValuesToSingleType (parameters);
            ValueType coercedType = coercedParams[0].valueType;

            if (coercedType == ValueType.Int) {
                return Call<int> (coercedParams);
            } else if (coercedType == ValueType.Float) {
                return Call<float> (coercedParams);
            } else if (coercedType == ValueType.String) {
                return Call<string> (coercedParams);
            } else if (coercedType == ValueType.DivertTarget) {
                return Call<Path> (coercedParams);
            } else if (coercedType == ValueType.Set) {
                return Call<SetDictionary> (coercedParams);
            }

            return null;
        }

        Value Call<T>(List<Value> parametersOfSingleType)
        {
            Value param1 = (Value) parametersOfSingleType [0];
            ValueType valType = param1.valueType;

            var val1 = (Value<T>)param1;

            int paramCount = parametersOfSingleType.Count;

            if (paramCount == 2 || paramCount == 1) {

                object opForTypeObj = null;
                if (!_operationFuncs.TryGetValue (valType, out opForTypeObj)) {
                    throw new StoryException ("Can not perform operation '"+this.name+"' on "+valType);
                }

                // Binary
                if (paramCount == 2) {
                    Value param2 = (Value) parametersOfSingleType [1];

                    var val2 = (Value<T>)param2;

                    var opForType = (BinaryOp<T>)opForTypeObj;

                    // Return value unknown until it's evaluated
                    object resultVal = opForType (val1.value, val2.value);

                    return Value.Create (resultVal);
                } 

                // Unary
                else {

                    var opForType = (UnaryOp<T>)opForTypeObj;

                    var resultVal = opForType (val1.value);

                    return Value.Create (resultVal);
                }  
            }
                
            else {
                throw new System.Exception ("Unexpected number of parameters to NativeFunctionCall: " + parametersOfSingleType.Count);
            }
        }

        Value CallSetIntOperation (List<Runtime.Object> setIntParams)
        {
            var setVal = (SetValue)setIntParams [0];
            var intVal = (IntValue)setIntParams [1];

            var coercedInts = new List<Value> {
                    new IntValue(setVal.maxItem.Value),
                    intVal
                };
            var intResult = (IntValue)Call<int> (coercedInts);

            string newItemName;
            var originSet = setVal.singleOriginSet;
            if (originSet != null && originSet.TryGetItemWithValue (intResult.value, out newItemName))
                newItemName = originSet.name + "." + newItemName;
            else
                newItemName = "UNKNOWN";
            
            return new SetValue (newItemName, intResult.value);
        }

        List<Value> CoerceValuesToSingleType(List<Runtime.Object> parametersIn)
        {
            ValueType valType = ValueType.Int;

            SetValue specialCaseSet = null;

            // Find out what the output type is
            // "higher level" types infect both so that binary operations
            // use the same type on both sides. e.g. binary operation of
            // int and float causes the int to be casted to a float.
            foreach (var obj in parametersIn) {
                var val = (Value)obj;
                if (val.valueType > valType) {
                    valType = val.valueType;
                }

                if (val.valueType == ValueType.Set) {
                    specialCaseSet = val as SetValue;
                }
            }

            // Coerce to this chosen type
            var parametersOut = new List<Value> ();

            // Special case: Coercing to Ints to Sets
            // We have to do it early when we have both parameters
            // to hand - so that we can make use of the Set's origin
            if (valType == ValueType.Set) {
                
                foreach (Value val in parametersIn) {
                    if (val.valueType == ValueType.Set) {
                        parametersOut.Add (val);
                    } else if (val.valueType == ValueType.Int) {
                        int intVal = (int)val.valueObject;
                        var set = specialCaseSet.singleOriginSet;
                        if (set == null)
                            throw new StoryException ("Cannot mix Set and Int values here because the existing Set appears to contain items from a mixture of different Set definitions. How do we know which Set is the Int referring to?");
                        
                        string itemName;
                        if (set.TryGetItemWithValue (intVal, out itemName)) {
                            var castedValue = new SetValue (set.name + "." + itemName, intVal);
                            parametersOut.Add (castedValue);
                        } else
                            throw new StoryException ("Could not find Set item with the value " + intVal + " in " + set.name);
                    } else
                        throw new StoryException ("Cannot mix Sets and " + val.valueType + " values in this operation");
                }
                
            } 

            // Normal Coercing (with standard casting)
            else {
                foreach (Value val in parametersIn) {
                    var castedValue = val.Cast (valType);
                    parametersOut.Add (castedValue);
                }
            }

            return parametersOut;
        }

        public NativeFunctionCall(string name)
        {
            GenerateNativeFunctionsIfNecessary ();

            this.name = name;
        }

        // Require default constructor for serialisation
        public NativeFunctionCall() { 
            GenerateNativeFunctionsIfNecessary ();
        }

        // Only called internally to generate prototypes
        NativeFunctionCall (string name, int numberOfParamters)
        {
            _isPrototype = true;
            this.name = name;
            this.numberOfParameters = numberOfParamters;
        }
            
        static void GenerateNativeFunctionsIfNecessary()
        {
            if (_nativeFunctions == null) {
                _nativeFunctions = new Dictionary<string, NativeFunctionCall> ();

                // Int operations
                AddIntBinaryOp(Add,      (x, y) => x + y);
                AddIntBinaryOp(Subtract, (x, y) => x - y);
                AddIntBinaryOp(Multiply, (x, y) => x * y);
                AddIntBinaryOp(Divide,   (x, y) => x / y);
                AddIntBinaryOp(Mod,      (x, y) => x % y); 
                AddIntUnaryOp (Negate,   x => -x); 

                AddIntBinaryOp(Equal,    (x, y) => x == y ? 1 : 0);
                AddIntBinaryOp(Greater,  (x, y) => x > y  ? 1 : 0);
                AddIntBinaryOp(Less,     (x, y) => x < y  ? 1 : 0);
                AddIntBinaryOp(GreaterThanOrEquals, (x, y) => x >= y ? 1 : 0);
                AddIntBinaryOp(LessThanOrEquals, (x, y) => x <= y ? 1 : 0);
                AddIntBinaryOp(NotEquals, (x, y) => x != y ? 1 : 0);
                AddIntUnaryOp (Not,       x => (x == 0) ? 1 : 0); 

                AddIntBinaryOp(And,      (x, y) => x != 0 && y != 0 ? 1 : 0);
                AddIntBinaryOp(Or,       (x, y) => x != 0 || y != 0 ? 1 : 0);

                AddIntBinaryOp(Max,      (x, y) => Math.Max(x, y));
                AddIntBinaryOp(Min,      (x, y) => Math.Min(x, y));

                // Float operations
                AddFloatBinaryOp(Add,      (x, y) => x + y);
                AddFloatBinaryOp(Subtract, (x, y) => x - y);
                AddFloatBinaryOp(Multiply, (x, y) => x * y);
                AddFloatBinaryOp(Divide,   (x, y) => x / y);
                AddFloatBinaryOp(Mod,      (x, y) => x % y); // TODO: Is this the operation we want for floats?
                AddFloatUnaryOp (Negate,   x => -x); 

                AddFloatBinaryOp(Equal,    (x, y) => x == y ? (int)1 : (int)0);
                AddFloatBinaryOp(Greater,  (x, y) => x > y  ? (int)1 : (int)0);
                AddFloatBinaryOp(Less,     (x, y) => x < y  ? (int)1 : (int)0);
                AddFloatBinaryOp(GreaterThanOrEquals, (x, y) => x >= y ? (int)1 : (int)0);
                AddFloatBinaryOp(LessThanOrEquals, (x, y) => x <= y ? (int)1 : (int)0);
                AddFloatBinaryOp(NotEquals, (x, y) => x != y ? (int)1 : (int)0);
                AddFloatUnaryOp (Not,       x => (x == 0.0f) ? (int)1 : (int)0); 

                AddFloatBinaryOp(And,      (x, y) => x != 0.0f && y != 0.0f ? (int)1 : (int)0);
                AddFloatBinaryOp(Or,       (x, y) => x != 0.0f || y != 0.0f ? (int)1 : (int)0);

                AddFloatBinaryOp(Max,      (x, y) => Math.Max(x, y));
                AddFloatBinaryOp(Min,      (x, y) => Math.Min(x, y));

                // String operations
                AddStringBinaryOp(Add,     (x, y) => x + y); // concat
                AddStringBinaryOp(Equal,   (x, y) => x.Equals(y) ? (int)1 : (int)0);

                // Set operations
                AddSetBinaryOp (Add, (x, y) => x.UnionWith (y));
                AddSetBinaryOp (Subtract, (x, y) => x.Without(y));
                //AddSetBinaryOp (Multiply, (x, y) => x * y);
                //AddSetBinaryOp (Divide, (x, y) => x / y);
                //AddSetBinaryOp (Mod, (x, y) => x % y); // TODO: Is this the operation we want for floats?
                //AddSetUnaryOp (Negate, x => -x);

                //AddSetBinaryOp (Equal, (x, y) => x == y ? (int)1 : (int)0);
                //AddSetBinaryOp (Greater, (x, y) => x > y ? (int)1 : (int)0);
                //AddSetBinaryOp (Less, (x, y) => x < y ? (int)1 : (int)0);
                //AddSetBinaryOp (GreaterThanOrEquals, (x, y) => x >= y ? (int)1 : (int)0);
                //AddSetBinaryOp (LessThanOrEquals, (x, y) => x <= y ? (int)1 : (int)0);
                //AddSetBinaryOp (NotEquals, (x, y) => x != y ? (int)1 : (int)0);
                //AddSetUnaryOp (Not, x => (x == 0.0f) ? (int)1 : (int)0);

                AddSetBinaryOp (And, (x, y) => x.IntersectWith(y));
                AddSetBinaryOp (Or, (x, y) => x.UnionWith (y));

                //AddSetBinaryOp (Max, (x, y) => Math.Max (x, y));
                //AddSetBinaryOp (Min, (x, y) => Math.Min (x, y));

                // Special case: The only operation you can do on divert target values
                BinaryOp<Path> divertTargetsEqual = (Path d1, Path d2) => {
                    return d1.Equals (d2) ? 1 : 0;
                };
                AddOpToNativeFunc (Equal, 2, ValueType.DivertTarget, divertTargetsEqual);

            }
        }

        void AddOpFuncForType(ValueType valType, object op)
        {
            if (_operationFuncs == null) {
                _operationFuncs = new Dictionary<ValueType, object> ();
            }

            _operationFuncs [valType] = op;
        }

        static void AddOpToNativeFunc(string name, int args, ValueType valType, object op)
        {
            NativeFunctionCall nativeFunc = null;
            if (!_nativeFunctions.TryGetValue (name, out nativeFunc)) {
                nativeFunc = new NativeFunctionCall (name, args);
                _nativeFunctions [name] = nativeFunc;
            }

            nativeFunc.AddOpFuncForType (valType, op);
        }

        static void AddIntBinaryOp(string name, BinaryOp<int> op)
        {
            AddOpToNativeFunc (name, 2, ValueType.Int, op);
        }

        static void AddIntUnaryOp(string name, UnaryOp<int> op)
        {
            AddOpToNativeFunc (name, 1, ValueType.Int, op);
        }

        static void AddFloatBinaryOp(string name, BinaryOp<float> op)
        {
            AddOpToNativeFunc (name, 2, ValueType.Float, op);
        }

        static void AddStringBinaryOp(string name, BinaryOp<string> op)
        {
            AddOpToNativeFunc (name, 2, ValueType.String, op);
        }

        static void AddSetBinaryOp (string name, BinaryOp<SetDictionary> op)
        {
            AddOpToNativeFunc (name, 2, ValueType.Set, op);
        }

        static void AddFloatUnaryOp(string name, UnaryOp<float> op)
        {
            AddOpToNativeFunc (name, 1, ValueType.Float, op);
        }

        public override string ToString ()
        {
            return "Native '" + name + "'";
        }

        delegate object BinaryOp<T>(T left, T right);
        delegate object UnaryOp<T>(T val);

        NativeFunctionCall _prototype;
        bool _isPrototype;

        // Operations for each data type, for a single operation (e.g. "+")
        Dictionary<ValueType, object> _operationFuncs;

        static Dictionary<string, NativeFunctionCall> _nativeFunctions;

    }
}

