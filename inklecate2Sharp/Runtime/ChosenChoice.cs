using System;

namespace Inklewriter.Runtime
{
	public class ChosenChoice
	{
		public Choice choice { get; }

		public ChosenChoice (Choice choice)
		{
			this.choice = choice;
		}
	}
}

