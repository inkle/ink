using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Inklewriter
{
	class CommandLineTool
	{
		class Options {
			public bool testMode;
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
            Options opts;

            if (ProcessArguments (args, out opts) == false) {
                Console.WriteLine (
                    "Usage: inklecate2 [-p] <ink file> \n"+
                    "   -p:    Play mode");
                Environment.Exit (ExitCodeError);
            }

            string inputString = null;
            try {
                inputString = File.ReadAllText(opts.inputFile);
            }
            catch {
                Console.WriteLine ("Could not open file '" + opts.inputFile+"'");
                Environment.Exit (ExitCodeError);
            }
			

			InkParser parser = new InkParser(inputString);
			Parsed.Story parsedStory = parser.Parse();
            if (parsedStory == null) {
                Environment.Exit (ExitCodeError);
            }

			Runtime.Story story = parsedStory.ExportRuntime ();
			if (story == null) {
				Environment.Exit (ExitCodeError);
			}

			// Randomly play through
			if (opts.playMode || opts.testMode) {

				var player = new CommandLinePlayer (story, autoPlay:opts.testMode);
				player.Begin ();

			}
		}

        bool ProcessArguments(string[] args, out Options opts)
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

					char secondChar = arg [1];

					switch (secondChar) {
					case 't':
						opts.testMode = true;
						opts.inputFile = "test.ink";
						break;
					case 'p':
						opts.playMode = true;
						break;
					default:
						Console.WriteLine ("Unsupported argument type: '{0}'", secondChar);
						break;
					}
				}
			}

            if (opts.testMode == false) {
                opts.inputFile = args.Last ();
            }

			return true;
		}

	}
}
