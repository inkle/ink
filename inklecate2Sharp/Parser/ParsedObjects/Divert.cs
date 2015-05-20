using System;

namespace inklecate2Sharp.Parsed
{
	public class Divert : Parsed.Object
	{
		public Parsed.Path target { get; protected set; }

		public Runtime.Path runtimeTargetPath
		{
			get
			{
				if (runtimeObject == null) {
					return null;
				}

				return (runtimeObject as Runtime.Divert).targetPath;
			}
		}

		public Divert (Parsed.Path target)
		{
			this.target = target;
		}

		public override Runtime.Object GenerateRuntimeObject ()
		{
			return new Runtime.Divert ();
		}

		public override void ResolvePaths()
		{
			Parsed.Object divertTargetObj = ResolvePath (target);
			if (divertTargetObj == null) {

				Error ("Divert: target not found: " + target);
			} else {
				var runtimeDivert = (Runtime.Divert)runtimeObject;
				runtimeDivert.targetPath = divertTargetObj.runtimeObject.path;
			}
		}


	}
}

