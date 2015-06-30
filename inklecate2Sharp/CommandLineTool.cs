using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter
{
	class CommandLineTool
	{
		class Options {
			public bool testMode;
            public bool stressTest;
            public bool verbose;
			public bool playMode;
			public string inputFile;
		}

		public static int ExitCodeError = 1;

		public static void Main (string[] args)
		{
			new CommandLineTool(args);
		}
            
		CommandLineTool(string[] args)
		{
            if (ProcessArguments (args) == false) {
                Console.WriteLine (
                    "Usage: inklecate2 <options> <ink file> \n"+
                    "   -p:    Play mode\n"+
                    "   -v:    Verbose mode - print compilation timings\n"+
                    "   -t:    Test mode - loads up test.ink\n"+
                    "   -s:    Stress test mode - generates test content and times compilation\n");
                Environment.Exit (ExitCodeError);
            }

            string inputString = null;

            if (opts.stressTest) {

                StressTestContentGenerator stressTestContent = null;
                TimeOperation ("Generating test content", () => {
                    stressTestContent = new StressTestContentGenerator (100);
                });

                Console.WriteLine ("Generated ~{0}k of test ink", stressTestContent.sizeInKiloChars);

                inputString = stressTestContent.content;

            } else {
                try {
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

            TimeOperation ("Creating parser", () => {
                parser = new InkParser (inputString, opts.inputFile);
            });

            TimeOperation ("Parsing", () => {
                parsedStory = parser.Parse();
            });

            if (parsedStory == null) {
                Environment.Exit (ExitCodeError);
            }

            TimeOperation ("Exporting runtime", () => {
                story = parsedStory.ExportRuntime ();
            });

			if (story == null) {
				Environment.Exit (ExitCodeError);
			}

			// Randomly play through
			if (opts.playMode || opts.testMode) {

                if (opts.testMode) {
                    story.dontCatchRuntimeExceptions = true;
                }

				var player = new CommandLinePlayer (story, false, parsedStory);
				player.Begin ();

			}
		}

        bool ProcessArguments(string[] args)
		{
            if (args.Length < 1) {
                opts = null;
                return false;
            }

			opts = new Options();

			// Process arguments
			foreach (string arg in args) {

				// Options
				var firstChar = arg.Substring(0,1);
				if (firstChar == "-" && arg.Length > 1) {

                    for (int i = 1; i < arg.Length; ++i) {
                        char argChar = arg [i];

                        switch (argChar) {
                        case 't':
                            opts.testMode = true;
                            opts.inputFile = "test.ink";
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
                        default:
                            Console.WriteLine ("Unsupported argument type: '{0}'", argChar);
                            break;
                        }
                    }
					
				}
			}

            if (opts.testMode == false) {
                opts.inputFile = args.Last ();
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
	}
}
