using System;
using System.Collections.Generic;
using System.Linq;

namespace Ink.Runtime
{
    public static class Json
    {
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

        public static void WriteDictionaryRuntimeObjs(SimpleJson.Writer writer, Dictionary<string, Runtime.Object> dictionary) 
        {
            writer.WriteObjectStart();
            foreach(var keyVal in dictionary) {
                writer.WritePropertyStart(keyVal.Key);
                WriteRuntimeObject(writer, keyVal.Value);
                writer.WritePropertyEnd();
            }
            writer.WriteObjectEnd();
        }


        public static void WriteListRuntimeObjs(SimpleJson.Writer writer, List<Runtime.Object> list)
        {
            writer.WriteArrayStart();
            foreach (var val in list)
            {
                WriteRuntimeObject(writer, val);
            }
            writer.WriteArrayEnd();
        }

        public static void WriteIntDictionary(SimpleJson.Writer writer, Dictionary<string, int> dict)
        {
            writer.WriteObjectStart();
            foreach (var keyVal in dict)
                writer.WriteProperty(keyVal.Key, keyVal.Value);
            writer.WriteObjectEnd();
        }

        public static void WriteRuntimeObject(SimpleJson.Writer writer, Runtime.Object obj)
        {
            var container = obj as Container;
            if (container) {
                WriteRuntimeContainer(writer, container);
                return;
            }

            var divert = obj as Divert;
            if (divert)
            {
                string divTypeKey = "->";
                if (divert.isExternal)
                    divTypeKey = "x()";
                else if (divert.pushesToStack)
                {
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

                writer.WriteObjectStart();

                writer.WriteProperty(divTypeKey, targetStr);

                if (divert.hasVariableTarget)
                    writer.WriteProperty("var", true);

                if (divert.isConditional)
                    writer.WriteProperty("c", true);

                if (divert.externalArgs > 0)
                    writer.WriteProperty("exArgs", divert.externalArgs);

                writer.WriteObjectEnd();
                return;
            }

            var choicePoint = obj as ChoicePoint;
            if (choicePoint)
            {
                writer.WriteObjectStart();
                writer.WriteProperty("*", choicePoint.pathStringOnChoice);
                writer.WriteProperty("flg", choicePoint.flags);
                writer.WriteObjectEnd();
                return;
            }

            var boolVal = obj as BoolValue;
            if (boolVal) {
                writer.Write(boolVal.value);
                return;
            }

            var intVal = obj as IntValue;
            if (intVal) {
                writer.Write(intVal.value);
                return;
            }

            var floatVal = obj as FloatValue;
            if (floatVal) {
                writer.Write(floatVal.value);
                return;
            }

            var strVal = obj as StringValue;
            if (strVal)
            {
                if (strVal.isNewline)
                    writer.Write("\\n", escape:false);
                else {
                    writer.WriteStringStart();
                    writer.WriteStringInner("^");
                    writer.WriteStringInner(strVal.value);
                    writer.WriteStringEnd();
                }
                return;
            }

            var listVal = obj as ListValue;
            if (listVal)
            {
                WriteInkList(writer, listVal);
                return;
            }

            var divTargetVal = obj as DivertTargetValue;
            if (divTargetVal)
            {
                writer.WriteObjectStart();
                writer.WriteProperty("^->", divTargetVal.value.componentsString);
                writer.WriteObjectEnd();
                return;
            }

            var varPtrVal = obj as VariablePointerValue;
            if (varPtrVal)
            {
                writer.WriteObjectStart();
                writer.WriteProperty("^var", varPtrVal.value);
                writer.WriteProperty("ci", varPtrVal.contextIndex);
                writer.WriteObjectEnd();
                return;
            }

            var glue = obj as Runtime.Glue;
            if (glue) {
                writer.Write("<>");
                return;
            }

            var controlCmd = obj as ControlCommand;
            if (controlCmd)
            {
                writer.Write(_controlCommandNames[(int)controlCmd.commandType]);
                return;
            }

            var nativeFunc = obj as Runtime.NativeFunctionCall;
            if (nativeFunc)
            {
                var name = nativeFunc.name;

                // Avoid collision with ^ used to indicate a string
                if (name == "^") name = "L^";

                writer.Write(name);
                return;
            }


            // Variable reference
            var varRef = obj as VariableReference;
            if (varRef)
            {
                writer.WriteObjectStart();

                string readCountPath = varRef.pathStringForCount;
                if (readCountPath != null)
                {
                    writer.WriteProperty("CNT?", readCountPath);
                }
                else
                {
                    writer.WriteProperty("VAR?", varRef.name);
                }

                writer.WriteObjectEnd();
                return;
            }

            // Variable assignment
            var varAss = obj as VariableAssignment;
            if (varAss)
            {
                writer.WriteObjectStart();

                string key = varAss.isGlobal ? "VAR=" : "temp=";
                writer.WriteProperty(key, varAss.variableName);

                // Reassignment?
                if (!varAss.isNewDeclaration)
                    writer.WriteProperty("re", true);

                writer.WriteObjectEnd();

                return;
            }

            // Void
            var voidObj = obj as Void;
            if (voidObj) {
                writer.Write("void");
                return;
            }

            // Legacy tag
            var tag = obj as Tag;
            if (tag)
            {
                writer.WriteObjectStart();
                writer.WriteProperty("#", tag.text);
                writer.WriteObjectEnd();
                return;
            }

            // Used when serialising save state only
            var choice = obj as Choice;
            if (choice) {
                WriteChoice(writer, choice);
                return;
            }

            throw new System.Exception("Failed to write runtime object to JSON: " + obj);
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
            if (token is int || token is float || token is bool) {
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
                if (str == "<>") return new Runtime.Glue ();

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

                // Legacy Tag with text
                if (obj.TryGetValue ("#", out propValue)) {
                    return new Runtime.Tag((string)propValue);
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

        public static void WriteRuntimeContainer(SimpleJson.Writer writer, Container container, bool withoutName = false)
        {
            writer.WriteArrayStart();

            foreach (var c in container.content)
                WriteRuntimeObject(writer, c);

            // Container is always an array [...]
            // But the final element is always either:
            //  - a dictionary containing the named content, as well as possibly
            //    the key "#" with the count flags
            //  - null, if neither of the above
            var namedOnlyContent = container.namedOnlyContent;
            var countFlags = container.countFlags;
            var hasNameProperty = container.name != null && !withoutName;

            bool hasTerminator = namedOnlyContent != null || countFlags > 0 || hasNameProperty;

            if( hasTerminator )
                writer.WriteObjectStart();

            if ( namedOnlyContent != null ) {
                foreach(var namedContent in namedOnlyContent) {
                    var name = namedContent.Key;
                    var namedContainer = namedContent.Value as Container;
                    writer.WritePropertyStart(name);
                    WriteRuntimeContainer(writer, namedContainer, withoutName:true);
                    writer.WritePropertyEnd();
                }
            }

            if (countFlags > 0)
                writer.WriteProperty("#f", countFlags);

            if (hasNameProperty)
                writer.WriteProperty("#n", container.name);

            if (hasTerminator)
                writer.WriteObjectEnd();
            else
                writer.WriteNull();

            writer.WriteArrayEnd();
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
            choice.sourcePath = jObj ["originalChoicePath"].ToString();
            choice.originalThreadIndex = (int)jObj ["originalThreadIndex"];
            choice.pathStringOnChoice = jObj ["targetPath"].ToString();
            choice.tags = JArrayToTags(jObj, choice);
            return choice;
        }

        private static List<string> JArrayToTags(Dictionary<string, object> jObj, Choice choice)
        {
            if (!jObj.TryGetValue("tags", out object jArray)) return null;
            
            var tags = new List<string>();
            foreach (var stringValue in (List<object>)jArray)
            {
                tags.Add(stringValue.ToString());
            }
            return tags;
        }

        public static void WriteChoice(SimpleJson.Writer writer, Choice choice)
        {
            writer.WriteObjectStart();
            writer.WriteProperty("text", choice.text);
            writer.WriteProperty("index", choice.index);
            writer.WriteProperty("originalChoicePath", choice.sourcePath);
            writer.WriteProperty("originalThreadIndex", choice.originalThreadIndex);
            writer.WriteProperty("targetPath", choice.pathStringOnChoice);
            WriteChoiceTags(writer, choice);
            writer.WriteObjectEnd();
        }

        private static void WriteChoiceTags(SimpleJson.Writer writer, Choice choice)
        {
            if (choice.tags == null || choice.tags.Count == 0) return;
            writer.WritePropertyStart("tags");
            writer.WriteArrayStart();
            foreach (var tag in choice.tags)
            {
                writer.Write(tag);
            }

            writer.WriteArrayEnd();
            writer.WritePropertyEnd();
        }

        static void WriteInkList(SimpleJson.Writer writer, ListValue listVal)
        {
            var rawList = listVal.value;

            writer.WriteObjectStart();

            writer.WritePropertyStart("list");

            writer.WriteObjectStart();

            foreach (var itemAndValue in rawList)
            {
                var item = itemAndValue.Key;
                int itemVal = itemAndValue.Value;

                writer.WritePropertyNameStart();
                writer.WritePropertyNameInner(item.originName ?? "?");
                writer.WritePropertyNameInner(".");
                writer.WritePropertyNameInner(item.itemName);
                writer.WritePropertyNameEnd();

                writer.Write(itemVal);

                writer.WritePropertyEnd();
            }

            writer.WriteObjectEnd();

            writer.WritePropertyEnd();

            if (rawList.Count == 0 && rawList.originNames != null && rawList.originNames.Count > 0)
            {
                writer.WritePropertyStart("origins");
                writer.WriteArrayStart();
                foreach (var name in rawList.originNames)
                    writer.Write(name);
                writer.WriteArrayEnd();
                writer.WritePropertyEnd();
            }

            writer.WriteObjectEnd();
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
            _controlCommandNames [(int)ControlCommand.CommandType.Turns] = "turn";
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
            _controlCommandNames [(int)ControlCommand.CommandType.ListRandom] = "lrnd";
            _controlCommandNames [(int)ControlCommand.CommandType.BeginTag] = "#";
            _controlCommandNames [(int)ControlCommand.CommandType.EndTag] = "/#";

            for (int i = 0; i < (int)ControlCommand.CommandType.TOTAL_VALUES; ++i) {
                if (_controlCommandNames [i] == null)
                    throw new System.Exception ("Control command not accounted for in serialisation");
            }
        }

        static string[] _controlCommandNames;
    }
}


