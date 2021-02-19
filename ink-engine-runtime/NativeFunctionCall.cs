using System;
using System.Collections.Generic;

namespace Ink.Runtime
{
    public class NativeFunctionCall : Runtime.Object
    {
        public const string Add      = "+";
        public const string Subtract = "-";
        public const string Divide   = "/";
        public const string Multiply = "*";
        public const string Mod      = "%";
        public const string Negate   = "_"; // distinguish from "-" for subtraction

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

        public const string Pow      = "POW";
        public const string Floor    = "FLOOR";
        public const string Ceiling  = "CEILING";
        public const string Int      = "INT";
        public const string Float    = "FLOAT";

        public const string Has      = "?";
        public const string Hasnt    = "!?";
        public const string Intersect = "^";

        public const string ListMin   = "LIST_MIN";
        public const string ListMax   = "LIST_MAX";
        public const string All       = "LIST_ALL";
        public const string Count     = "LIST_COUNT";
        public const string ValueOfList = "LIST_VALUE";
        public const string Invert    = "LIST_INVERT";

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

            bool hasList = false;
            foreach (var p in parameters) {
                if (p is Void)
                    throw new StoryException ("Attempting to perform operation on a void value. Did you forget to 'return' a value from a function you called here?");
                if (p is ListValue)
                    hasList = true;
            }

