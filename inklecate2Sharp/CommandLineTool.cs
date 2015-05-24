using System;
using System.IO;

namespace Inklewriter
{
	class CommandLineTool
	{
		struct Options {
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
			Options opts = ProcessArguments(args);

			string inputString = File.ReadAllText(opts.inputFile);

			InkParser parser = new InkParser(inputString);
			Parsed.Story parsedStory = parser.Parse();
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

		Options ProcessArguments(string[] args)
		{
			Options opts = new Options();

			// Process arguments
			foreach (string arg in args) {

				// Options
				var firstChar = arg.Substring(0,1);
				if (firstChar == "-" && arg.Length > 1) {

					char secondChar = arg [1];

					switch (secondChar) {
					case 't':
						//opts.testMode = true;
						opts.playMode = true;
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

			return opts;
		}

	}
}
