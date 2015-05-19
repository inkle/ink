using System;
using System.IO;

namespace inklecate2Sharp
{
	class CommandLineTool
	{
		struct Options {
			public bool testMode;
			public string inputFile;
		}

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

			if (opts.testMode) {
				story.Begin ();
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

					var secondChar = arg.Substring(1,1);

					// Test mode
					if (secondChar == "t") {
						opts.testMode = true;
						opts.inputFile = "test.ink";
					}
				}
			}

			return opts;
		}

	}
}
