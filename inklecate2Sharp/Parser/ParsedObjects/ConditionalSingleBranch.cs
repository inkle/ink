using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class ConditionalSingleBranch : Parsed.Object
    {
        public bool isBoolCondition { get; set; }
        public bool boolRequired { get; set; }
        public Expression ownExpression { get; set; }
        public bool shouldMatchEquality { get; set; }
        public bool alwaysMatch { get; set; } // used for else clause
        public Runtime.Divert returnDivert { get; protected set; }

        public ConditionalSingleBranch (List<Parsed.Object> content)
        {
            AddContent (content);
            this.content = content;
            foreach (var c in this.content) {
                c.parent = this;
            }
        }

        // Runtime content can be summarised as follows:
        //  - Evaluate an expression if necessary to branch on
        //  - Branch to a named container if true
        //       - Divert back to main flow
        //         (owner Conditional is in control of this target point)
        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // Are we testing against a condition that's used for more than just this
            // branch? If so, the first thing we need to do is replicate the value that's
            // on the evaluation stack so that we don't fully consume it, in case other
            // branches need to use it.
            if ( (isBoolCondition || shouldMatchEquality) && !alwaysMatch ) {
                container.AddContent (Runtime.ControlCommand.Duplicate ());
            }

            _divertOnBranch = new Runtime.Divert ();

            Runtime.Branch branch;

            if (isBoolCondition) {
                if (boolRequired == true) {
                    branch = new Runtime.Branch (trueDivert: _divertOnBranch);
                } else {
                    branch = new Runtime.Branch (falseDivert: _divertOnBranch);
                }
            } else {

                bool needsEval = ownExpression != null || alwaysMatch;

                if( needsEval )
                    container.AddContent (Runtime.ControlCommand.EvalStart ());

                if (ownExpression != null)
                    ownExpression.GenerateIntoContainer (container);

                if (shouldMatchEquality)
                    container.AddContent (Runtime.NativeFunctionCall.CallWithName ("=="));

                if (alwaysMatch)
                    container.AddContent (new Runtime.LiteralInt (1));

                if( needsEval ) 
                    container.AddContent (Runtime.ControlCommand.EvalEnd ()); 

                branch = new Runtime.Branch (trueDivert: _divertOnBranch);
            }

            container.AddContent (branch);

            _contentContainer = GenerateRuntimeForContent ();
            _contentContainer.name = "b";
            container.AddToNamedContentOnly (_contentContainer);

            returnDivert = new Runtime.Divert ();
            _contentContainer.AddContent (returnDivert);

            return container;
        }

        Runtime.Container GenerateRuntimeForContent()
        {
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
            foreach (var c in this.content) {
                c.ResolveReferences (context);
            }

            _divertOnBranch.targetPath = _contentContainer.path;
        }

        Runtime.Container _contentContainer;
        Runtime.Divert _divertOnBranch;
    }
}

