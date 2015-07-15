
namespace Inklewriter.Runtime
{
	public class ChosenChoice : Runtime.Object
	{
		public Choice choice { get; private set; }

		public ChosenChoice (Choice choice)
		{
			this.choice = choice;
		}
	}
}

