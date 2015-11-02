using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inklewriter.Runtime
{
    internal class ObjectJsonConverter : JsonConverter
    {
        public ObjectJsonConverter()
        {
            _typesByCustomName = new Dictionary<string, Type> ();
            _typesByUniqueFieldName = new Dictionary<string, Type> ();

            var runtimeObjType = typeof(Runtime.Object);
            var runtimeObjSubclasses = runtimeObjType.Assembly.GetTypes ().Where (t => t.IsSubclassOf (runtimeObjType));

            foreach (var subclassT in runtimeObjSubclasses) {

                // Find all classes that have a custom json type name (so they can be abbreviated)
                var customJsonName = (CustomJsonNameAttribute) Attribute.GetCustomAttribute (subclassT, typeof(CustomJsonNameAttribute));
                if (customJsonName != null) {
                    _typesByCustomName [customJsonName.name] = subclassT;
                }

                // Find all classes that can be identified by a unique property name that isn't used
                // within any other class. e.g. "var" for variable assignment
                foreach (var prop in subclassT.GetProperties()) {
                    if (Attribute.GetCustomAttribute (prop, typeof(UniqueJsonIdentifierAttribute)) != null) {
                        var jsonPropertyAttr = (JsonPropertyAttribute) Attribute.GetCustomAttribute (prop, typeof(JsonPropertyAttribute));
                        var jsonName = jsonPropertyAttr.PropertyName;
                        _typesByUniqueFieldName [jsonName] = subclassT;
                    }
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
            var token = JToken.ReadFrom (reader);

            var jArray = token as JArray;
            if (jArray != null) {

                var container = new Container ();
                container.content = jArray.ToObject<List<Runtime.Object>>(serializer);
                return container;
            }

            else if ( token is JValue ) {
                var val = (JValue) token;
                if (val.Type == JTokenType.String) {
                    return new Text (val.Value<string> ());
                } else if (val.Type == JTokenType.Integer) {
                    return new LiteralInt (val.Value<int> ());
                } else if (val.Type == JTokenType.Float) {
                    return new LiteralFloat (val.Value<float> ());
                } 

                throw new System.Exception ("Unexpected value type");
            }

            // Load JObject from stream
            JObject jObject = (JObject)token;

            Type type = null;

            // Try to find an explicit type of the Runtime.Object
            // (this is the serialisedTypeName property of Object)
            var runtimeObjTypeName = jObject.Value<string> ("%t");

            // No explicit type name - try to find a field in the object
            // that could identify it (e.g. "var" in a variable assignment)
            if (runtimeObjTypeName == null) {

                foreach (var p in jObject.Properties()) {
                    if (_typesByUniqueFieldName.TryGetValue (p.Name, out type)) {
                        break;
                    }
                }

                System.Diagnostics.Debug.Assert (type != null);
            } 
                
            // Try to use a custom class name
            else if (!_typesByCustomName.TryGetValue (runtimeObjTypeName, out type)) {

                // No custom type name: Assume it must be using a built in class name
                type = Type.GetType ("Inklewriter.Runtime." + runtimeObjTypeName);
            }

            var newObj = (Runtime.Object) System.Activator.CreateInstance (type);

            // Populate the object properties as usual
            if (type == typeof(Container)) {

                var container = (Container)newObj;

                JToken propToken = null;

                if (jObject.TryGetValue ("namedOnly", out propToken)) {
                    var named = propToken.ToObject<Dictionary<string, Runtime.Object>> (serializer);
                    container.namedOnlyContent = named;
                }

                if (jObject.TryGetValue ("c", out propToken)) {
                    container.content = propToken.ToObject<List<Runtime.Object>> (serializer);
                }

                if (jObject.TryGetValue ("name", out propToken)) {
                    container.name = propToken.Value<string>();
                }

                if (jObject.TryGetValue ("cntFlags", out propToken)) {
                    container.countFlags = propToken.Value<int> ();
                }

            } else {
                serializer.Populate (jObject.CreateReader (), newObj);
            }

            return newObj;
        }

        // Converter is only used for reading, not writing
        public override void WriteJson(JsonWriter writer, 
            object value,
            JsonSerializer serializer)
        {
            throw new System.NotImplementedException ();
        }

        Dictionary<string, Type> _typesByCustomName;
        Dictionary<string, Type> _typesByUniqueFieldName;
    }

    // Convert objects that contain simple values into JSON values
    // e.g. where a LiteralInt might otherwise have been stored as an object: { "value": 5 }
    //      or where Text would've been stored as: { "text": "the text" }
    // In the case of Containers, try to write them out as a simple array if they don't 
    // need any other properties
    internal class JsonSimplificationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.Equals (typeof(LiteralInt)) 
                || objectType.Equals (typeof(LiteralFloat)) 
                || objectType.Equals (typeof(Text))
                || objectType.Equals (typeof(Container));
        }

        public override object ReadJson(JsonReader reader, 
            Type objectType, 
            object existingValue, 
            JsonSerializer serializer)
        {
            throw new System.NotImplementedException ();
        }

        public override void WriteJson(JsonWriter writer, 
            object value,
            JsonSerializer serializer)
        {
            var text = value as Text;
            if (text) {
                writer.WriteValue (text.text);
            }

            var literal = value as Literal;
            if (literal) {
                writer.WriteValue (literal.valueObject);
            }

            var container = value as Container;
            if (container) {
                var namedOnlyContent = container.namedOnlyContent;
                var normalContent = container.content;

                bool needsFullObject = namedOnlyContent != null 
                    || container.visitsShouldBeCounted == true
                    || container.name != null;
                
                if (needsFullObject) {
                    writer.WriteStartObject ();

                    if (container.name != null) {
                        writer.WritePropertyName ("name");
                        writer.WriteValue (container.name);
                    }

                    writer.WritePropertyName ("c");
                }

                serializer.Serialize (writer, normalContent);

                if (needsFullObject) {

                    if (namedOnlyContent != null) {
                        writer.WritePropertyName ("namedOnly");
                        JToken t = JToken.FromObject (namedOnlyContent, serializer);
                        t.WriteTo (writer);
                    }

                    if (container.visitsShouldBeCounted || container.turnIndexShouldBeCounted) {
                        writer.WritePropertyName ("cntFlags");
                        writer.WriteValue (container.countFlags);
                    }

                    writer.WriteEndObject ();
                }
            }
        }
    }
   

    // Allows a custom/shorter type name than the default
    // class name to be used as the property for "%t" type name
    internal class CustomJsonNameAttribute : Attribute
    {
        public string name;

        public CustomJsonNameAttribute(string name) {
            this.name = name;

            if (name == null || name.Length == 0) {
                throw new  System.Exception ("Invalid custom name");
            }
        }
    }

    // Attribute used in conjunction with JsonProperty to indicate
    // that it's a property name that never exists on any other
    // object type. e.g. "var" as a property on a variable assignment
    // identifies the object type, and prevents the fallback "%t" type
    // name property from being used.
    internal class UniqueJsonIdentifierAttribute : Attribute
    {
    }
}

