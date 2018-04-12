using System;
using System.Collections.Generic;
using Ink.Runtime;

namespace Ink
{
	internal class CommandLinePlayer
	{
		public Story story { get; protected set; }
		public bool autoPlay { get; set; }
        public bool keepOpenAfterStoryFinish { get; set; }

        public CommandLinePlayer (Story story, bool autoPlay = false, Compiler compiler = null, bool keepOpenAfterStoryFinish = false)
		{
			this.story = story;
			this.autoPlay = autoPlay;
            _compiler = compiler;
            this.keepOpenAfterStoryFinish = keepOpenAfterStoryFinish;
		}

		public void Begin()
		{
            EvaluateStory ();

			var rand = new Random ();

            while (story.currentChoices.Count > 0 || this.keepOpenAfterStoryFinish) {
				var choices = story.currentChoices;
				
                var choiceIdx = 0;
                bool choiceIsValid = false;
                string userDivertedPath = null;

				// autoPlay: Pick random choice
				if (autoPlay) {
					choiceIdx = rand.Next () % choices.Count;
				}

				// Normal: Ask user for choice number
				else {

                    Console.ForegroundColor = ConsoleColor.Blue;

                    // Add extra newline to ensure that the choice is
                    // on a separate line.
                    Console.WriteLine ();

					int i = 1;
					foreach (Choice choice in choices) {
						Console.WriteLine ("{0}: {1}", i, choice.text);
						i++;
					}


                    do {
                        // Prompt
                        Console.Write("?> ");
                        string userInput = Console.ReadLine ();

                        // If we have null user input, it means that we're
                        // "at the end of the stream", or in other words, the input
                        // stream has closed, so there's nothing more we can do.
                        // We return immediately, since otherwise we get into a busy
                        // loop waiting for user input.
                        if (userInput == null) {
                            Console.WriteLine ("<User input stream closed.>");
                            return;
                        }

                        var result = _compiler.ReadCommandLineInput (userInput);

                        if (result.output != null)
                            Console.WriteLine (result.output);

                        if (result.requestsExit)
                            return;

                        if (result.divertedPath != null)
                            userDivertedPath = result.divertedPath;

                        if (result.choiceIdx >= 0) {
                            if (result.choiceIdx >= choices.Count) {
                                Console.WriteLine ("Choice out of range");
                            } else {
                                choiceIdx = result.choiceIdx;
                                choiceIsValid = true;
                            }
                        }

                    } while(!choiceIsValid && userDivertedPath == null);

				}

                Console.ResetColor ();

                if (choiceIsValid) {
                    story.ChooseChoiceIndex (choiceIdx);
                } else if (userDivertedPath != null) {
                    story.ChoosePathString (userDivertedPath);
                    userDivertedPath = null;
                }
                    
                EvaluateStory ();
			}
		}

        void EvaluateStory ()
        {
            while (story.canContinue) {

                story.Continue ();

                _compiler.RetrieveDebugSourceForLatestContent ();

                Console.Write (story.currentText);

                var tags = story.currentTags;
                if (tags.Count > 0)
                    Console.WriteLine ("# tags: " + string.Join (", ", tags));

                if (story.hasError) {
                    foreach (var errorMsg in story.currentErrors) {
                        Console.WriteLine (errorMsg, ConsoleColor.Red);
                    }
                }
            }

            if (story.currentChoices.Count == 0 && keepOpenAfterStoryFinish) {
                Console.WriteLine ("--- End of story ---");
            }

            story.ResetErrors ();
        }

        Compiler _compiler;
	}


}

