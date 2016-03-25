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

        List<Value> CoerceValuesToSingleType(List<Runtime.Object> parametersIn)
        {
            ValueType valType = ValueType.Int;

            // Find out what the output type is
            // "higher level" types infect both so that binary operations
            // use the same type on both sides. e.g. binary operation of
            // int and float causes the int to be casted to a float.
            foreach (var obj in parametersIn) {
                var val = (Value)obj;
                if (val.valueType > valType) {
                    valType = val.valueType;
                }
            }

            // Coerce to this chosen type
            var parametersOut = new List<Value> ();
            foreach (Value val in parametersIn) {
                var castedValue = val.Cast (valType);
                parametersOut.Add (castedValue);
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

                // Special case: The only operation you can do on divert target values
                BinaryOp<Path> divertTargetsEqual = (Path d1, Path d2) => {
                    return d1.Equals(d2) ? 1 : 0;
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

        static void AddFloatUnaryOp(string name, UnaryOp<int> op)
        {
            AddOpToNativeFunc (name, 1, ValueType.Int, op);
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

