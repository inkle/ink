using System;

namespace Inklewriter.Parsed
{
	public class Divert : Parsed.Object
	{
		public Parsed.Path target { get; protected set; }
        public Parsed.Object targetContent { get; protected set; }
		public Runtime.Divert runtimeDivert { get; protected set; }

		public Divert (Parsed.Path target)
		{
			this.target = target;
		}

        public Divert (Parsed.Object targetContent)
        {
            this.targetContent = targetContent;
        }

		public override Runtime.Object GenerateRuntimeObject ()
		{
			runtimeDivert = new Runtime.Divert ();
            return runtimeDivert;
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

