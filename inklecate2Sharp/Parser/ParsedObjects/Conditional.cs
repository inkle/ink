using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class Conditional : Parsed.Object
    {
        public Expression condition { get; }
        public List<Parsed.Object> contentIfTrue { get; }
        public List<Parsed.Object> contentIfFalse { get; }
        
        public Conditional (Expression condition, List<Parsed.Object> contentIfTrue, List<Parsed.Object> contentIfFalse = null)
        {
            this.condition = condition;
            this.contentIfTrue = contentIfTrue;
            this.contentIfFalse = contentIfFalse;

            this.condition.parent = this;
            foreach (var obj in contentIfTrue) {
                obj.parent = this;
            }
            if (contentIfFalse != null) {
                foreach (var obj in contentIfFalse) {
                    obj.parent = this;
                }
            }

        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            container.AddContent (condition.runtimeObject);

            // True branch - always generate runtime
            var trueRuntimeContainer = RuntimeContentForList (contentIfTrue);
            trueRuntimeContainer.name = "true";
            container.AddToNamedContentOnly (trueRuntimeContainer);
            _trueTargetObj = trueRuntimeContainer;
            _trueDivert = new Runtime.Divert ();

            _trueCompleteDivert = new Runtime.Divert ();
            trueRuntimeContainer.AddContent (_trueCompleteDivert);

            // False branch - optional
            Runtime.Container falseRuntimeContainer = null; 
            if (contentIfFalse != null) {
                falseRuntimeContainer = RuntimeContentForList (contentIfFalse);
                if (falseRuntimeContainer != null) {
                    falseRuntimeContainer.name = "false";
                    container.AddToNamedContentOnly (falseRuntimeContainer);
                    _falseDivert = new Runtime.Divert ();
                    _falseTargetObj = falseRuntimeContainer;

                    _falseCompleteDivert = new Runtime.Divert ();
                    falseRuntimeContainer.AddContent (_falseCompleteDivert);
                }
            }

            var branch = new Runtime.Branch (_trueDivert, _falseDivert);
            container.AddContent (branch);

            _reJoinTarget = Runtime.ControlCommand.NoOp ();
            container.AddContent (_reJoinTarget);

            return container;
        }

        Runtime.Container RuntimeContentForList(List<Parsed.Object> content)
        {
            if (content == null)
                return null;

            var container = new Runtime.Container ();

            foreach (var parsedObj in content) {
                container.AddContent (parsedObj.runtimeObject);
            }

            // Small optimisation: If it's just one piece of content that has
            // its own container already (without an explicit name), then just
            // re-use that container rather than nesting further.
            if (container.content.Count == 1) {
                var runtimeObj = container.content [0];
                var singleContentContainer = runtimeObj as Runtime.Container;
                if (singleContentContainer != null && !singleContentContainer.hasValidName) {
                    return singleContentContainer;
                }
            }

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            var container = (Runtime.Container)runtimeObject;

            var pathToReJoin = _reJoinTarget.path;

            _trueDivert.targetPath = _trueTargetObj.path;
            _trueCompleteDivert.targetPath = pathToReJoin;

            if (_falseDivert != null) {
                _falseDivert.targetPath = _falseTargetObj.path;
                _falseCompleteDivert.targetPath = pathToReJoin;
            }

            foreach (var obj in contentIfTrue) {
                obj.ResolveReferences (context);
            }
            if (contentIfFalse != null) {
                foreach (var obj in contentIfFalse) {
                    obj.ResolveReferences (context);
                }
            }

            condition.ResolveReferences (context);

        }

        Runtime.Divert _trueDivert;
        Runtime.Divert _falseDivert;

        Runtime.Object _trueTargetObj;
        Runtime.Object _falseTargetObj;

        Runtime.Divert _trueCompleteDivert;
        Runtime.Divert _falseCompleteDivert;

        Runtime.ControlCommand _reJoinTarget;
    }
}

