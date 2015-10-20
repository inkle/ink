
namespace Inklewriter.Runtime
{
	internal class ChoiceInstance : Runtime.Object
	{
		public Choice choice { get; private set; }
        public bool hasBeenChosen { get; set; }
        public CallStack callStackAtGeneration { get; set; }

		public ChoiceInstance (Choice choice)
		{
			this.choice = choice;
		}
	}
}

