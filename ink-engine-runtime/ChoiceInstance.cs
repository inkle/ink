
namespace Ink.Runtime
{
	public class ChoiceInstance : Runtime.Object
	{
        public string choiceText { get; set; }
        public string pathStringOnChoice { get { return choice.pathStringOnChoice; } }
        public int choiceIndex { get; set; }

        internal Choice choice { get; private set; }
        internal bool hasBeenChosen { get; set; }
        internal CallStack.Thread threadAtGeneration { get; set; }

        public ChoiceInstance()
        {
        }

		internal ChoiceInstance (Choice choice)
		{
			this.choice = choice;
		}

	}
}

