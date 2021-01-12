using System.Collections.Generic;

namespace Ink.Parsed
{
    public class TunnelOnwards : Parsed.Object
    {
        public Divert divertAfter {
            get {
                return _divertAfter;
            }
            set {
                _divertAfter = value;
                if (_divertAfter) AddContent (_divertAfter);
            }
        }
        Divert _divertAfter;

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // Set override path for tunnel onwards (or nothing)
            container.AddContent (Runtime.ControlCommand.EvalStart ());

            if (divertAfter) {

                // Generate runtime object's generated code and steal the arguments runtime code
                var returnRuntimeObj = divertAfter.GenerateRuntimeObject ();
                var returnRuntimeContainer = returnRuntimeObj as Runtime.Container;
                if (returnRuntimeContainer) {

                    // Steal all code for generating arguments from the divert
                    var args = divertAfter.arguments;
                    if (args != null && args.Count > 0) {

                        // Steal everything betwen eval start and eval end
                        int evalStart = -1;
                        int evalEnd = -1;
                        for (int i = 0; i < returnRuntimeContainer.content.Count; i++) {
                            var cmd = returnRuntimeContainer.content [i] as Runtime.ControlCommand;
                            if (cmd) {
                                if (evalStart == -1 && cmd.commandType == Runtime.ControlCommand.CommandType.EvalStart)
                                    evalStart = i;
                                else if (cmd.commandType == Runtime.ControlCommand.CommandType.EvalEnd)
                                    evalEnd = i;
                            }
                        }

                        for (int i = evalStart + 1; i < evalEnd; i++) {
                            var obj = returnRuntimeContainer.content [i];
                            obj.parent = null; // prevent error of being moved between owners
                            container.AddContent (returnRuntimeContainer.content [i]);
                        }
                    }
                }

                // Finally, divert to the requested target 
                _overrideDivertTarget = new Runtime.DivertTargetValue ();
                container.AddContent (_overrideDivertTarget);
            } 

            // No divert after tunnel onwards
            else {
                container.AddContent (new Runtime.Void ());
            }

            container.AddContent (Runtime.ControlCommand.EvalEnd ());

            container.AddContent (Runtime.ControlCommand.PopTunnel ());

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            if (divertAfter && divertAfter.targetContent)
                _overrideDivertTarget.targetPath = divertAfter.targetContent.runtimePath;
        }

        Runtime.DivertTargetValue _overrideDivertTarget;
    }
}

