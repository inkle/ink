namespace Ink.Parsed
{
    internal class TunnelOnwards : Parsed.Object
    {
        public Parsed.Path overrideReturnPath;

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // Set override path for tunnel onwards (or nothing)
            container.AddContent (Runtime.ControlCommand.EvalStart ());
            if (overrideReturnPath != null) {
                _overrideDivertTarget = new Runtime.DivertTargetValue ();
                container.AddContent (_overrideDivertTarget);
            } else {
                container.AddContent (new Runtime.Void ());
            }
            container.AddContent (Runtime.ControlCommand.EvalEnd ());

            container.AddContent (Runtime.ControlCommand.PopTunnel ());

            this.story.CanFlattenContainer (container);

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            if (overrideReturnPath != null) {
                var targetContent = overrideReturnPath.ResolveFromContext (this);
                if (targetContent) {
                    _overrideDivertTarget.targetPath = targetContent.runtimePath;
                } else {
                    Error ("Override target after tunnel onwards not found: ->-> " + overrideReturnPath.dotSeparatedComponents, this);
                }
            }
        }

        Runtime.DivertTargetValue _overrideDivertTarget;
    }
}

