using System;

namespace inklecate2Sharp.Parsed
{
	public class Choice : Parsed.Object
	{
		public string choiceText { get; protected set; }
		public Divert divert { get; }

		public Choice (string choiceText, Divert divert)
		{
			this.choiceText = choiceText;
			this.divert = divert;
		}

		public override Runtime.Object GenerateRuntimeObject ()
		{
			var runtimeChoice = new Runtime.Choice (choiceText);
			return runtimeChoice;
		}

		public override void ResolvePaths()
		{
			// Don't actually use the Parsed.Divert in the runtime, but use its path resolution
			// to set the pathOnChoice property of the Runtime.Choice.
			divert.ResolvePaths ();

			var runtimeChoice = runtimeObject as Runtime.Choice;
			runtimeChoice.pathOnChoice = divert.runtimeTargetPath;
		}
	}
}

