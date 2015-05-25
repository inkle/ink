using System;

namespace Inklewriter.Parsed
{
	public class Divert : Parsed.Object
	{
		public Parsed.Path target { get; protected set; }
		public bool returning { get; }

		public Runtime.Divert runtimeDivert { get; protected set; }

		public Divert (Parsed.Path target, bool returning)
		{
			this.target = target;
			this.returning = returning;
		}

		public override Runtime.Object GenerateRuntimeObject ()
		{
			runtimeDivert = new Runtime.Divert ();

			// If returning, do a stack push first
			if (returning) {
				var container = new Runtime.Container ();
				container.AddContent (new Runtime.StackPush ());
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
			Parsed.Object divertTargetObj = ResolvePath (target);
			if (divertTargetObj == null) {
				Error ("Divert: target not found: '" + target.ToString () + "'");
			} else {
				runtimeDivert.targetPath = divertTargetObj.runtimeObject.path;
			}
		}
			
	}
}