            // Binary operations on lists are treated outside of the standard coerscion rules
            if( parameters.Count == 2 && hasList )
                return CallBinaryListOperation (parameters);

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
            } else if (coercedType == ValueType.List) {
                return Call<InkList> (coercedParams);
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
                    throw new StoryException ("Cannot perform operation '"+this.name+"' on "+valType);
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

        Value CallBinaryListOperation (List<Runtime.Object> parameters)
        {
            // List-Int addition/subtraction returns a List (e.g. "alpha" + 1 = "beta")
            if ((name == "+" || name == "-") && parameters [0] is ListValue && parameters [1] is IntValue)
                return CallListIncrementOperation (parameters);

            var v1 = parameters [0] as Value;
            var v2 = parameters [1] as Value;

            // And/or with any other type requires coerscion to bool (int)
            if ((name == "&&" || name == "||") && (v1.valueType != ValueType.List || v2.valueType != ValueType.List)) {
                var op = _operationFuncs [ValueType.Int] as BinaryOp<int>;
                var result = (bool)op (v1.isTruthy ? 1 : 0, v2.isTruthy ? 1 : 0);
                return new BoolValue (result);
            }

            // Normal (list • list) operation
            if (v1.valueType == ValueType.List && v2.valueType == ValueType.List)
                return Call<InkList> (new List<Value> { v1, v2 });

            throw new StoryException ("Can not call use '" + name + "' operation on " + v1.valueType + " and " + v2.valueType);
        }

        Value CallListIncrementOperation (List<Runtime.Object> listIntParams)
        {
            var listVal = (ListValue)listIntParams [0];
            var intVal = (IntValue)listIntParams [1];


            var resultRawList = new InkList ();

            foreach (var listItemWithValue in listVal.value) {
                var listItem = listItemWithValue.Key;
                var listItemValue = listItemWithValue.Value;

                // Find + or - operation
                var intOp = (BinaryOp<int>)_operationFuncs [ValueType.Int];

                // Return value unknown until it's evaluated
                int targetInt = (int) intOp (listItemValue, intVal.value);

                // Find this item's origin (linear search should be ok, should be short haha)
                ListDefinition itemOrigin = null;
                foreach (var origin in listVal.value.origins) {
                    if (origin.name == listItem.originName) {
                        itemOrigin = origin;
                        break;
                    }
                }
                if (itemOrigin != null) {
                    InkListItem incrementedItem;
                    if (itemOrigin.TryGetItemWithValue (targetInt, out incrementedItem))
                        resultRawList.Add (incrementedItem, targetInt);
                }
            }

            return new ListValue (resultRawList);
        }

        List<Value> CoerceValuesToSingleType(List<Runtime.Object> parametersIn)
        {
            ValueType valType = ValueType.Int;

            ListValue specialCaseList = null;

            // Find out what the output type is
            // "higher level" types infect both so that binary operations
            // use the same type on both sides. e.g. binary operation of
            // int and float causes the int to be casted to a float.
            foreach (var obj in parametersIn) {
                var val = (Value)obj;
                if (val.valueType > valType) {
                    valType = val.valueType;
                }

                if (val.valueType == ValueType.List) {
                    specialCaseList = val as ListValue;
                }
            }

            // Coerce to this chosen type
            var parametersOut = new List<Value> ();

            // Special case: Coercing to Ints to Lists
            // We have to do it early when we have both parameters
            // to hand - so that we can make use of the List's origin
            if (valType == ValueType.List) {
                
                foreach (Value val in parametersIn) {
                    if (val.valueType == ValueType.List) {
                        parametersOut.Add (val);
                    } else if (val.valueType == ValueType.Int) {
                        int intVal = (int)val.valueObject;
                        var list = specialCaseList.value.originOfMaxItem;

                        InkListItem item;
                        if (list.TryGetItemWithValue (intVal, out item)) {
                            var castedValue = new ListValue (item, intVal);
                            parametersOut.Add (castedValue);
                        } else
                            throw new StoryException ("Could not find List item with the value " + intVal + " in " + list.name);
                    } else
                        throw new StoryException ("Cannot mix Lists and " + val.valueType + " values in this operation");
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
        NativeFunctionCall (string name, int numberOfParameters)
        {
            _isPrototype = true;
            this.name = name;
            this.numberOfParameters = numberOfParameters;
        }

        // For defining operations that do nothing to the specific type
        // (but are still supported), such as floor/ceil on int and float
        // cast on float.
        static object Identity<T>(T t) {
            return t;
        }

        static void GenerateNativeFunctionsIfNecessary()
        {
            if (_nativeFunctions == null) {
                _nativeFunctions = new Dictionary<string, NativeFunctionCall> ();

                // Why no bool operations?
                // Before evaluation, all bools are coerced to ints in
                // CoerceValuesToSingleType (see default value for valType at top).
                // So, no operations are ever directly done in bools themselves.
                // This also means that 1 == true works, since true is always converted
                // to 1 first.
                // However, many operations return a "native" bool (equals, etc).

                // Int operations
                AddIntBinaryOp(Add,      (x, y) => x + y);
                AddIntBinaryOp(Subtract, (x, y) => x - y);
                AddIntBinaryOp(Multiply, (x, y) => x * y);
                AddIntBinaryOp(Divide,   (x, y) => x / y);
                AddIntBinaryOp(Mod,      (x, y) => x % y); 
                AddIntUnaryOp (Negate,   x => -x); 

                AddIntBinaryOp(Equal,    (x, y) => x == y);
                AddIntBinaryOp(Greater,  (x, y) => x > y);
                AddIntBinaryOp(Less,     (x, y) => x < y);
                AddIntBinaryOp(GreaterThanOrEquals, (x, y) => x >= y);
                AddIntBinaryOp(LessThanOrEquals, (x, y) => x <= y);
                AddIntBinaryOp(NotEquals, (x, y) => x != y);
                AddIntUnaryOp (Not,       x => x == 0); 

                AddIntBinaryOp(And,      (x, y) => x != 0 && y != 0);
                AddIntBinaryOp(Or,       (x, y) => x != 0 || y != 0);

                AddIntBinaryOp(Max,      (x, y) => Math.Max(x, y));
                AddIntBinaryOp(Min,      (x, y) => Math.Min(x, y));

                // Have to cast to float since you could do POW(2, -1)
                AddIntBinaryOp (Pow,      (x, y) => (float) Math.Pow(x, y));
                AddIntUnaryOp(Floor,      Identity);
                AddIntUnaryOp(Ceiling,    Identity);
                AddIntUnaryOp(Int,        Identity);
                AddIntUnaryOp (Float,     x => (float)x);

                // Float operations
                AddFloatBinaryOp(Add,      (x, y) => x + y);
                AddFloatBinaryOp(Subtract, (x, y) => x - y);
                AddFloatBinaryOp(Multiply, (x, y) => x * y);
                AddFloatBinaryOp(Divide,   (x, y) => x / y);
                AddFloatBinaryOp(Mod,      (x, y) => x % y); // TODO: Is this the operation we want for floats?
                AddFloatUnaryOp (Negate,   x => -x); 

                AddFloatBinaryOp(Equal,    (x, y) => x == y);
                AddFloatBinaryOp(Greater,  (x, y) => x > y);
                AddFloatBinaryOp(Less,     (x, y) => x < y);
                AddFloatBinaryOp(GreaterThanOrEquals, (x, y) => x >= y);
                AddFloatBinaryOp(LessThanOrEquals, (x, y) => x <= y);
                AddFloatBinaryOp(NotEquals, (x, y) => x != y);
                AddFloatUnaryOp (Not,       x => (x == 0.0f)); 

                AddFloatBinaryOp(And,      (x, y) => x != 0.0f && y != 0.0f);
                AddFloatBinaryOp(Or,       (x, y) => x != 0.0f || y != 0.0f);

                AddFloatBinaryOp(Max,      (x, y) => Math.Max(x, y));
                AddFloatBinaryOp(Min,      (x, y) => Math.Min(x, y));

                AddFloatBinaryOp (Pow,      (x, y) => (float)Math.Pow(x, y));
                AddFloatUnaryOp(Floor,      x => (float)Math.Floor(x));
                AddFloatUnaryOp(Ceiling,    x => (float)Math.Ceiling(x));
                AddFloatUnaryOp(Int,        x => (int)x);
                AddFloatUnaryOp(Float,      Identity);

                // String operations
                AddStringBinaryOp(Add,     (x, y) => x + y); // concat
                AddStringBinaryOp(Equal,   (x, y) => x.Equals(y));
                AddStringBinaryOp (NotEquals, (x, y) => !x.Equals (y));
                AddStringBinaryOp (Has,    (x, y) => x.Contains(y));
                AddStringBinaryOp (Hasnt,   (x, y) => !x.Contains(y));

                // List operations
                AddListBinaryOp (Add, (x, y) => x.Union (y));
                AddListBinaryOp (Subtract, (x, y) => x.Without(y));
                AddListBinaryOp (Has, (x, y) => x.Contains (y));
                AddListBinaryOp (Hasnt, (x, y) => !x.Contains (y));
                AddListBinaryOp (Intersect, (x, y) => x.Intersect (y));

                AddListBinaryOp (Equal, (x, y) => x.Equals(y));
                AddListBinaryOp (Greater, (x, y) => x.GreaterThan(y));
                AddListBinaryOp (Less, (x, y) => x.LessThan(y));
                AddListBinaryOp (GreaterThanOrEquals, (x, y) => x.GreaterThanOrEquals(y));
                AddListBinaryOp (LessThanOrEquals, (x, y) => x.LessThanOrEquals(y));
                AddListBinaryOp (NotEquals, (x, y) => !x.Equals(y));

                AddListBinaryOp (And, (x, y) => x.Count > 0 && y.Count > 0);
                AddListBinaryOp (Or,  (x, y) => x.Count > 0 || y.Count > 0);

                AddListUnaryOp (Not, x => x.Count == 0 ? (int)1 : (int)0);

                // Placeholders to ensure that these special case functions can exist,
                // since these function is never actually run, and is special cased in Call
                AddListUnaryOp (Invert, x => x.inverse);
                AddListUnaryOp (All, x => x.all);
                AddListUnaryOp (ListMin, (x) => x.MinAsList());
                AddListUnaryOp (ListMax, (x) => x.MaxAsList());
                AddListUnaryOp (Count,  (x) => x.Count);
                AddListUnaryOp (ValueOfList,  (x) => x.maxItem.Value);

                // Special case: The only operations you can do on divert target values
                BinaryOp<Path> divertTargetsEqual = (Path d1, Path d2) => {
                    return d1.Equals (d2);
                };
                BinaryOp<Path> divertTargetsNotEqual = (Path d1, Path d2) => {
                	return !d1.Equals (d2);
                };
                AddOpToNativeFunc (Equal, 2, ValueType.DivertTarget, divertTargetsEqual);
                AddOpToNativeFunc (NotEquals, 2, ValueType.DivertTarget, divertTargetsNotEqual);

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

        static void AddListBinaryOp (string name, BinaryOp<InkList> op)
        {
            AddOpToNativeFunc (name, 2, ValueType.List, op);
        }

        static void AddListUnaryOp (string name, UnaryOp<InkList> op)
        {
            AddOpToNativeFunc (name, 1, ValueType.List, op);
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

