using System;
using Inklewriter.Runtime;

namespace Inklewriter
{
	public class CommandLinePlayer
	{
		public Story story { get; protected set; }
		public bool autoPlay { get; set; }

		public CommandLinePlayer (Story story, bool autoPlay = false)
		{
			this.story = story;
			this.autoPlay = autoPlay;
		}

		public void Begin()
		{
			story.Begin ();

			Console.WriteLine(story.currentText);

			var rand = new Random ();

			while (story.currentChoices.Count > 0) {
				var choices = story.currentChoices;
				var choiceIdx = 0;

				// autoPlay: Pick random choice
				if (autoPlay) {
					choiceIdx = rand.Next () % choices.Count;
				} 

				// Normal: Ask user for choice number
				else {

					int i = 1;
					foreach (Choice choice in choices) {
						Console.WriteLine ("{0}: {1}", i, choice.choiceText);
						i++;
					}

					string userInput = Console.ReadLine ();

					var inputParser = new StringParser (userInput);
					var intOrNull = inputParser.ParseInt ();
					if (intOrNull == null) {
						Console.WriteLine ("That's not a choice number");
					} else {
						choiceIdx = ((int) intOrNull)-1;
					}
				}

				story.ContinueWithChoiceIndex (choiceIdx);

				Console.WriteLine(story.currentText);
			}
		}
	}
}

