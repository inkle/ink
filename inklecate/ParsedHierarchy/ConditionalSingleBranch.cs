
using System.Collections.Generic;

namespace Ink.Parsed
{
    internal class ConditionalSingleBranch : Parsed.Object
    {
        // bool condition, e.g.:
        // { 5 == 4:
        //   - the true branch
        //   - the false branch
        // }
        public bool isTrueBranch { get; set; }

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

        public bool isElse { get; set; }

        public Runtime.Divert returnDivert { get; protected set; }

        public ConditionalSingleBranch (List<Parsed.Object> content)
        {
            // Branches are allowed to be empty
            if (content != null) {
                _innerWeave = new Weave (content);
                AddContent (_innerWeave);
            }
        }

        // Runtime content can be summarised as follows:
        //  - Evaluate an expression if necessary to branch on
        //  - Branch to a named container if true
        //       - Divert back to main flow
        //         (owner Conditional is in control of this target point)
        public override Runtime.Object GenerateRuntimeObject ()
        {
            // Check for common mistake, of putting "else:" instead of "- else:"
            if (_innerWeave) {
                foreach (var c in _innerWeave.content) {
                    var text = c as Parsed.Text;
                    if (text) {
                        // Don't need to trim at the start since the parser handles that already
                        if (text.text.StartsWith ("else:", System.StringComparison.InvariantCulture)) {
                            Warning ("Saw the text 'else:' which is being treated as content. Did you mean '- else:'?", text);
                        }
                    }
                }
            }
                                           
            var container = new Runtime.Container ();

            // Are we testing against a condition that's used for more than just this
            // branch? If so, the first thing we need to do is replicate the value that's
            // on the evaluation stack so that we don't fully consume it, in case other
            // branches need to use it.
            bool duplicatesStackValue = shouldMatchEquality && !isElse;
            if ( duplicatesStackValue )
                container.AddContent (Runtime.ControlCommand.Duplicate ());

            _conditionalDivert = new Runtime.Divert ();

            // else clause is unconditional catch-all, otherwise the divert is conditional
            _conditionalDivert.isConditional = !isElse;

            // Need extra evaluation?
            if( !isTrueBranch && !isElse ) {

                bool needsEval = ownExpression != null;
                if( needsEval )
                    container.AddContent (Runtime.ControlCommand.EvalStart ());

                if (ownExpression)
                    ownExpression.GenerateIntoContainer (container);

                // Uses existing duplicated value
                if (shouldMatchEquality)
                    container.AddContent (Runtime.NativeFunctionCall.CallWithName ("=="));

                if( needsEval ) 
                    container.AddContent (Runtime.ControlCommand.EvalEnd ()); 
            }

            // Will pop from stack if conditional
            container.AddContent (_conditionalDivert);

            _contentContainer = GenerateRuntimeForContent ();
            _contentContainer.name = "b";

            if( duplicatesStackValue )
                _contentContainer.InsertContent (Runtime.ControlCommand.PopEvaluatedValue (), 0);

            container.AddToNamedContentOnly (_contentContainer);

            returnDivert = new Runtime.Divert ();
            _contentContainer.AddContent (returnDivert);

            return container;
        }

        Runtime.Container GenerateRuntimeForContent()
        {
            // Empty branch - create empty container
            if (_innerWeave == null) {
                return new Runtime.Container ();
            }

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
            _conditionalDivert.targetPath = _contentContainer.path;

            base.ResolveReferences (context);
        }

        Runtime.Container _contentContainer;
        Runtime.Divert _conditionalDivert;
        Expression _ownExpression;

        Weave _innerWeave;
    }
}

