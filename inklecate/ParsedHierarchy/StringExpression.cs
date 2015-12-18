using System;

namespace Ink.Parsed
{
    internal class StringExpression : Parsed.Expression
    {
        // TODO: Replace with ContentList or something
        public string tempText;

        public StringExpression (string text)
        {
            tempText = text;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            container.AddContent (new Runtime.LiteralString (tempText));
        }

        public override string ToString ()
        {
            return this.tempText;
        }
    }
}

