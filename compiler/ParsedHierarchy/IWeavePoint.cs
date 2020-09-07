using System.Collections.Generic;

namespace Ink.Parsed
{
    public interface IWeavePoint
    {
        int indentationDepth { get; }
        Runtime.Container runtimeContainer { get; }
        List<Parsed.Object> content { get; }
        string name { get; }
        Identifier identifier { get; }

    }
}

