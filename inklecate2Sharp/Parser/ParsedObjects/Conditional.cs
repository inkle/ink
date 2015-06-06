using System;
using System.Collections.Generic;
using System.Linq;
using Inklewriter.Runtime;

namespace Inklewriter.Parsed
{
    public class Conditional : Parsed.Object
    {
        public Expression initialCondition { get; }
        public List<ConditionalSingleBranch> branches { 
            get { 
                return this.content.Cast<ConditionalSingleBranch> ().ToList (); 
            }
        }
        
        public Conditional (Expression condition, List<ConditionalSingleBranch> branches)
        {
            this.initialCondition = condition;
            if (this.initialCondition != null) {
                this.initialCondition.parent = this;
            }

            AddContent (branches.Cast<Parsed.Object>().ToList());
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();

            // Initial condition
            if (this.initialCondition != null) {
                container.AddContent (initialCondition.runtimeObject);
            }

            // Individual branches
            foreach (var obj in content) {
                var branch = (ConditionalSingleBranch)obj;
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

            foreach (var obj in content) {
                var branch = (ConditionalSingleBranch)obj;
                branch.returnDivert.targetPath = pathToReJoin;
            }

            if( initialCondition != null) 
                initialCondition.ResolveReferences (context);

            base.ResolveReferences (context);
        }
            
        Runtime.ControlCommand _reJoinTarget;
    }
}

