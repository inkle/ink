using System;

namespace Inklewriter.Parsed
{
    public interface IWeavePoint
    {
        int indentationDepth { get; }
        bool hasLooseEnd { get; }
        Runtime.Container runtimeContainer { get; }

        void AddNestedContent(Parsed.Object obj);
    }
}

