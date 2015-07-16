using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inklewriter.Runtime
{
    public class ObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //return objectType.IsSubclassOf (typeof(Runtime.Object));
            return objectType.IsSubclassOf(typeof(Runtime.Literal));
        }

        public override object ReadJson(JsonReader reader, 
            Type objectType, 
            object existingValue, 
            JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            var runtimeObjTypeName = jObject.Value<string> ("%t");
            var newObj = Create (runtimeObjTypeName);

            // Populate the object properties
            serializer.Populate (jObject.CreateReader (), newObj);

            return newObj;
        }

        public override void WriteJson(JsonWriter writer, 
            object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        Runtime.Object Create(string type)
        {
            switch (type) {
            case "int":
                return new LiteralInt(0);
            case "float":
                return new LiteralFloat (0.0f);
            case "divert":
                return new LiteralDivertTarget();
            case "pointer":
                return new LiteralVariablePointer();
            }

            return null;
        }

        public static string TypeName(Runtime.Object obj)
        {
            var literal = obj as Literal;
            if (literal == null) {
                return "<unknown>";
            }

            switch (literal.literalType) {
            case LiteralType.Int:
                return "int";
            case LiteralType.Float:
                return "float";
            case LiteralType.DivertTarget:
                return "divert";
            case LiteralType.VariablePointer:
                return "pointer";
            }
            return "other";
        }
    }
}

