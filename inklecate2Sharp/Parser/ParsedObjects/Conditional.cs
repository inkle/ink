using System;
using System.Collections.Generic;
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
            if (this.initialCondition != null) {
                this.initialCondition.parent = this;
            }

            this.branches = branches;
            foreach (var branch in this.branches) {
                if (condition != null) {
                    branch.shouldMatchEquality = true;
                }
                branch.parent = this;
            }
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // Initial condition
            if (this.initialCondition != null) {
                container.AddContent (initialCondition.runtimeObject);
            }

            // Individual branches
            foreach (var branch in branches) {
                var branchContainer = (Container) branch.runtimeObject;
                container.AddContent (branchContainer);
            }

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
                branch.ResolveReferences (context);
            }

            if( initialCondition != null) 
                initialCondition.ResolveReferences (context);
        }
            
        Runtime.ControlCommand _reJoinTarget;
    }
}

