
namespace Ink.Runtime
{
	public class ChoiceInstance : Runtime.Object
	{
        public string choiceText { get; internal set; }

        internal bool hasBeenChosen { get; set; }
		internal Choice choice { get; private set; }
        internal CallStack.Thread threadAtGeneration { get; set; }

		internal ChoiceInstance (Choice choice)
		{
			this.choice = choice;
		}

	}
}

