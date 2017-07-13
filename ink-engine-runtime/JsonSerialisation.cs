using System;
using System.Collections.Generic;
using System.Linq;

namespace Ink.Runtime
{
    internal static class Json
    {
        public static List<object> ListToJArray<T>(List<T> serialisables) where T : Runtime.Object
        {
            var jArray = new List<object> ();
            foreach (var s in serialisables) {
                jArray.Add (RuntimeObjectToJToken(s));
            }
            return jArray;
        }

        public static List<T> JArrayToRuntimeObjList<T>(List<object> jArray, bool skipLast=false) where T : Runtime.Object
        {
            int count = jArray.Count;
            if (skipLast)
                count--;

            var list = new List<T> (jArray.Count);

            for (int i = 0; i < count; i++) {
                var jTok = jArray [i];
                var runtimeObj = JTokenToRuntimeObject (jTok) as T;
                list.Add (runtimeObj);
            }

            return list;
        }

        public static List<Runtime.Object> JArrayToRuntimeObjList(List<object> jArray, bool skipLast=false)
        {
            return JArrayToRuntimeObjList<Runtime.Object> (jArray, skipLast);
        }

        public static Dictionary<string, object> DictionaryRuntimeObjsToJObject(Dictionary<string, Runtime.Object> dictionary)
        {
            var jsonObj = new Dictionary<string, object> ();

            foreach (var keyVal in dictionary) {
                var runtimeObj = keyVal.Value as Runtime.Object;
                if (runtimeObj != null)
                    jsonObj [keyVal.Key] = RuntimeObjectToJToken(runtimeObj);
            }

            return jsonObj;
        }

        public static Dictionary<string, Runtime.Object> JObjectToDictionaryRuntimeObjs(Dictionary<string, object> jObject)
        {
            var dict = new Dictionary<string, Runtime.Object> (jObject.Count);

            foreach (var keyVal in jObject) {
                dict [keyVal.Key] = JTokenToRuntimeObject(keyVal.Value);
            }

            return dict;
        }

        public static Dictionary<string, int> JObjectToIntDictionary(Dictionary<string, object> jObject)
        {
            var dict = new Dictionary<string, int> (jObject.Count);
            foreach (var keyVal in jObject) {
                dict [keyVal.Key] = (int)keyVal.Value;
            }
            return dict;
        }

        public static Dictionary<string, object> IntDictionaryToJObject(Dictionary<string, int> dict)
        {
            var jObj = new Dictionary<string, object> ();
            foreach (var keyVal in dict) {
                jObj [keyVal.Key] = keyVal.Value;
            }
            return jObj;
        }

