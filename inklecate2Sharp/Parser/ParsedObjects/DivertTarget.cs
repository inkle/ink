using System;

namespace Inklewriter.Parsed
{
    public class DivertTarget : Expression
    {
        public Divert divert;

        public DivertTarget (Divert divert)
        {
            this.divert = divert;
            this.divert.parent = this;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            _runtimeDivert = (Runtime.Divert) divert.runtimeObject;
            _runtimeLiteralDivertTarget = new Runtime.LiteralDivertTarget (_runtimeDivert);

            _runtimeLiteralDivertTarget.divert = _runtimeDivert;

            container.AddContent (_runtimeLiteralDivertTarget);
        }

        public override void ResolveReferences (Story context)
        {
            divert.ResolveReferences (context);

            _runtimeLiteralDivertTarget.divert = _runtimeDivert;
        }
            
        Runtime.LiteralDivertTarget _runtimeLiteralDivertTarget;
        Runtime.Divert _runtimeDivert;
    }
}

