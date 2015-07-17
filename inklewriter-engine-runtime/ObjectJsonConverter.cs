using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inklewriter.Runtime
{
    public class ObjectJsonConverter : JsonConverter
    {
        public ObjectJsonConverter()
        {
            _typesByUniqueFieldName = new Dictionary<string, Type> ();

            var runtimeObjType = typeof(Runtime.Object);
            var runtimeObjSubclasses = runtimeObjType.Assembly.GetTypes ().Where (t => t.IsSubclassOf (runtimeObjType));

            foreach (var subclassT in runtimeObjSubclasses) {
                var customJsonName = (CustomJsonNameAttribute) Attribute.GetCustomAttribute (subclassT, typeof(CustomJsonNameAttribute));
                if (customJsonName != null) {
                    _typesByUniqueFieldName [customJsonName.name] = subclassT;
                }
            }
        }

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

            Type type;
            if (!_typesByUniqueFieldName.TryGetValue (runtimeObjTypeName, out type)) {
                type = Type.GetType ("Inklewriter.Runtime." + runtimeObjTypeName);
            }
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

        Dictionary<string, Type> _typesByUniqueFieldName;
    }
       
    public class CustomJsonNameAttribute : Attribute
    {
        public string name;

        public CustomJsonNameAttribute(string name) {
            this.name = name;

            if (name == null || name.Length == 0) {
                throw new  System.Exception ("Invalid custom name");
            }
        }
    }
}

