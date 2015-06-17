
namespace Inklewriter.Runtime
{
	public class ChosenChoice : Runtime.Object
	{
		public Choice choice { get; }

		public ChosenChoice (Choice choice)
		{
			this.choice = choice;
		}
	}
}

