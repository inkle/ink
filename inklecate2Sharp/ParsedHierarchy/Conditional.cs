using System.Collections.Generic;
using System.Linq;
using Inklewriter.Runtime;

namespace Inklewriter.Parsed
{
    public class Conditional : Parsed.Object
    {
        public Expression initialCondition { get; }
        public List<ConditionalSingleBranch> branches { get; }
        
        public Conditional (Expression condition, List<ConditionalSingleBranch> branches)
        {
            this.initialCondition = condition;
            if (this.initialCondition) {
                AddContent (condition);
            }

            this.branches = branches;
            if (this.branches != null) {
                AddContent (this.branches.Cast<Parsed.Object> ().ToList ());
            }

        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // Initial condition
            if (this.initialCondition) {
                container.AddContent (initialCondition.runtimeObject);
            }

            // Individual branches
            foreach (var branch in branches) {
                var branchContainer = (Container) branch.runtimeObject;
                container.AddContent (branchContainer);
            }

            // If no branches matched, tidy up after ourselves
            if (this.initialCondition)
                container.AddContent (Runtime.ControlCommand.PopEvaluatedValue ());

            // Target for branches to rejoin to
            _reJoinTarget = Runtime.ControlCommand.NoOp ();
            container.AddContent (_reJoinTarget);

            return container;
        }

        public override void ResolveReferences (Story context)
        {
            var container = (Runtime.Container)runtimeObject;

            var pathToReJoin = _reJoinTarget.path;

            foreach (var branch in branches) {
                branch.returnDivert.targetPath = pathToReJoin;
            }

            base.ResolveReferences (context);
        }
            
        Runtime.ControlCommand _reJoinTarget;
    }
}

