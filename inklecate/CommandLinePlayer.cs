using System;
using System.Collections.Generic;
using Ink.Runtime;

namespace Ink
{
	internal class CommandLinePlayer
	{
		public Story story { get; protected set; }
		public bool autoPlay { get; set; }
        public Parsed.Story parsedStory { get; set; }

        public CommandLinePlayer (Story story, bool autoPlay = false, Parsed.Story parsedStory = null)
		{
			this.story = story;
			this.autoPlay = autoPlay;
            this.parsedStory = parsedStory;

            _debugSourceListByLine = new List<DebugSourceRange> ();
		}

		public void Begin()
		{
            EvaluateStory ();

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

                        var inputParser = new InkParser (userInput);
                        var input = inputParser.CommandLineUserInput();

                        // Choice
                        if (input.choiceInput != null) {

                            choiceIdx = ((int)input.choiceInput) - 1;

                            if (choiceIdx < 0 || choiceIdx >= choices.Count) {
                                Console.WriteLine ("Choice out of range");
                            } else {
                                choiceIsValid = true;
                            }
                        }

                        // Help
                        else if (input.isHelp) {
                            Console.WriteLine ("Type a choice number, a divert (e.g. '-> myKnot'), an expression, or a variable assignment (e.g. 'x = 5')");
                        }

                        // Request for debug source line number
                        else if (input.debugSource != null) {
                            var offset = (int)input.debugSource;
                            var dm = DebugMetadataForContentAtOffset (offset);
                            if (dm != null)
                                Console.WriteLine (dm.ToString ());
                            else
                                Console.WriteLine ("Unknown source");
                        }

                        // User entered some ink
                        else if (input.userImmediateModeStatement != null ) {

                            var parsedObj = input.userImmediateModeStatement as Parsed.Object;

                            // Variable assignment: create in Parsed.Story as well as the Runtime.Story
                            // so that we don't get an error message during reference resolution
                            if( parsedObj is Parsed.VariableAssignment ) {
                                var varAssign = (Parsed.VariableAssignment) parsedObj;
                                if( varAssign.isNewTemporaryDeclaration ) {
                                    parsedStory.TryAddNewVariableDeclaration(varAssign);
                                }
                            }

                            parsedObj.parent = parsedStory;
                            var runtimeObj = parsedObj.runtimeObject;

                            parsedObj.ResolveReferences(parsedStory);

                            if( !parsedStory.hadError ) {

                                // Divert
                                if( parsedObj is Parsed.Divert ) {
                                    var parsedDivert = parsedObj as Parsed.Divert;
                                    userDivertedPath = parsedDivert.runtimeDivert.targetPath;
                                }

                                // Expression or variable assignment
                                else if( parsedObj is Parsed.Expression || parsedObj is Parsed.VariableAssignment ) {
                                    var result = story.EvaluateExpression((Container)runtimeObj);
                                    if( result != null ) {
                                        Console.WriteLine(result.ToString());
                                    }
                                }
                            } else {
                                parsedStory.ResetError();
                            }

                        }

                        else {
                            Console.WriteLine ("Unexpected input. Type 'help' or a choice number.");
                        }

                    } while(!choiceIsValid && userDivertedPath == null);

				}

                Console.ResetColor ();

                if (choiceIsValid) {
                    story.ChooseChoiceIndex (choiceIdx);
                } else if (userDivertedPath != null) {
                    story.ChoosePath (userDivertedPath);
                    userDivertedPath = null;
                }
                    
                EvaluateStory ();
			}
		}

        void EvaluateStory ()
        {
            _debugSourceListByLine.Clear ();

            while (story.canContinue) {

                story.Continue ();

                LogDebugSourceForLine ();

                Console.Write (story.currentText);

                if (story.hasError) {
                    foreach (var errorMsg in story.currentErrors) {
                        Console.WriteLine (errorMsg, ConsoleColor.Red);
                    }
                }
            }

            story.ResetErrors ();
        }

        void LogDebugSourceForLine ()
        {
            foreach (var outputObj in story.state.outputStream) {
                var textContent = outputObj as StringValue;
                if (textContent != null) {
                    var range = new DebugSourceRange ();
                    range.length = textContent.value.Length;
                    range.debugMetadata = textContent.debugMetadata;
                    _debugSourceListByLine.Add (range);

                }
            }
        }

        DebugMetadata DebugMetadataForContentAtOffset (int offset)
        {
            int currOffset = 0;

            foreach (var range in _debugSourceListByLine) {
                if (offset > currOffset && offset < currOffset + range.length)
                    return range.debugMetadata;
                currOffset += range.length;
            }

            return null;
        }

        internal struct DebugSourceRange
        {
            public int length;
            public DebugMetadata debugMetadata;
        }

        List<DebugSourceRange> _debugSourceListByLine;
	}


}

