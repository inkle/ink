using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Inklewriter.Runtime
{
    public class NativeFunctionCall : Runtime.Object
    {
        public const string Add      = "+";
        public const string Subtract = "-";
        public const string Divide   = "/";
        public const string Multiply = "*";
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
        public const string AndWord  = "and";
        public const string OrWord   = "or";

        public static NativeFunctionCall CallWithName(string functionName)
        {
            GenerateNativeFunctionsIfNecessary ();

            return _nativeFunctions [functionName];
        }

        public string name { get; protected set; }
        public int numberOfParamters { get; protected set; }

        public Runtime.Object Call(List<Runtime.Object> parameters)
        {
            //Debug.Assert (_binaryOp != null || _unaryOp != null, "No function implemention defined?");

            var coercedParams = CoerceLiteralsToSingleType (parameters);

            Literal param1 = (Literal) coercedParams [0];
            LiteralType chosenType = param1.literalType;

            object opForType = null;

            Literal result = null;

            if (parameters.Count == 2) {
                opForType = _binaryOps [chosenType];

                if (chosenType == LiteralType.Int) {
                    var int1 = (LiteralInt)param1;
                    var int2 = (LiteralInt)coercedParams [1];
                    var intOp = (BinaryOp<int>)opForType;
                    int intResult = intOp (int1.value, int2.value);
                    result = new LiteralInt (intResult);
                }
            } else if (parameters.Count == 1) {
                opForType = _unaryOps [chosenType];

                if (chosenType == LiteralType.Int) {
                    var int1 = (LiteralInt)param1;
                    var intOp = (UnaryOp<int>)opForType;
                    int intResult = intOp (int1.value);
                    result = new LiteralInt (intResult);
                }
            } 

            else {
                throw new System.Exception ("Unexpected number of parameters to NativeFunctionCall: " + parameters.Count);
            }

            return result;
        }



        List<Literal> CoerceLiteralsToSingleType(List<Runtime.Object> parametersIn)
        {
            LiteralType litType = LiteralType.Int;

            // Find out what the output type is
            // "higher level" types infect both so that binary operations
            // use the same type on both sides. e.g. binary operation of
            // int and float causes the int to be casted to a float.
            foreach (var obj in parametersIn) {
                var literal = (Literal)obj;
                if (literal.literalType == LiteralType.Float) {
                    litType = LiteralType.Float;
                }
            }

            // Coerce to this chosen type
            var parametersOut = new List<Literal> ();
            foreach (Literal literal in parametersIn) {
                var castedLiteral = literal.Cast (litType);
                parametersOut.Add (castedLiteral);
            }

            return parametersOut;
        }

        NativeFunctionCall (string name, int numberOfParamters)
        {
            this.name = name;
            this.numberOfParamters = numberOfParamters;
        }
            
        static void GenerateNativeFunctionsIfNecessary()
        {
            if (_nativeFunctions == null) {
                _nativeFunctions = new Dictionary<string, NativeFunctionCall> ();

                AddIntBinaryOp(Add,      (x, y) => x + y);
                AddIntBinaryOp(Subtract, (x, y) => x - y);
                AddIntBinaryOp(Multiply, (x, y) => x * y);
                AddIntBinaryOp(Divide,   (x, y) => x / y);
                AddIntUnaryOp (Negate,   x => -x); 

                AddIntBinaryOp(Equal,    (x, y) => x == y ? 1 : 0);
                AddIntBinaryOp(Greater,  (x, y) => x > y  ? 1 : 0);
                AddIntBinaryOp(Less,     (x, y) => x < y  ? 1 : 0);
                AddIntBinaryOp(GreaterThanOrEquals, (x, y) => x >= y ? 1 : 0);
                AddIntBinaryOp(LessThanOrEquals, (x, y) => x <= y ? 1 : 0);
                AddIntBinaryOp(NotEquals, (x, y) => x != y ? 1 : 0);
                AddIntUnaryOp (Not,       x => (x == 0) ? 1 : 0); 

                AddIntBinaryOp(And,      (x, y) => x != 0 && y != 0 ? 1 : 0);
                AddIntBinaryOp(AndWord,  (x, y) => x != 0 && y != 0 ? 1 : 0);
                AddIntBinaryOp(Or,       (x, y) => x != 0 || y != 0 ? 1 : 0);
                AddIntBinaryOp(OrWord,   (x, y) => x != 0 || y != 0 ? 1 : 0);

            }
        }

        void AddBinaryOp<T>(LiteralType litType, BinaryOp<T> op)
        {
            if (_binaryOps == null) {
                _binaryOps = new Dictionary<LiteralType, object> ();
            }

            _binaryOps [litType] = op;
        }

        void AddUnaryOp<T>(LiteralType litType, UnaryOp<T> op)
        {
            if (_unaryOps == null) {
                _unaryOps = new Dictionary<LiteralType, object> ();
            }

            _unaryOps [litType] = op;
        }

        static void AddIntBinaryOp(string name, BinaryOp<int> op)
        {
            NativeFunctionCall nativeFunc = null;
            if (!_nativeFunctions.TryGetValue (name, out nativeFunc)) {
                nativeFunc = new NativeFunctionCall (name, 2);
                _nativeFunctions [name] = nativeFunc;
            }

            nativeFunc.AddBinaryOp(LiteralType.Int, op);
        }

        static void AddIntUnaryOp(string name, UnaryOp<int> op)
        {
            NativeFunctionCall nativeFunc = null;
            if (!_nativeFunctions.TryGetValue (name, out nativeFunc)) {
                nativeFunc = new NativeFunctionCall (name, 1);
                _nativeFunctions [name] = nativeFunc;
            }

            nativeFunc.AddUnaryOp(LiteralType.Int, op);
        }

        public override string ToString ()
        {
            return "Native '" + name + "'";
        }

        delegate int BinaryOp<T>(T left, T right);
        delegate int UnaryOp<T>(T val);

        // Operations for each data type, for a single operation (e.g. "+")
        Dictionary<LiteralType, object> _binaryOps;
        Dictionary<LiteralType, object> _unaryOps;

        static Dictionary<string, NativeFunctionCall> _nativeFunctions;

    }
}

