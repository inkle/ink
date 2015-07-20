
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    internal class ConditionalSingleBranch : Parsed.Object
    {
        // bool condition, e.g.:
        // { 5 == 4:
        //   - the true branch
        //   - the false branch
        // }
        public bool isBoolCondition { get; set; }

        // whether it's the "true" branch or "false" branch when it's a bool condition
        public bool boolRequired { get; set; }

        // When each branch has its own expression like a switch statement,
        // this is non-null. e.g.
        // { x:
        //    - 4: the value of x is four (ownExpression is the value 4)
        //    - 3: the value of x is three
        // }
        public Expression ownExpression { 
            get { 
                return _ownExpression; 
            } 
            set { 
                _ownExpression = value; 
                if (_ownExpression) {
                    AddContent (_ownExpression); 
                }
            }
        }

        // In the above example, match equality of x with 4 for the first branch.
        // This is as opposed to simply evaluating boolean equality for each branch,
        // example when shouldMatchEqualtity is FALSE:
        // {
        //    3 > 2:  This will happen
        //    2 > 3:  This won't happen
        // }
        public bool shouldMatchEquality { get; set; }

        // used for else branches
        public bool alwaysMatch { get; set; }

        public Runtime.Divert returnDivert { get; protected set; }

        public ConditionalSingleBranch (List<Parsed.Object> content)
        {
            _innerWeave = new Weave (content);
            AddContent (_innerWeave);
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
            bool usingValueOnStack = (isBoolCondition || shouldMatchEquality) && !alwaysMatch;
            if ( usingValueOnStack ) {
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

                bool needsEval = ownExpression || alwaysMatch;

                if( needsEval )
                    container.AddContent (Runtime.ControlCommand.EvalStart ());

                if (ownExpression)
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

            if( usingValueOnStack )
                _contentContainer.InsertContent (Runtime.ControlCommand.PopEvaluatedValue (), 0);

            container.AddToNamedContentOnly (_contentContainer);

            returnDivert = new Runtime.Divert ();
            _contentContainer.AddContent (returnDivert);

            return container;
        }

        Runtime.Container GenerateRuntimeForContent()
        {
            var container = _innerWeave.rootContainer;

            // Small optimisation: If it's just one piece of content that has
            // its own container already (without an explicit name), then just
            // re-use that container rather than nesting further.
            if (container.content.Count == 1) {
                var runtimeObj = container.content [0];
                var singleContentContainer = runtimeObj as Runtime.Container;
                if (singleContentContainer && !singleContentContainer.hasValidName) {
                    container = singleContentContainer;
                }
            } 

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            _divertOnBranch.targetPath = _contentContainer.path;

            base.ResolveReferences (context);
        }

        Runtime.Container _contentContainer;
        Runtime.Divert _divertOnBranch;
        Expression _ownExpression;

        Weave _innerWeave;
    }
}

