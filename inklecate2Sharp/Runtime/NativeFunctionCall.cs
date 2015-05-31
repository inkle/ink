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

        public static NativeFunctionCall CallWithName(string functionName)
        {
            GenerateNativeFunctionsIfNecessary ();

            return _nativeFunctions [functionName];
        }

        public string name { get; protected set; }
        public int numberOfParamters { get; protected set; }

        public Runtime.Object Call(List<Runtime.Object> parameters)
        {
            Debug.Assert (_binaryOp != null || _unaryOp != null, "No function implemention defined?");

            // TODO: Handle various other types other than Number (int)
            Number param1 = (Number) parameters [0];

            int result = 0;
            if (_binaryOp != null) {
                Number param2 = (Number) parameters [1];
                result = _binaryOp (param1.value, param2.value);
            } else if (_unaryOp != null) {
                result = _unaryOp (param1.value);
            }

            return new Number (result);
        }

        NativeFunctionCall (string name, int numberOfParamters)
        {
            this.name = name;
            this.numberOfParamters = numberOfParamters;
        }

        NativeFunctionCall (string name, int numberOfParamters, BinaryOp binaryOp) : this(name, numberOfParamters)
        {
            _binaryOp = binaryOp;
        }

        NativeFunctionCall (string name, int numberOfParamters, UnaryOp unaryOp) : this(name, numberOfParamters)
        {
            _unaryOp = unaryOp;
        }
            
        static void GenerateNativeFunctionsIfNecessary()
        {
            if (_nativeFunctions == null) {
                _nativeFunctions = new Dictionary<string, NativeFunctionCall> ();

                AddBinaryOp(Add,      (x, y) => (int)x + (int)y);
                AddBinaryOp(Subtract, (x, y) => (int)x - (int)y);
                AddBinaryOp(Multiply, (x, y) => (int)x * (int)y);
                AddBinaryOp(Divide,   (x, y) => (int)x / (int)y);
                AddUnaryOp (Negate,   x => -(int)x); 

                AddBinaryOp(Equal,    (x, y) => x == y ? 1 : 0);
                AddBinaryOp(Greater,  (x, y) => x > y  ? 1 : 0);
                AddBinaryOp(Less,     (x, y) => x < y  ? 1 : 0);
                AddBinaryOp(GreaterThanOrEquals, (x, y) => x >= y ? 1 : 0);
                AddBinaryOp(LessThanOrEquals, (x, y) => x <= y ? 1 : 0);
                AddBinaryOp(NotEquals, (x, y) => x != y ? 1 : 0);
                AddUnaryOp (Not,       x => (x == 0) ? 1 : 0); 
            }
        }

        static void AddBinaryOp(string name, BinaryOp op)
        {
            var f = new NativeFunctionCall (name, 2, op);
            _nativeFunctions [name] = f;
        }

        static void AddUnaryOp(string name, UnaryOp op)
        {
            var f = new NativeFunctionCall (name, 1, op);
            _nativeFunctions [name] = f;
        }

        public override string ToString ()
        {
            return "Native '" + name + "'";
        }

        delegate int BinaryOp(int left, int right);
        delegate int UnaryOp(int val);

        BinaryOp _binaryOp;
        UnaryOp _unaryOp;

        static Dictionary<string, NativeFunctionCall> _nativeFunctions;

    }
}

