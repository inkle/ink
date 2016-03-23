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

        public static List<Runtime.Object> JArrayToRuntimeObjList(JArray jArray, bool skipLast=false)
        {
            int count = jArray.Count;
            if (skipLast)
                count--;

            var list = new List<Runtime.Object> (jArray.Count);

            for (int i = 0; i < count; i++) {
                var jTok = jArray [i];
                var runtimeObj = JTokenToRuntimeObject (jTok);
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

        // ----------------------
        // JSON ENCODING SCHEME
        // ----------------------
        //
        // Text:           "#The string"  "##The string that begins with a literal hash"
        //                 "\n"
        // 
        // Glue:           "<>", "G<", "G>"
        // 
        // ControlCommand: "ev", "out", "/ev", "du" "pop", "str", "/str", "nop", 
        //                 "choiceCnt", "turns", "visit", "seq", "thread", "done", "end"
        // 
        // NativeFunction: "+", "-", "/", "*", "%" "~", "==", ">", "<", ">=", "<=", "!=", "!"... etc
        // 
        // Pop:            "->->", "~ret"
        //
        // Void:           "void"
        // 
        // Literal:        "^literal string", "^^literal string beginning with ^"
        //                 5, 5.2
        //                 {"^->": "path.target"}
        //                 {"^var": "varname", "ci": 0}
        // 
        // Container:      [...]
        //                 [..., 
        //                     {
        //                         "subContainerName": ..., 
        //                         "#f": 5,                    // flags
        //                         "#n": "containerOwnName"    // only if not redundant
        //                     }
        //                 ]
        // 
        // Divert:         {"->": "path.target"}
        //                 {"->": "path.target", "var": true}
        //                 {"f()": "path.func"}
        //                 {"->t->": "path.tunnel"}
        //                 {"x()": "externalFuncName", "exArgs": 5}
        // 
        // Branch:         {"t?": divert if true }
        //                 {"f?": divert if false }
        // 
        // Var Assign:     {"VAR=": "varName", "re": true}   // reassignment
        //                 {"temp=": "varName"}
        // 
        // Var ref:        {"VAR?": "varName"}
        //                 {"CNT?": "stitch name"}
        // 
        // Choice:         {"*": pathString,
        //                  "flg": 18 }
        //
        //
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
                if (firstChar == '$')
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

                // Void
                if (str == "void")
                    return new Runtime.Void ();
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

                // Divert
                bool isDivert = false;
                bool pushesToStack = false;
                PushPopType divPushType = PushPopType.Function;
                bool external = false;
                if (obj.TryGetValue ("->", out propValue)) {
                    isDivert = true;
                }
                else if (obj.TryGetValue ("f()", out propValue)) {
                    isDivert = true;
                    pushesToStack = true;
                    divPushType = PushPopType.Function;
                }
                else if (obj.TryGetValue ("->t->", out propValue)) {
                    isDivert = true;
                    pushesToStack = true;
                    divPushType = PushPopType.Tunnel;
                }
                else if (obj.TryGetValue ("x()", out propValue)) {
                    isDivert = true;
                    external = true;
                    pushesToStack = false;
                    divPushType = PushPopType.Function;
                }
                if (isDivert) {
                    var divert = new Divert ();
                    divert.pushesToStack = pushesToStack;
                    divert.stackPushType = divPushType;
                    divert.isExternal = external;

                    string target = propValue.ToString ();

                    if (obj.TryGetValue ("var", out propValue))
                        divert.variableDivertName = target;
                    else
                        divert.targetPathString = target;

                    if (external) {
                        if (obj.TryGetValue ("exArgs", out propValue))
                            divert.externalArgs = propValue.ToObject<int> ();
                    }

                    return divert;
                }
                    
                // Choice
                if (obj.TryGetValue ("*", out propValue)) {
                    var choice = new Choice ();
                    choice.pathStringOnChoice = propValue.ToString();

                    if (obj.TryGetValue ("flg", out propValue))
                        choice.flags = propValue.ToObject<int>();

                    return choice;
                }

                // Variable reference
                if (obj.TryGetValue ("VAR?", out propValue)) {
                    return new VariableReference (propValue.ToString ());
                } else if (obj.TryGetValue ("CNT?", out propValue)) {
                    var readCountVarRef = new VariableReference ();
                    readCountVarRef.pathStringForCount = propValue.ToString ();
                    return readCountVarRef;
                }

                // Variable assignment
                bool isVarAss = false;
                bool isGlobalVar = false;
                if (obj.TryGetValue ("VAR=", out propValue)) {
                    isVarAss = true;
                    isGlobalVar = true;
                } else if (obj.TryGetValue ("temp=", out propValue)) {
                    isVarAss = true;
                    isGlobalVar = false;
                }
                if (isVarAss) {
                    var varName = propValue.ToString ();
                    var isNewDecl = !obj.TryGetValue("re", out propValue);
                    var varAss = new VariableAssignment (varName, isNewDecl);
                    varAss.isGlobal = isGlobalVar;
                    return varAss;
                }

                Divert trueDivert = null;
                Divert falseDivert = null;
                if (obj.TryGetValue ("t?", out propValue)) {
                    trueDivert = JTokenToRuntimeObject(propValue) as Divert;
                }
                if (obj.TryGetValue ("f?", out propValue)) {
                    falseDivert = JTokenToRuntimeObject(propValue) as Divert;
                }
                if (trueDivert || falseDivert) {
                    return new Branch (trueDivert, falseDivert);
                }
            }

            // Array is always a Runtime.Container
            if (token.Type == JTokenType.Array) {

                var jArray = (JArray)token;

                var container = new Container ();
                container.content = JArrayToRuntimeObjList (jArray, skipLast:true);

                // Final object in the array is always a combination of
                //  - named content
                //  - a "#" key with the countFlags
                // (if either exists at all, otherwise null)
                var terminatingObj = jArray [jArray.Count - 1] as JObject;
                if (terminatingObj != null) {

                    var namedOnlyContent = new Dictionary<string, Runtime.Object> (terminatingObj.Count);

                    foreach (var keyVal in terminatingObj) {
                        if (keyVal.Key == "#f") {
                            container.countFlags = keyVal.Value.ToObject<int> ();
                        } else if (keyVal.Key == "#n") {
                            container.name = keyVal.Value.ToString ();
                        } else {
                            var namedContentItem = JTokenToRuntimeObject(keyVal.Value);
                            var namedSubContainer = namedContentItem as Container;
                            if (namedSubContainer)
                                namedSubContainer.name = keyVal.Key;
                            namedOnlyContent [keyVal.Key] = namedContentItem;
                        }
                    }

                    container.namedOnlyContent = namedOnlyContent;
                }

                return container;
            }

            if (token.Type == JTokenType.Null)
                return null;

            throw new System.Exception ("Failed to convert token to runtime object: " + token);
        }

        public static JToken RuntimeObjectToJToken(Runtime.Object obj)
        {
            var container = obj as Container;
            if (container) {

                var jArray = ListToJArray (container.content);

                // Container is always an array [...]
                // But the final element is always either:
                //  - a dictionary containing the named content, as well as possibly
                //    the key "#" with the count flags
                //  - null, if neither of the above
                var namedOnlyContent = container.namedOnlyContent;
                var countFlags = container.countFlags;
                if (namedOnlyContent != null && namedOnlyContent.Count > 0 || countFlags > 0 || container.name != null) {

                    JObject terminatingObj;
                    if (namedOnlyContent != null) {
                        terminatingObj = DictionaryRuntimeObjsToJObject (namedOnlyContent);

                        // Strip redundant names from containers if necessary
                        foreach (var namedContentObj in terminatingObj) {
                            var subContainerJArray = namedContentObj.Value as JArray;
                            if (subContainerJArray != null) {
                                var attrJObj = subContainerJArray [subContainerJArray.Count - 1] as JObject;
                                if (attrJObj != null) {
                                    attrJObj.Remove ("#n");
                                    if (attrJObj.Count == 0)
                                        subContainerJArray [subContainerJArray.Count - 1] = null;
                                }
                            }
                        }

                    } else
                        terminatingObj = new JObject ();

                    if( countFlags > 0 )
                        terminatingObj ["#f"] = countFlags;

                    if( container.name != null )
                        terminatingObj ["#n"] = container.name;

                    jArray.Add (terminatingObj);
                } 

                // Add null terminator to indicate that there's no dictionary
                else {
                    jArray.Add (null);
                }

                return jArray;
            }

            var divert = obj as Divert;
            if (divert) {
                string divTypeKey = "->";
                if (divert.isExternal)
                    divTypeKey = "x()";
                else if (divert.pushesToStack) {
                    if (divert.stackPushType == PushPopType.Function)
                        divTypeKey = "f()";
                    else if (divert.stackPushType == PushPopType.Tunnel)
                        divTypeKey = "->t->";
                }

                string targetStr;
                if (divert.hasVariableTarget)
                    targetStr = divert.variableDivertName;
                else
                    targetStr = divert.targetPathString;

                var jObj = new JObject();
                jObj[divTypeKey] = targetStr;

                if (divert.hasVariableTarget)
                    jObj ["var"] = true;

                if (divert.externalArgs > 0)
                    jObj ["exArgs"] = divert.externalArgs;

                return jObj;
            }

            var choice = obj as Choice;
            if (choice) {
                var jObj = new JObject ();
                jObj ["*"] = choice.pathStringOnChoice;
                jObj ["flg"] = choice.flags;
                return jObj;
            }

            var litInt = obj as LiteralInt;
            if (litInt)
                return litInt.value;

            var litFloat = obj as LiteralFloat;
            if (litFloat)
                return litFloat.value;
            
            var litStr = obj as LiteralString;
            if (litStr)
                return "^" + litStr.value;

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
                    return "\n";
                else
                    return "$" + text.text;
            }

            var glue = obj as Runtime.Glue;
            if (glue) {
                if (glue.isBi)
                    return "<>";
                else if (glue.isLeft)
                    return "G<";
                else
                    return "G>";
            }

            var controlCmd = obj as ControlCommand;
            if (controlCmd) {
                return _controlCommandNames [(int)controlCmd.commandType];
            }

            var nativeFunc = obj as Runtime.NativeFunctionCall;
            if (nativeFunc)
                return nativeFunc.name;

            var pop = obj as Runtime.Pop;
            if (pop) {
                if (pop.type == PushPopType.Function)
                    return "~ret";
                else
                    return "->->";
            }

            // Variable reference
            var varRef = obj as VariableReference;
            if (varRef) {
                var jObj = new JObject ();
                string readCountPath = varRef.pathStringForCount;
                if (readCountPath != null) {
                    jObj ["CNT?"] = readCountPath;
                } else {
                    jObj ["VAR?"] = varRef.name;
                }

                return jObj;
            }

            // Variable assignment
            var varAss = obj as VariableAssignment;
            if (varAss) {
                string key = varAss.isGlobal ? "VAR=" : "temp=";
                var jObj = new JObject ();
                jObj [key] = varAss.variableName;

                // Reassignment?
                if (!varAss.isNewDeclaration)
                    jObj ["re"] = true;

                return jObj;
            }

            var branch = obj as Branch;
            if (branch) {
                var jObj = new JObject ();
                if (branch.trueDivert)
                    jObj ["t?"] = RuntimeObjectToJToken (branch.trueDivert);
                if (branch.falseDivert)
                    jObj ["f?"] = RuntimeObjectToJToken (branch.falseDivert);
                return jObj;
            }

            var voidObj = obj as Void;
            if (voidObj)
                return "void";

            throw new System.Exception ("Failed to convert runtime object to Json token: " + obj);
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


