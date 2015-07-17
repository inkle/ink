using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inklewriter.Runtime
{
    public class ObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var runtimeObjType = typeof(Runtime.Object);
            return runtimeObjType.Equals (objectType) || objectType.IsSubclassOf (runtimeObjType);
        }

        public override object ReadJson(JsonReader reader, 
            Type objectType, 
            object existingValue, 
            JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            var runtimeObjTypeName = jObject.Value<string> ("%t");

            Type type = Type.GetType ("Inklewriter.Runtime." + runtimeObjTypeName);
            var newObj = (Runtime.Object) System.Activator.CreateInstance (type);

            // Populate the object properties
            serializer.Populate (jObject.CreateReader (), newObj);

            return newObj;
        }

        // This converter is only used for reading, not writing
        public override void WriteJson(JsonWriter writer, 
            object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public static string TypeName(Runtime.Object obj)
        {
            return obj.GetType ().Name;
        }
    }
}

