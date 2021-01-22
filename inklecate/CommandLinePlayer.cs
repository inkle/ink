using System;
using System.Collections.Generic;
using Ink.Runtime;

namespace Ink
{
	public class CommandLinePlayer
	{
		public Story story { get; protected set; }
		public bool autoPlay { get; set; }
        public bool keepOpenAfterStoryFinish { get; set; }

        public CommandLinePlayer (Story story, bool autoPlay = false, Compiler compiler = null, bool keepOpenAfterStoryFinish = false, bool jsonOutput = false)
		{
			this.story = story;
            this.story.onError += OnStoryError;
			this.autoPlay = autoPlay;
            _compiler = compiler;
            _jsonOutput = jsonOutput;
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
                    
                    if( !_jsonOutput ) {
                        Console.ForegroundColor = ConsoleColor.Blue;

                        // Add extra newline to ensure that the choice is
                        // on a separate line.
                        Console.WriteLine ();

                        int i = 1;
                        foreach (Choice choice in choices) {
                            Console.WriteLine ("{0}: {1}", i, choice.text);
                            i++;
                        }
                    }

                    else {
                        var writer = new Runtime.SimpleJson.Writer();
                        writer.WriteObjectStart();
                        writer.WritePropertyStart("choices");
                        writer.WriteArrayStart();
                        foreach(var choice in choices) {
                            writer.Write(choice.text);
                        }
                        writer.WriteArrayEnd();
                        writer.WritePropertyEnd();
                        writer.WriteObjectEnd();
                        Console.WriteLine(writer.ToString());
                    }


                    do {
                        if( !_jsonOutput ) {
                            // Prompt
                            Console.Write("?> ");
                        }
                        
                        else {
                            // Johnny Five, he's alive!
                            Console.Write("{\"needInput\": true}");
                        }

                        string userInput = Console.ReadLine ();

                        // If we have null user input, it means that we're
                        // "at the end of the stream", or in other words, the input
                        // stream has closed, so there's nothing more we can do.
                        // We return immediately, since otherwise we get into a busy
                        // loop waiting for user input.
                        if (userInput == null) {
                            if( _jsonOutput ) {
                                Console.WriteLine ("{\"close\": true}");
                            } else {
                                Console.WriteLine ("<User input stream closed.>");
                            }
                            return;
                        }

                        var result = ReadCommandLineInput (userInput);

                        if (result.output != null) {
                            if( _jsonOutput ) {
                                var writer = new Runtime.SimpleJson.Writer();
                                writer.WriteObjectStart();
                                writer.WriteProperty("cmdOutput", result.output);
                                writer.WriteObjectEnd();
                                Console.WriteLine(writer.ToString());
                            } else {
                                Console.WriteLine (result.output);
                            }
                        }

                        if (result.requestsExit)
                            return;

                        if (result.divertedPath != null)
                            userDivertedPath = result.divertedPath;

                        if (result.choiceIdx >= 0) {
                            if (result.choiceIdx >= choices.Count) {
                                if( !_jsonOutput ) // fail silently in json mode
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

                if (_compiler != null)
                  _compiler.RetrieveDebugSourceForLatestContent ();

                if( _jsonOutput ) {
                    var writer = new Runtime.SimpleJson.Writer();
                    writer.WriteObjectStart();
                    writer.WriteProperty("text", story.currentText);
                    writer.WriteObjectEnd();
                    Console.WriteLine (writer.ToString());
                } else {
                    Console.Write (story.currentText);
                }

                var tags = story.currentTags;
                if (tags.Count > 0) {
                    if( _jsonOutput ) {
                        var writer = new Runtime.SimpleJson.Writer();
                        writer.WriteObjectStart();
                        writer.WritePropertyStart("tags");
                        writer.WriteArrayStart();
                        foreach(var tag in tags) {
                            writer.Write(tag);
                        }
                        writer.WriteArrayEnd();
                        writer.WritePropertyEnd();
                        writer.WriteObjectEnd();
                        Console.WriteLine(writer.ToString());
                    } else {
                        Console.WriteLine ("# tags: " + string.Join (", ", tags));
                    }
                }

                Runtime.SimpleJson.Writer issueWriter = null;
                if( _jsonOutput && (_errors.Count > 0 || _warnings.Count > 0) ) {
                    issueWriter = new Runtime.SimpleJson.Writer();
                    issueWriter.WriteObjectStart();
                    issueWriter.WritePropertyStart("issues");
                    issueWriter.WriteArrayStart();

                    if( _errors.Count > 0 ) {
                        foreach (var errorMsg in _errors) {
                            issueWriter.Write (errorMsg);
                        }
                    }
                    if( _warnings.Count > 0 ) {
                        foreach (var warningMsg in _warnings) {
                            issueWriter.Write (warningMsg);
                        }
                    }

                    issueWriter.WriteArrayEnd();
                    issueWriter.WritePropertyEnd();
                    issueWriter.WriteObjectEnd();
                    Console.WriteLine(issueWriter.ToString());
                }

                if (_errors.Count > 0 && !_jsonOutput ) {
                    foreach (var errorMsg in _errors) {
                        Console.WriteLine (errorMsg, ConsoleColor.Red);
                    }
                }

                if (_warnings.Count > 0 && !_jsonOutput) {
                    foreach (var warningMsg in _warnings) {
                        Console.WriteLine (warningMsg, ConsoleColor.Blue);
                    }
                }

                _errors.Clear();
                _warnings.Clear();
            }

            if (story.currentChoices.Count == 0 && keepOpenAfterStoryFinish) {
                if( _jsonOutput ) {
                    Console.WriteLine("{\"end\": true}");
                } else {
                    Console.WriteLine ("--- End of story ---");
                }
            }
        }

        void OnStoryError(string msg, ErrorType type)
        {
            if( type == ErrorType.Error )
                _errors.Add(msg);
            else
                _warnings.Add(msg);
        }

        Compiler.CommandLineInputResult ReadCommandLineInput (string userInput) {
            var inputParser = new InkParser (userInput);
            var inputResult = inputParser.CommandLineUserInput ();
            var result = new Compiler.CommandLineInputResult ();

            // Choice
            if (inputResult.choiceInput != null) {
                result.choiceIdx = ((int)inputResult.choiceInput) - 1;
                return result;
            }

            // Help
            if (inputResult.isHelp) {
                result.output = "Type a choice number, a divert (e.g. '-> myKnot'), an expression, or a variable assignment (e.g. 'x = 5')";
                return result;
            }

            // Quit
            if (inputResult.isExit) {
                result.requestsExit = true;
                return result;
            }

            // If the compiler is available give it a chance to handle
            // the input.
            if (_compiler != null) {
                var compilerResult = _compiler.HandleInput(inputResult);
                if (compilerResult != null) {
                    return compilerResult;
                }
            }

            result.output = "Unexpected input. Type 'help' or a choice number.";
            return result;
        }

        Compiler _compiler;
        bool _jsonOutput;
        List<string> _errors = new List<string>();
        List<string> _warnings = new List<string>();
	}


}

