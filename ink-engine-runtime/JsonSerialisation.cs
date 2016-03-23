using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Ink.Runtime
{
    internal static class Json
    {
        public static JArray ListToJArray(List<Runtime.Object> serialisables)
        {
            var jArray = new JArray ();
            foreach (var s in serialisables) {
                jArray.Add (RuntimeObjectToJToken(s));
            }
            return jArray;
        }

        public static List<Runtime.Object> JArrayToRuntimeObjList(JArray jArray)
        {
            var list = new List<Runtime.Object> (jArray.Count);
            foreach (var jTok in jArray) {
                var runtimeObj = JTokenToRuntimeObject (jTok);;
                list.Add (runtimeObj);
            }
            return list;
        }

        public static JObject DictionaryRuntimeObjsToJObject(Dictionary<string, Runtime.Object> dictionary)
        {
            var jsonObj = new JObject ();

            foreach (var keyVal in dictionary) {
                var runtimeObj = keyVal.Value as Runtime.Object;
                if (runtimeObj != null)
                    jsonObj [keyVal.Key] = RuntimeObjectToJToken(runtimeObj);
            }

            return jsonObj;
        }

        public static Dictionary<string, Runtime.Object> JObjectToDictionaryRuntimeObjs(JObject jObject)
        {
            var dict = new Dictionary<string, Runtime.Object> (jObject.Count);

            foreach (var keyVal in jObject) {
                dict [keyVal.Key] = JTokenToRuntimeObject(keyVal.Value);
            }

            return dict;
        }

        public static Runtime.Object JTokenToRuntimeObject(JToken token)
        {
            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float) {
                return Literal.Create (((JValue)token).Value);
            }
            
            if (token.Type == JTokenType.String) {
                string str = token.ToString ();

                // Literal string
                char firstChar = str[0];
                if (firstChar == '^')
                    return new LiteralString (str.Substring (1));

                // Runtime text
                if (firstChar == '#')
                    return new Runtime.Text (str.Substring (1));
                if (firstChar == '\n' && str.Length == 1)
                    return new Runtime.Text ("\n");

                // Glue
                if (str == "<>")
                    return new Runtime.Glue (GlueType.Bidirectional);
                else if(str == "G<")
                    return new Runtime.Glue (GlueType.Left);
                else if(str == "G>")
                    return new Runtime.Glue (GlueType.Right);

                // Control commands (would looking up in a hash set be faster?)
                for (int i = 0; i < _controlCommandNames.Length; ++i) {
                    string cmdName = _controlCommandNames [i];
                    if (str == cmdName) {
                        return new Runtime.ControlCommand ((ControlCommand.CommandType)i);
                    }
                }

                // Native functions
                if( NativeFunctionCall.CallExistsWithName(str) )
                    return NativeFunctionCall.CallWithName (str);

                // Pop
                if (str == "->->")
                    return new Runtime.Pop (PushPopType.Tunnel);
                else if (str == "~ret")
                    return new Runtime.Pop (PushPopType.Function);

            }

            if (token.Type == JTokenType.Object) {

                JObject obj = (JObject)token;
                JToken propValue;

                // Literal divert target to path
                if (obj.TryGetValue ("^->", out propValue))
                    return new LiteralDivertTarget (new Path (propValue.ToString()));

                // LiteralVariablePointer
                if (obj.TryGetValue ("^var", out propValue)) {
                    var varPtr = new LiteralVariablePointer (propValue.ToString ());
                    if (obj.TryGetValue ("ci", out propValue))
                        varPtr.contextIndex = propValue.ToObject<int> ();
                    return varPtr;
                }
            }

            throw new System.Exception ("Failed to convert token to runtime object: " + token);
        }

        public static JToken RuntimeObjectToJToken(Runtime.Object obj)
        {
            var litInt = obj as LiteralInt;
            if (litInt)
                return new JValue (litInt.value);

            var litFloat = obj as LiteralFloat;
            if (litFloat)
                return new JValue (litFloat.value);
            
            var litStr = obj as LiteralString;
            if (litStr)
                return new JValue("^" + litStr.value);

            var litDivTarget = obj as LiteralDivertTarget;
            if (litDivTarget)
                return new JObject (
                    new JProperty("^->", litDivTarget.value.componentsString)
                );

            var litVarPtr = obj as LiteralVariablePointer;
            if (litVarPtr)
                return new JObject (
                    new JProperty("^var", litVarPtr.value),
                    new JProperty("ci", litVarPtr.contextIndex)
                );

            var text = obj as Runtime.Text;
            if (text) {
                if (text.isNewline)
                    return new JValue ("\n");
                else
                    return new JValue ("#" + text.text);
            }

            var glue = obj as Runtime.Glue;
            if (glue) {
                if (glue.isBi)
                    return new JValue ("<>");
                else if (glue.isLeft)
                    return new JValue ("G<");
                else
                    return new JValue ("G>");
            }

            var nativeFunc = obj as Runtime.NativeFunctionCall;
            if (nativeFunc)
                return new JValue(nativeFunc.name);

            var pop = obj as Runtime.Pop;
            if (pop) {
                if (pop.type == PushPopType.Function)
                    return new JValue ("~ret");
                else
                    return new JValue ("->->");
            }

            throw new System.Exception ("Failed to runtime object to token: " + obj);
        }

        static Json() 
        {
            _controlCommandNames = new string[(int)ControlCommand.CommandType.TOTAL_VALUES];

            _controlCommandNames [(int)ControlCommand.CommandType.EvalStart] = "ev";
            _controlCommandNames [(int)ControlCommand.CommandType.EvalOutput] = "out";
            _controlCommandNames [(int)ControlCommand.CommandType.EvalEnd] = "/ev";
            _controlCommandNames [(int)ControlCommand.CommandType.Duplicate] = "du";
            _controlCommandNames [(int)ControlCommand.CommandType.PopEvaluatedValue] = "pop";
            _controlCommandNames [(int)ControlCommand.CommandType.BeginString] = "str";
            _controlCommandNames [(int)ControlCommand.CommandType.EndString] = "/str";
            _controlCommandNames [(int)ControlCommand.CommandType.NoOp] = "nop";
            _controlCommandNames [(int)ControlCommand.CommandType.ChoiceCount] = "choiceCnt";
            _controlCommandNames [(int)ControlCommand.CommandType.TurnsSince] = "turns";
            _controlCommandNames [(int)ControlCommand.CommandType.VisitIndex] = "visit";
            _controlCommandNames [(int)ControlCommand.CommandType.SequenceShuffleIndex] = "seq";
            _controlCommandNames [(int)ControlCommand.CommandType.StartThread] = "thread";
            _controlCommandNames [(int)ControlCommand.CommandType.Done] = "done";
            _controlCommandNames [(int)ControlCommand.CommandType.End] = "end";

            for (int i = 0; i < (int)ControlCommand.CommandType.TOTAL_VALUES; ++i) {
                if (_controlCommandNames [i] == null)
                    throw new System.Exception ("Control command not accounted for in serialisation");
            }
        }

        static string[] _controlCommandNames;
    }
}


