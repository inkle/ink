using System;
using Newtonsoft.Json.Linq;

namespace Ink.Runtime
{
    internal interface IJsonSerialisable
    {
        JToken jsonToken { get; set; }
    }
}

