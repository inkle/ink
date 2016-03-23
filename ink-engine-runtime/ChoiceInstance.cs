
namespace Ink.Runtime
{
	public class ChoiceInstance : Runtime.Object
	{
        public string choiceText { get; set; }
        public string pathStringOnChoice { get { return choice.pathStringOnChoice; } }
        public int choiceIndex { get; set; }

        internal Choice choice { get; set; }
        internal CallStack.Thread threadAtGeneration { get; set; }
        internal int originalThreadIndex;

        // Only used temporarily for loading/saving from JSON
        internal string originalChoicePath;


        public ChoiceInstance()
        {
        }

		internal ChoiceInstance (Choice choice)
		{
			this.choice = choice;
		}

	}
}

