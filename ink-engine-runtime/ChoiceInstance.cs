
namespace Ink.Runtime
{
	internal class ChoiceInstance : Runtime.Object
	{
		public Choice choice { get; private set; }
        public bool hasBeenChosen { get; set; }
        public CallStack.Thread threadAtGeneration { get; set; }

		public ChoiceInstance (Choice choice)
		{
			this.choice = choice;
		}
	}
}

