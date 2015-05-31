using System;

namespace Inklewriter.Parsed
{
    public class Conditional : Parsed.Object
    {
        public Expression condition { get; }
        public Parsed.Object contentIfTrue { get; }
        public Parsed.Object contentIfFalse { get; }
        
        public Conditional (Expression condition, Parsed.Object contentIfTrue, Parsed.Object contentIfFalse = null)
        {
            this.condition = condition;
            this.contentIfTrue = contentIfTrue;
            this.contentIfFalse = contentIfFalse;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            container.AddContent (condition.runtimeObject);

            var trueRuntimeObj = contentIfTrue.runtimeObject;
            var trueRuntimeContainer = WrapInContainerIfNecessary (trueRuntimeObj);
            trueRuntimeContainer.name = "true";
            container.AddToNamedContentOnly (trueRuntimeContainer);
            _trueTargetObj = trueRuntimeContainer;
            _trueDivert = new Runtime.Divert ();

            _trueCompleteDivert = new Runtime.Divert ();
            trueRuntimeContainer.AddContent (_trueCompleteDivert);

            Runtime.Container falseRuntimeContainer = null; 
            if (contentIfFalse != null) {
                var falseRuntimeObj = contentIfFalse.runtimeObject;
                falseRuntimeContainer = WrapInContainerIfNecessary (falseRuntimeObj);
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

            return container;
        }

        Runtime.Container WrapInContainerIfNecessary(Runtime.Object obj)
        {
            if (obj == null)
                return null;

            if (obj is Runtime.Container) {
                var container = (Runtime.Container)obj;
                if (!container.hasValidName) {
                    return container;
                }
            }

            var wrapper = new Runtime.Container ();
            wrapper.AddContent (obj);
            return wrapper;
        }

        public override void ResolveReferences (Story context)
        {
            var container = (Runtime.Container)runtimeObject;

            var pathToReJoin = container.path.PathByAppendingPath(container.pathToEnd);

            _trueDivert.targetPath = _trueTargetObj.path;
            _trueCompleteDivert.targetPath = pathToReJoin;

            if (_falseDivert != null) {
                _falseDivert.targetPath = _falseTargetObj.path;
                _falseCompleteDivert.targetPath = pathToReJoin;
            }
        }

        Runtime.Divert _trueDivert;
        Runtime.Divert _falseDivert;

        Runtime.Object _trueTargetObj;
        Runtime.Object _falseTargetObj;

        Runtime.Divert _trueCompleteDivert;
        Runtime.Divert _falseCompleteDivert;
    }
}

