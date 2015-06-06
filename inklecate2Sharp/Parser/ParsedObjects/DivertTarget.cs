using System;

namespace Inklewriter.Parsed
{
    public class DivertTarget : Expression
    {
        public Divert divert;

        public DivertTarget (Divert divert)
        {
            this.divert = AddContent(divert);
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            divert.GenerateRuntimeObject();

            _runtimeDivert = (Runtime.Divert) divert.runtimeDivert;
            _runtimeLiteralDivertTarget = new Runtime.LiteralDivertTarget (_runtimeDivert);
            _runtimeLiteralDivertTarget.divert = _runtimeDivert;

            if (divert.arguments != null && divert.arguments.Count > 0) {
                Error ("Can't use a divert target as a variable if it has parameters");
                return;
            }

            container.AddContent (_runtimeLiteralDivertTarget);
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            _runtimeLiteralDivertTarget.divert = _runtimeDivert;
        }
            
        Runtime.LiteralDivertTarget _runtimeLiteralDivertTarget;
        Runtime.Divert _runtimeDivert;
    }
}