        // ----------------------
        // JSON ENCODING SCHEME
        // ----------------------
        //
        // Glue:           "<>", "G<", "G>"
        // 
        // ControlCommand: "ev", "out", "/ev", "du" "pop", "->->", "~ret", "str", "/str", "nop", 
        //                 "choiceCnt", "turns", "visit", "seq", "thread", "done", "end"
        // 
        // NativeFunction: "+", "-", "/", "*", "%" "~", "==", ">", "<", ">=", "<=", "!=", "!"... etc
        // 
        // Void:           "void"
        // 
        // Value:          "^string value", "^^string value beginning with ^"
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
        // Divert:         {"->": "path.target", "c": true }
        //                 {"->": "path.target", "var": true}
        //                 {"f()": "path.func"}
        //                 {"->t->": "path.tunnel"}
        //                 {"x()": "externalFuncName", "exArgs": 5}
        // 
        // Var Assign:     {"VAR=": "varName", "re": true}   // reassignment
        //                 {"temp=": "varName"}
        // 
        // Var ref:        {"VAR?": "varName"}
        //                 {"CNT?": "stitch name"}
        // 
        // ChoicePoint:    {"*": pathString,
        //                  "flg": 18 }
        //
        // Choice:         Nothing too clever, it's only used in the save state,
        //                 there's not likely to be many of them.
        // 
        // Tag:            {"#": "the tag text"}
        public static Runtime.Object JTokenToRuntimeObject(object token)
        {
            if (token is int || token is float) {
                return Value.Create (token);
            }
            
            if (token is string) {
                string str = (string)token;

                // String value
                char firstChar = str[0];
                if (firstChar == '^')
                    return new StringValue (str.Substring (1));
                else if( firstChar == '\n' && str.Length == 1)
                    return new StringValue ("\n");

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
                // "^" conflicts with the way to identify strings, so now
                // we know it's not a string, we can convert back to the proper
                // symbol for the operator.
                if (str == "L^") str = "^";
                if( NativeFunctionCall.CallExistsWithName(str) )
                    return NativeFunctionCall.CallWithName (str);

                // Pop
                if (str == "->->")
                    return Runtime.ControlCommand.PopTunnel ();
                else if (str == "~ret")
                    return Runtime.ControlCommand.PopFunction ();

                // Void
                if (str == "void")
                    return new Runtime.Void ();
            }

            if (token is Dictionary<string, object>) {

                var obj = (Dictionary < string, object> )token;
                object propValue;

                // Divert target value to path
                if (obj.TryGetValue ("^->", out propValue))
                    return new DivertTargetValue (new Path ((string)propValue));

                // VariablePointerValue
                if (obj.TryGetValue ("^var", out propValue)) {
                    var varPtr = new VariablePointerValue ((string)propValue);
                    if (obj.TryGetValue ("ci", out propValue))
                        varPtr.contextIndex = (int)propValue;
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

                    divert.isConditional = obj.TryGetValue("c", out propValue);

                    if (external) {
                        if (obj.TryGetValue ("exArgs", out propValue))
                            divert.externalArgs = (int)propValue;
                    }

                    return divert;
                }
                    
                // Choice
                if (obj.TryGetValue ("*", out propValue)) {
                    var choice = new ChoicePoint ();
                    choice.pathStringOnChoice = propValue.ToString();

                    if (obj.TryGetValue ("flg", out propValue))
                        choice.flags = (int)propValue;

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

                // Tag
                if (obj.TryGetValue ("#", out propValue)) {
                    return new Runtime.Tag ((string)propValue);
                }

                // List value
                if (obj.TryGetValue ("list", out propValue)) {
                    var listContent = (Dictionary<string, object>)propValue;
                    var rawList = new InkList ();
                    if (obj.TryGetValue ("origins", out propValue)) {
                        var namesAsObjs = (List<object>)propValue;
                        rawList.SetInitialOriginNames (namesAsObjs.Cast<string>().ToList());
                    }
                    foreach (var nameToVal in listContent) {
                        var item = new InkListItem (nameToVal.Key);
                        var val = (int)nameToVal.Value;
                        rawList.Add (item, val);
                    }
                    return new ListValue (rawList);
                }

                // Used when serialising save state only
                if (obj ["originalChoicePath"] != null)
                    return JObjectToChoice (obj);
            }

            // Array is always a Runtime.Container
            if (token is List<object>) {
                return JArrayToContainer((List<object>)token);
            }

            if (token == null)
                return null;

            throw new System.Exception ("Failed to convert token to runtime object: " + token);
        }

        public static object RuntimeObjectToJToken(Runtime.Object obj)
        {
            var container = obj as Container;
            if (container) {
                return ContainerToJArray (container);
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

                var jObj = new Dictionary<string, object> ();
                jObj[divTypeKey] = targetStr;

                if (divert.hasVariableTarget)
                    jObj ["var"] = true;

                if (divert.isConditional)
                    jObj ["c"] = true;

                if (divert.externalArgs > 0)
                    jObj ["exArgs"] = divert.externalArgs;

                return jObj;
            }

            var choicePoint = obj as ChoicePoint;
            if (choicePoint) {
                var jObj = new Dictionary<string, object> ();
                jObj ["*"] = choicePoint.pathStringOnChoice;
                jObj ["flg"] = choicePoint.flags;
                return jObj;
            }

            var intVal = obj as IntValue;
            if (intVal)
                return intVal.value;

            var floatVal = obj as FloatValue;
            if (floatVal)
                return floatVal.value;
            
            var strVal = obj as StringValue;
            if (strVal) {
                if (strVal.isNewline)
                    return "\n";
                else
                    return "^" + strVal.value;
            }

            var listVal = obj as ListValue;
            if (listVal) {
                return InkListToJObject (listVal);
            }

            var divTargetVal = obj as DivertTargetValue;
            if (divTargetVal) {
                var divTargetJsonObj = new Dictionary<string, object> ();
                divTargetJsonObj ["^->"] = divTargetVal.value.componentsString;
                return divTargetJsonObj;
            }

            var varPtrVal = obj as VariablePointerValue;
            if (varPtrVal) {
                var varPtrJsonObj = new Dictionary<string, object> ();
                varPtrJsonObj ["^var"] = varPtrVal.value;
                varPtrJsonObj ["ci"] = varPtrVal.contextIndex;
                return varPtrJsonObj;
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
            if (nativeFunc) {
                var name = nativeFunc.name;

                // Avoid collision with ^ used to indicate a string
                if (name == "^") name = "L^";
                return name;
            }


            // Variable reference
            var varRef = obj as VariableReference;
            if (varRef) {
                var jObj = new Dictionary<string, object> ();
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
                var jObj = new Dictionary<string, object> ();
                jObj [key] = varAss.variableName;

                // Reassignment?
                if (!varAss.isNewDeclaration)
                    jObj ["re"] = true;

                return jObj;
            }
                
            // Void
            var voidObj = obj as Void;
            if (voidObj)
                return "void";

            // Tag
            var tag = obj as Tag;
            if (tag) {
                var jObj = new Dictionary<string, object> ();
                jObj ["#"] = tag.text;
                return jObj;
            }

            // Used when serialising save state only
            var choice = obj as Choice;
            if (choice)
                return ChoiceToJObject (choice);

            throw new System.Exception ("Failed to convert runtime object to Json token: " + obj);
        }

        static List<object> ContainerToJArray(Container container)
        {
            var jArray = ListToJArray (container.content);

            // Container is always an array [...]
            // But the final element is always either:
            //  - a dictionary containing the named content, as well as possibly
            //    the key "#" with the count flags
            //  - null, if neither of the above
            var namedOnlyContent = container.namedOnlyContent;
            var countFlags = container.countFlags;
            if (namedOnlyContent != null && namedOnlyContent.Count > 0 || countFlags > 0 || container.name != null) {

                Dictionary<string, object> terminatingObj;
                if (namedOnlyContent != null) {
                    terminatingObj = DictionaryRuntimeObjsToJObject (namedOnlyContent);

                    // Strip redundant names from containers if necessary
                    foreach (var namedContentObj in terminatingObj) {
                        var subContainerJArray = namedContentObj.Value as List<object>;
                        if (subContainerJArray != null) {
                            var attrJObj = subContainerJArray [subContainerJArray.Count - 1] as Dictionary<string, object>;
                            if (attrJObj != null) {
                                attrJObj.Remove ("#n");
                                if (attrJObj.Count == 0)
                                    subContainerJArray [subContainerJArray.Count - 1] = null;
                            }
                        }
                    }

                } else
                    terminatingObj = new Dictionary<string, object> ();

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

        static Container JArrayToContainer(List<object> jArray)
        {
            var container = new Container ();
            container.content = JArrayToRuntimeObjList (jArray, skipLast:true);

            // Final object in the array is always a combination of
            //  - named content
            //  - a "#f" key with the countFlags
            // (if either exists at all, otherwise null)
            var terminatingObj = jArray [jArray.Count - 1] as Dictionary<string, object>;
            if (terminatingObj != null) {

                var namedOnlyContent = new Dictionary<string, Runtime.Object> (terminatingObj.Count);

                foreach (var keyVal in terminatingObj) {
                    if (keyVal.Key == "#f") {
                        container.countFlags = (int)keyVal.Value;
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

        static Choice JObjectToChoice(Dictionary<string, object> jObj)
        {
            var choice = new Choice();
            choice.text = jObj ["text"].ToString();
            choice.index = (int)jObj ["index"];
            choice.originalChoicePath = jObj ["originalChoicePath"].ToString();
            choice.originalThreadIndex = (int)jObj ["originalThreadIndex"];
            return choice;
        }

        static Dictionary<string, object> ChoiceToJObject(Choice choice)
        {
            var jObj = new Dictionary<string, object> ();
            jObj ["text"] = choice.text;
            jObj ["index"] = choice.index;
            jObj ["originalChoicePath"] = choice.originalChoicePath;
            jObj ["originalThreadIndex"] = choice.originalThreadIndex;
            return jObj;
        }

        static Dictionary<string, object> InkListToJObject (ListValue listVal)
        {
            var rawList = listVal.value;

            var dict = new Dictionary<string, object> ();

            var content = new Dictionary<string, object> ();

            foreach (var itemAndValue in rawList) {
                var item = itemAndValue.Key;
                int val = itemAndValue.Value;
                content [item.ToString ()] = val;
            }

            dict ["list"] = content;

            if (rawList.Count == 0 && rawList.originNames != null && rawList.originNames.Count > 0) {
                dict ["origins"] = rawList.originNames.Cast<object> ().ToList ();
            }

            return dict;
        }

        public static Dictionary<string, object> ListDefinitionsToJToken (ListDefinitionsOrigin origin)
        {
            var result = new Dictionary<string, object> ();
            foreach (ListDefinition def in origin.lists) {
                var listDefJson = new Dictionary<string, object> ();
                foreach (var itemToVal in def.items) {
                    InkListItem item = itemToVal.Key;
                    int val = itemToVal.Value;
                    listDefJson [item.itemName] = (object)val;
                }
                result [def.name] = listDefJson;
            }
            return result;
        }

        public static ListDefinitionsOrigin JTokenToListDefinitions (object obj)
        {
            var defsObj = (Dictionary<string, object>)obj;

            var allDefs = new List<ListDefinition> ();

            foreach (var kv in defsObj) {
                var name = (string) kv.Key;
                var listDefJson = (Dictionary<string, object>)kv.Value;

                // Cast (string, object) to (string, int) for items
                var items = new Dictionary<string, int> ();
                foreach (var nameValue in listDefJson)
                    items.Add(nameValue.Key, (int)nameValue.Value);

                var def = new ListDefinition (name, items);
                allDefs.Add (def);
            }

            return new ListDefinitionsOrigin (allDefs);
        }

        static Json() 
        {
            _controlCommandNames = new string[(int)ControlCommand.CommandType.TOTAL_VALUES];

            _controlCommandNames [(int)ControlCommand.CommandType.EvalStart] = "ev";
            _controlCommandNames [(int)ControlCommand.CommandType.EvalOutput] = "out";
            _controlCommandNames [(int)ControlCommand.CommandType.EvalEnd] = "/ev";
            _controlCommandNames [(int)ControlCommand.CommandType.Duplicate] = "du";
            _controlCommandNames [(int)ControlCommand.CommandType.PopEvaluatedValue] = "pop";
            _controlCommandNames [(int)ControlCommand.CommandType.PopFunction] = "~ret";
            _controlCommandNames [(int)ControlCommand.CommandType.PopTunnel] = "->->";
            _controlCommandNames [(int)ControlCommand.CommandType.BeginString] = "str";
            _controlCommandNames [(int)ControlCommand.CommandType.EndString] = "/str";
            _controlCommandNames [(int)ControlCommand.CommandType.NoOp] = "nop";
            _controlCommandNames [(int)ControlCommand.CommandType.ChoiceCount] = "choiceCnt";
            _controlCommandNames [(int)ControlCommand.CommandType.TurnsSince] = "turns";
            _controlCommandNames [(int)ControlCommand.CommandType.ReadCount] = "readc";
            _controlCommandNames [(int)ControlCommand.CommandType.Random] = "rnd";
            _controlCommandNames [(int)ControlCommand.CommandType.SeedRandom] = "srnd";
            _controlCommandNames [(int)ControlCommand.CommandType.VisitIndex] = "visit";
            _controlCommandNames [(int)ControlCommand.CommandType.SequenceShuffleIndex] = "seq";
            _controlCommandNames [(int)ControlCommand.CommandType.StartThread] = "thread";
            _controlCommandNames [(int)ControlCommand.CommandType.Done] = "done";
            _controlCommandNames [(int)ControlCommand.CommandType.End] = "end";
            _controlCommandNames [(int)ControlCommand.CommandType.ListFromInt] = "listInt";
            _controlCommandNames [(int)ControlCommand.CommandType.ListRange] = "range";

            for (int i = 0; i < (int)ControlCommand.CommandType.TOTAL_VALUES; ++i) {
                if (_controlCommandNames [i] == null)
                    throw new System.Exception ("Control command not accounted for in serialisation");
            }
        }

        static string[] _controlCommandNames;
    }
}


