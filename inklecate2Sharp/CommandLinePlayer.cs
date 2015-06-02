using System;
using Inklewriter.Runtime;

namespace Inklewriter
{
	public class CommandLinePlayer
	{
		public Story story { get; protected set; }
		public bool autoPlay { get; set; }
        public Parsed.Story parsedStory { get; set; }

        public CommandLinePlayer (Story story, bool autoPlay = false, Parsed.Story parsedStory = null)
		{
			this.story = story;
			this.autoPlay = autoPlay;
            this.parsedStory = parsedStory;
		}

		public void Begin()
		{
			story.Begin ();

			Console.WriteLine(story.currentText);

			var rand = new Random ();

			while (story.currentChoices.Count > 0) {
				var choices = story.currentChoices;
				
                var choiceIdx = 0;
                bool choiceIsValid = false;
                Runtime.Path userDivertedPath = null;

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


                    do {
                        string userInput = Console.ReadLine ();

                        var inputParser = new InkParser (userInput);
                        object evaluatedInput = inputParser.CommandLineUserInput();

                        // Choice
                        if( evaluatedInput is int? ) {

                            choiceIdx = ((int)evaluatedInput) - 1;

                            if (choiceIdx < 0 || choiceIdx >= choices.Count) {
                                Console.WriteLine ("Choice out of range");
                            } else {
                                choiceIsValid = true;
                            }
                        }

                        // Help
                        else if( evaluatedInput is string && (string)evaluatedInput == "help" ) {
                            Console.WriteLine("Type a choice number, a divert (e.g. '==> myKnot'), an expression, or a variable assignment (e.g. 'x = 5')");
                        }

                        // Divert
                        else if( evaluatedInput is Parsed.Divert ) {
                            var divert = (Parsed.Divert) evaluatedInput;
                            divert.parent = parsedStory;
                            divert.GenerateRuntimeObject();
                            divert.ResolveReferences(parsedStory);
                            userDivertedPath = divert.runtimeDivert.targetPath;
                        }

                        // Expression
                        else if( evaluatedInput is Parsed.Expression ) {
                            var expr = (Parsed.Expression) evaluatedInput;
                            expr.parent = parsedStory;
                            expr.GenerateRuntimeObject();
                            var exprContainer = (Runtime.Container) expr.runtimeObject;
                            var result = story.EvaluateExpression(exprContainer);
                            Console.WriteLine(result);
                        }

                        // Variable assignment
                        else if( evaluatedInput is Parsed.VariableAssignment ) {
                            var varAss = (Parsed.VariableAssignment)evaluatedInput;
                            varAss.parent = parsedStory;
                            varAss.GenerateRuntimeObject();
                            var exprContainer = (Runtime.Container) varAss.runtimeObject;
                            var result = story.EvaluateExpression(exprContainer);
                            if( result != null ) {
                                Console.WriteLine(result);
                            }
                        }

                        else {
                            Console.WriteLine ("Unexpected input. Type 'help' or a choice number.");
                        }

                    } while(!choiceIsValid && userDivertedPath == null);

				}

                if (choiceIsValid) {
                    story.ContinueWithChoiceIndex (choiceIdx);
                } else if (userDivertedPath != null) {
                    story.ContinueFromPath (userDivertedPath);
                    userDivertedPath = null;
                }

				Console.WriteLine(story.currentText);
			}
		}
	}
}

