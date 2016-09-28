using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Ink
{
	class CommandLineTool
	{
		class Options {
			public bool testMode;
            public bool stressTest;
            public bool verbose;
			public bool playMode;
			public string inputFile;
            public string outputFile;
            public bool countAllVisits;
            public bool keepOpenAfterStoryFinish;
		}

		public static int ExitCodeError = 1;

		public static void Main (string[] args)
		{
			new CommandLineTool(args);
		}

        void ExitWithUsageInstructions()
        {
            Console.WriteLine (
                "Usage: inklecate2 <options> <ink file> \n"+
                "   -o <filename>:   Output file name\n"+
                "   -c:              Count all visits to knots, stitches and weave points, not\n" +
                "                    just those referenced by TURNS_SINCE and read counts.\n" +
                "   -p:              Play mode\n"+
                "   -v:              Verbose mode - print compilation timings\n"+
                "   -x <pluginname>: Use external plugin. 'ChoiceListPlugin' is only available plugin right now.\n"+
                "   -t:              Test mode - loads up test.ink\n"+
                "   -s:              Stress test mode - generates test content and \n" +
                "                    times compilation\n" + 
                "   -k:              Keep inklecate running in play mode even after story is complete\n");
            Environment.Exit (ExitCodeError);
        }
            
		CommandLineTool(string[] args)
		{
            // Set console's output encoding to UTF-8
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (ProcessArguments (args) == false) {
                ExitWithUsageInstructions ();
            }

            if (opts.testMode) {
                opts.inputFile = "test.ink";
            }

            if (opts.inputFile == null) {
                ExitWithUsageInstructions ();
            }
                
            string inputString = null;
            string workingDirectory = Directory.GetCurrentDirectory();

            if (opts.outputFile == null)
                opts.outputFile = Path.ChangeExtension (opts.inputFile, ".ink.json");

            if( !Path.IsPathRooted(opts.outputFile) )
                opts.outputFile = Path.Combine (workingDirectory, opts.outputFile);

            if (opts.stressTest) {

                StressTestContentGenerator stressTestContent = null;
                TimeOperation ("Generating test content", () => {
                    stressTestContent = new StressTestContentGenerator (100);
                });

                Console.WriteLine ("Generated ~{0}k of test ink", stressTestContent.sizeInKiloChars);

                inputString = stressTestContent.content;

            } else {
                try {
                    string fullFilename = opts.inputFile;
                    if(!Path.IsPathRooted(fullFilename)) {
                        fullFilename = Path.Combine(workingDirectory, fullFilename);
                    }

                    // Make the working directory the directory for the root ink file,
                    // so that relative paths for INCLUDE files are correct.
                    workingDirectory = Path.GetDirectoryName(fullFilename);
                    Directory.SetCurrentDirectory(workingDirectory);

                    // Now make the input file relative to the working directory,
                    // but just getting the file's actual name.
                    opts.inputFile = Path.GetFileName(fullFilename);

                    inputString = File.ReadAllText(opts.inputFile);
                }
                catch {
                    Console.WriteLine ("Could not open file '" + opts.inputFile+"'");
                    Environment.Exit (ExitCodeError);
                }
            }

            InkParser parser = null;
            Parsed.Story parsedStory = null;
            Runtime.Story story = null;
            errors = new List<string> ();
            warnings = new List<string> ();
            authorMessages = new List<string> ();
            var pluginManager = new PluginManager (pluginNames);

            var inputIsJson = opts.inputFile.EndsWith (".json");

            // Loading a normal ink file (as opposed to an already compiled json file)
            if (!inputIsJson) {
                TimeOperation ("Creating parser", () => {
                    parser = new InkParser (inputString, opts.inputFile, OnError);
                });

                TimeOperation ("Parsing", () => {
                    parsedStory = parser.Parse ();
                });

                TimeOperation ("PostParsePlugins", () => {
                    pluginManager.PostParse(parsedStory);
                });

                if (parsedStory != null) {

                    if (opts.countAllVisits) {
                        parsedStory.countAllVisits = true;
                    }

                    TimeOperation ("Exporting runtime", () => {
                        story = parsedStory.ExportRuntime (OnError);
                    });

                    TimeOperation ("PostParsePlugins", () => {
                        pluginManager.PostExport(parsedStory, story);
                    });

                }

            } 

            // Opening up a compiled json file for playing
            else {
                story = new Runtime.Story (inputString);

                // No purpose for loading an already compiled file other than to play it
                opts.playMode = true;
            }

            PrintAllMessages ();

            if (story == null || errors.Count > 0) {
				Environment.Exit (ExitCodeError);
			}
                
            // JSON round trip testing
            //if (opts.testMode) {
            //    var jsonStr = story.ToJsonString ();
            //    Console.WriteLine (jsonStr);

            //    Console.WriteLine ("---------------------------------------------------");

            //    var reloadedStory = new Runtime.Story (jsonStr);
            //    var newJsonStr = reloadedStory.ToJsonString ();
            //    Console.WriteLine (newJsonStr);

            //    story = reloadedStory;
            //}

			// Play mode
            // Test mode may use "-tp" in commmand line args to specify that
            // the test script is also played
            if (opts.playMode) {

                _playing = true;

                // Always allow ink external fallbacks
                story.allowExternalFunctionFallbacks = true;

                var player = new CommandLinePlayer (story, false, parsedStory, opts.keepOpenAfterStoryFinish);

                //Capture a CTRL+C key combo so we can restore the console's foreground color back to normal when exiting
                Console.CancelKeyPress += OnExit;

                try {
                    player.Begin ();
                } catch (Runtime.StoryException e) {
                    if (e.Message.Contains ("Missing function binding")) {
                        OnError (e.Message, ErrorType.Error);
                        PrintAllMessages ();
                    } else {
                        throw e;
                    }
                }
            } 

            // Compile mode
            else {
                
                var jsonStr = story.ToJsonString ();

                try {
                    File.WriteAllText (opts.outputFile, jsonStr, System.Text.Encoding.UTF8);
                } catch {
                    Console.WriteLine ("Could not write to output file '" + opts.outputFile+"'");
                    Environment.Exit (ExitCodeError);
                }
            }
        }

        private void OnExit(object sender, ConsoleCancelEventArgs e)
        {
            Console.ResetColor();
        }
        void OnError(string message, ErrorType errorType)
        {
            switch (errorType) {
            case ErrorType.Author:
                authorMessages.Add (message);
                break;

            case ErrorType.Warning:
                warnings.Add (message);
                break;

            case ErrorType.Error:
                errors.Add (message);
                break;
            }

            // If you get an error while playing, just print immediately
            if( _playing ) PrintAllMessages ();
        }

        void PrintMessages(List<string> messageList, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;

            foreach (string msg in messageList) {
                Console.WriteLine (msg);
            }

            Console.ResetColor ();
        }

        void PrintAllMessages ()
        {
            PrintMessages (authorMessages, ConsoleColor.Green);
            PrintMessages (warnings, ConsoleColor.Blue);
            PrintMessages (errors, ConsoleColor.Red);

            authorMessages.Clear ();
            warnings.Clear ();
            errors.Clear ();
        }

        bool ProcessArguments(string[] args)
		{
            if (args.Length < 1) {
                opts = null;
                return false;
            }

			opts = new Options();
            pluginNames = new List<string> ();

            bool nextArgIsOutputFilename = false;
            bool nextArgIsPlugin = false;

			// Process arguments
            int argIdx = 0;
			foreach (string arg in args) {
                            
                if (nextArgIsOutputFilename) {
                    opts.outputFile = arg;
                    nextArgIsOutputFilename = false;
                } else if (nextArgIsPlugin) {
                    pluginNames.Add (arg);
                    nextArgIsPlugin = false;
                }

				// Options
				var firstChar = arg.Substring(0,1);
                if (firstChar == "-" && arg.Length > 1) {

                    for (int i = 1; i < arg.Length; ++i) {
                        char argChar = arg [i];

                        switch (argChar) {
                        case 't':
                            opts.testMode = true;
                            break;
                        case 's':
                            opts.testMode = true;
                            opts.stressTest = true;
                            opts.verbose = true;
                            break;
                        case 'p':
                            opts.playMode = true;
                            break;
                        case 'v':
                            opts.verbose = true;
                            break;
                        case 'o':
                            nextArgIsOutputFilename = true;   
                            break;
                        case 'c':
                            opts.countAllVisits = true;
                            break;
                        case 'x':
                            nextArgIsPlugin = true;
                            break;
                        case 'k':
                            opts.keepOpenAfterStoryFinish = true;
                            break;
                        default:
                            Console.WriteLine ("Unsupported argument type: '{0}'", argChar);
                            break;
                        }
                    }
                } 
                    
                // Last argument: input file
                else if( argIdx == args.Length-1 ) {
                    opts.inputFile = arg;
                }

                argIdx++;
			}

			return true;
		}

        void TimeOperation(string opDescription, Action op)
        {
            if (!opts.verbose) {
                op ();
                return;
            }

            Console.WriteLine ("{0}...", opDescription);

            var stopwatch = Stopwatch.StartNew ();
            op ();
            stopwatch.Stop ();

            long duration = stopwatch.ElapsedMilliseconds;

            if (duration > 500) {
                Console.WriteLine ("{0} took {1}s", opDescription, duration / 1000.0f);  
            } else {
                Console.WriteLine ("{0} took {1}ms", opDescription, duration);  
            }
        }

        Options opts;
        List<string> pluginNames;

        List<string> errors;
        List<string> warnings;
        List<string> authorMessages;

        bool _playing;
	}
}
