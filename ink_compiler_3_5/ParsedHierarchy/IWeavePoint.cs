using System.Collections.Generic;

namespace Ink.Parsed
{
    internal interface IWeavePoint
    {
        int indentationDepth { get; }
        Runtime.Container runtimeContainer { get; }
        List<Parsed.Object> content { get; }
        string name { get; }

    }
}

