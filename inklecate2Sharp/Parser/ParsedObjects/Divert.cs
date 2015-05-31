using System;

namespace Inklewriter.Parsed
{
	public class Divert : Parsed.Object
	{
		public Parsed.Path target { get; protected set; }
        public Parsed.Object targetContent { get; protected set; }
		public bool returning { get; }

		public Runtime.Divert runtimeDivert { get; protected set; }

		public Divert (Parsed.Path target, bool returning)
		{
			this.target = target;
			this.returning = returning;
		}

        public Divert (Parsed.Object targetContent, bool returning)
        {
            this.targetContent = targetContent;
            this.returning = returning;
        }

		public override Runtime.Object GenerateRuntimeObject ()
		{
			runtimeDivert = new Runtime.Divert ();

			// If returning, do a stack push first
			if (returning) {
				var container = new Runtime.Container ();
                container.AddContent (Runtime.ControlCommand.StackPush());
				container.AddContent (runtimeDivert);
				return container;
			} 

			// Pure divert, no need for Container
			else {
				return runtimeDivert;
			}
		}

        public override void ResolveReferences(Story context)
		{
            if (targetContent == null) {
                targetContent = target.ResolveFromContext (this);

                if (targetContent == null) {

                    bool foundAlternative = false;
                    Path alternativePath = target.debugSuggestedAlternative;
                    if (alternativePath != null) {
                        targetContent = alternativePath.ResolveFromContext (this);
                        if (targetContent != null) {
                            foundAlternative = true;
                        }
                    }

                    if (foundAlternative) {
                        Error ("Divert: target not found: '" + target.ToString () + "'. Did you mean '"+alternativePath+"'?");
                        target = alternativePath;
                    } else {
                        Error ("Divert: target not found: '" + target.ToString () + "'");
                    }
                }
            }

			if (targetContent != null) {
				runtimeDivert.targetPath = targetContent.runtimePath;
			}
		}
			
	}
}

