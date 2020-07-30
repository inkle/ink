using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Ink.Inklecate.Interaction;
using Ink.Inklecate.OutputManagement;

namespace Ink.Inklecate
{
    /// <summary>The CommandLineTool class encapsulates the functionality of the tool that is started on the command line.</summary>
    public partial class CommandLineTool
    {
        #region Properties

        public IFileSystemInteractable FileSystemInteractor { get; set; } = new FileSystemInteractor();
        public ICompilerInteractable CompilerInteractor { get; set; } = new CompilerInteractor();
        public IEngineInteractable EngineInteractor { get; set; } = new EngineInteractor();
        public IConsoleInteractable ConsoleInteractor { get; set; } = new ConsoleInteractor();
        public IToolOutputManagable OutputManager { get; set; } = null; // default null because determined by flag
        public IConsoleUserInterface UserInterface { get; set; } = new ConsoleUserInterface();

        public ParsedCommandLineOptions parsedOptions;
        public CommandLineToolOptions toolOptions;

        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> AuthorMessages { get; set; } = new List<string>();

        #endregion Properties

        /// <summary>Defines the entry point of the application.</summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var tool = new CommandLineTool();

            // Add default values by processing the configured options.
            var toolOptions = tool.CreateCommandLineToolOptions(args);
            tool.SetOuputFormat(toolOptions);
            tool.Run(toolOptions);
        }

        #region Instructions

        /// <summary>Exits with the usage instructions.</summary>
        public void ExitWithUsageInstructions()
        {
            string usageInstructions =
                "Usage: inklecate2 <options> <ink file> \n" +
                "   -o <filename>:   Output file name\n" +
                "   -c:              Count all visits to knots, stitches and weave points, not\n" +
                "                    just those referenced by TURNS_SINCE and read counts.\n" +
                "   -p:              Play mode\n" +
                "   -j:              Output in JSON format (for communication with tools like Inky)\n" +
                "   -s:              Print stats about story including word count in JSON format\n" +
                "   -v:              Verbose mode - print compilation timings\n" +
                "   -k:              Keep inklecate running in play mode even after story is complete\n";
            ConsoleInteractor.WriteInformation(usageInstructions);
            ConsoleInteractor.EnvironmentExitWithCodeError1();
        }

        #endregion Instructions

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="CommandLineTool" /> class.</summary>
        public CommandLineTool()
        {
            // Nothing here, witch is kind of shocking if you know what we started out with.
        }

        #endregion Constructor

        #region ArgumentProcessing

        /// <summary>Creates the command line tool options.</summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private CommandLineToolOptions CreateCommandLineToolOptions(string[] args)
        {
            toolOptions = new CommandLineToolOptions();

            // Getting the current dir early is better in ustable situations.
            string startingDirectory = Directory.GetCurrentDirectory();

            parsedOptions = ParseArguments(args);
            if (parsedOptions == null || !parsedOptions.IsInputPathGiven)
                ExitWithUsageInstructions();

            ProcesOutputFilePath(parsedOptions, toolOptions, startingDirectory);
            ProcesInputFilePath(parsedOptions, toolOptions, startingDirectory);
            ProcesFlags(parsedOptions, toolOptions);

            return toolOptions;
        }

        /// <summary>Parses the command line arguments.</summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>An options object.</returns>
        public ParsedCommandLineOptions ParseArguments(string[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            var options = new ParsedCommandLineOptions();

            bool expectingOutputFilename = false;
            bool expectingPluginName = false;

            // Process arguments
            int lastArgumentIndex = args.Length - 1;
            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i];

                if (i == lastArgumentIndex)
                {
                    // When on the last argument we assume it's the file.
                    options.InputFilePath = argument;
                }
                else if (expectingOutputFilename)
                {
                    // When a outputfilename flag preceded the current argument we assume it's the outputfilename.
                    options.OutputFilePath = argument;
                    expectingOutputFilename = false;
                }
                else if (expectingPluginName)
                {
                    // When a pluginname flag preceded the current argument we assume it's a pluginname.
                    options.PluginNames.Add(argument);
                    expectingPluginName = false;
                }
                else if (argument.StartsWith("-"))
                {
                    // Determin options  
                    switch (argument)
                    {
                        case "-p": options.IsPlayMode = true; break;
                        case "-v": options.IsVerboseMode = true; break;
                        case "-j": options.IsJsonOutputNeeded = true; break;
                        case "-s": options.IsOnlyShowJsonStatsActive = true; break;
                        case "-o": expectingOutputFilename = true; break;
                        case "-c": options.IsCountAllVisitsNeeded = true; break;
                        case "-x": expectingPluginName = true; break;
                        case "-k": options.IsKeepOpenAfterStoryFinishNeeded = true; break;
                        default: ConsoleInteractor.WriteWarning("Unsupported argument flag: '{0}'", argument); break;
                    }
                }
                else
                {
                    ConsoleInteractor.WriteWarning("Unexpected argument: '{0}'", argument); break;
                }
            }
            return options;
        }

        /// <summary>Proceses the output file path.</summary>
        /// <param name="parsedOptions">The parsed options.</param>
        /// <param name="processedOptions">The processed options.</param>
        /// <param name="startingDirectory">The starting directory.</param>
        public void ProcesOutputFilePath(ParsedCommandLineOptions parsedOptions, CommandLineToolOptions processedOptions, string startingDirectory)
        {
            // Without a parsed object and a input file path we can't do anything.
            if (parsedOptions == null || processedOptions == null)
                return;

            // Generate an outputpath when none is given.
            if (!string.IsNullOrEmpty(parsedOptions.OutputFilePath))
            {
                // if the GIVEN outputpath is not rooted we strip of the filename and tag the directory on it.
                processedOptions.RootedOutputFilePath = Path.IsPathRooted(parsedOptions.OutputFilePath)
                    ? parsedOptions.OutputFilePath
                    : Path.Combine(startingDirectory, parsedOptions.OutputFilePath);
            }
            else
            {
                processedOptions.GeneratedOutputFilePath = Path.ChangeExtension(parsedOptions.InputFilePath, ".ink.json");

                // if the GENERATED outputpath is not rooted we strip of the filename and tag the directory on it.
                processedOptions.RootedOutputFilePath = Path.IsPathRooted(processedOptions.GeneratedOutputFilePath)
                    ? processedOptions.GeneratedOutputFilePath
                    : Path.Combine(startingDirectory, processedOptions.GeneratedOutputFilePath);
            }
        }

        /// <summary>Proceses the input file path.</summary>
        /// <param name="parsedOptions">The parsed options.</param>
        /// <param name="processedOptions">The processed options.</param>
        /// <param name="startingDirectory">The starting directory.</param>
        public void ProcesInputFilePath(ParsedCommandLineOptions parsedOptions, CommandLineToolOptions processedOptions, string startingDirectory)
        {
            // Without a parsed object and a input file path we can't do anything.
            if (parsedOptions == null || processedOptions == null)
                return;

            processedOptions.InputFilePath = parsedOptions.InputFilePath;

            // Get the file's actual name, needed for reading after the working directory has changed.
            processedOptions.InputFileName = Path.GetFileName(parsedOptions.InputFilePath);

            processedOptions.RootedInputFilePath = Path.IsPathRooted(parsedOptions.InputFilePath)
                ? parsedOptions.InputFilePath
                : Path.Combine(startingDirectory, parsedOptions.InputFilePath);

            processedOptions.InputFileDirectory = Path.GetDirectoryName(processedOptions.RootedInputFilePath);
        }

        /// <summary>Proceses the flags by copying them from the parsed options to the processed options so we can always compare them.</summary>
        /// <param name="parsedOptions">The parsed options.</param>
        /// <param name="processedOptions">The processed options.</param>
        public void ProcesFlags(ParsedCommandLineOptions parsedOptions, CommandLineToolOptions processedOptions)
        {
            // Without a parsed object and a input file path we can't do anything.
            if (parsedOptions == null || processedOptions == null)
                return;

            // Most of the flags are not changed while running exept for IsPlayMode.
            processedOptions.IsPlayMode = parsedOptions.IsPlayMode;
            processedOptions.IsVerboseMode = parsedOptions.IsVerboseMode;
            processedOptions.IsCountAllVisitsNeeded = parsedOptions.IsCountAllVisitsNeeded;
            processedOptions.IsOnlyShowJsonStatsActive = parsedOptions.IsOnlyShowJsonStatsActive;
            processedOptions.IsJsonOutputNeeded = parsedOptions.IsJsonOutputNeeded;
            processedOptions.IsKeepRunningAfterStoryFinishedNeeded = parsedOptions.IsKeepOpenAfterStoryFinishNeeded;
            processedOptions.PluginNames = parsedOptions.PluginNames;
        }

        #endregion ArgumentProcessing

        #region Run

        /// <summary>Set the output format.</summary>
        /// <param name="options"></param>
        public void SetOuputFormat(CommandLineToolOptions options)
        {
            // Set console's output encoding to UTF-8
            ConsoleInteractor.SetEncodingToUtF8();

            if (options.IsJsonOutputNeeded)
                OutputManager = new JsonToolOutputManager(ConsoleInteractor);
            else
                OutputManager = new ConsoleToolOutputManager(ConsoleInteractor);
        }

        /// <summary>Does a Run with the specified options.</summary>
        /// <param name="options">The options.</param>
        public void Run(CommandLineToolOptions options)
        {
            if (options == null)
            {
                ConsoleInteractor.WriteErrorMessage("Missing options object");
                ConsoleInteractor.EnvironmentExitWithCodeError1();
            }

            // Read the file content
            string fileContent = ReadFileText(toolOptions.InputFileDirectory, toolOptions.InputFileName);

            SetOuputFormat(options);

            Parsed.Fiction parsedFiction;
            var story = CreateStory(fileContent, options, out parsedFiction);

            // If we have a story without errors we have compiled succesfully.
            var compileSuccess = !(story == null || Errors.Count > 0);
            OutputManager.ShowCompileSuccess(options, compileSuccess);

            // If we only wanted to show the stats we are done now.
            if (options.IsOnlyShowJsonStatsActive)
                return;


            PrintAllMessages();

            // Without having succesfully compiled we can not go on to play or flush Json.
            if (!compileSuccess)
                ConsoleInteractor.EnvironmentExitWithCodeError1();


            if (options.IsPlayMode)
            {
                PlayStory(story, parsedFiction, options);
            }
            else
            {
                WriteStoryToJsonFile(story, options);
            }
        }

        /// <summary>Reads the file text.</summary>
        /// <param name="inputFileDirectory">The input file directory.</param>
        /// <param name="inputFileName">Name of the input file.</param>
        /// <returns>The file text content.</returns>
        public string ReadFileText(string inputFileDirectory, string inputFileName)
        {
            if (string.IsNullOrEmpty(inputFileDirectory) || string.IsNullOrEmpty(inputFileName))
                return null;

            try
            {
                // Make the working directory the directory for the root ink file,
                // so that relative paths for INCLUDE files are correct.
                FileSystemInteractor.SetCurrentDirectory(inputFileDirectory);
            }
            catch (Exception exception)
            {
                ConsoleInteractor.WriteErrorMessage("Could not set directory '{0}'", exception);
                ConsoleInteractor.EnvironmentExitWithCodeError1();
            }

            string fileText = null;
            try
            {
                fileText = FileSystemInteractor.ReadAllTextFromFile(inputFileName);
            }
            catch (Exception exception)
            {
                ConsoleInteractor.WriteErrorMessage("Could not open file '{0}'", exception);
                ConsoleInteractor.EnvironmentExitWithCodeError1();
            }
            return fileText;
        }

        /// <summary>Creates the story from the file contents.</summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="options">The options.</param>
        /// <param name="compiler">The compiler.</param>
        /// <param name="compileSuccess">if set to <c>true</c> [compile success].</param>
        /// <param name="finished">if set to <c>true</c> [finished].</param>
        /// <returns></returns>
        public Runtime.IStory CreateStory(string fileContent, CommandLineToolOptions options, out Parsed.Fiction parsedFiction)
        {
            Runtime.IStory story = null;

            if (!options.IsInputFileJson)
            {
                // Loading a normal ink file (as opposed to an already compiled json file)
                var compiler = CreateCompiler(fileContent, options);

                if (options.IsOnlyShowJsonStatsActive)
                {
                    ShowStats(compiler, options);
                    parsedFiction = null;
                }
                else
                {
                    //Parsed.Fiction parsedFiction = null;
                    // Full compile
                    story = compiler.Compile(out parsedFiction);
                }
            }
            else
            {
                story = CreateStoryFromJson(fileContent, options);
                parsedFiction = null;
            }

            return story;
        }

        /// <summary>Creates the compiler with specific compiler options.</summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private IInkCompiler CreateCompiler(string fileContent, CommandLineToolOptions options)
        {            
            CompilerOptions compilerOptions = new CompilerOptions
            {
                sourceFilename = options.InputFilePath,
                pluginNames = options.PluginNames,
                countAllVisits = options.IsCountAllVisitsNeeded,
            };
            return CompilerInteractor.CreateCompiler(fileContent, compilerOptions);
        }

        /// <summary>Shows the stats of the compiled story.</summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="options">The options.</param>
        private void ShowStats(IInkCompiler compiler, CommandLineToolOptions options)
        {
            // Only want stats, don't need to code-gen
            var parsedStory = compiler.Parse();

            // Print any errors
            PrintAllMessages();

            // Generate stats, then print as JSON
            var stats = Ink.Stats.Generate(compiler.ParsedFiction);

            OutputManager.ShowStats(options, stats);
        }

        /// <summary>Creates the story from json.</summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private Runtime.IStory CreateStoryFromJson(string fileContent, CommandLineToolOptions options)
        {
            // Opening up a compiled json file for playing
            var story = EngineInteractor.CreateStoryFromJson(fileContent);

            // No purpose for loading an already compiled file other than to play it
            options.IsPlayMode = true;
            return story;
        }

        /// <summary>Plays the story.</summary>
        /// <param name="story">The story.</param>
        /// <param name="compiler">The compiler.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="Exception"></exception>
        public void PlayStory(Runtime.IStory story, Parsed.Fiction parsedFiction, CommandLineToolOptions options)
        {
            // Always allow ink external fallbacks
            story.allowExternalFunctionFallbacks = true;

            //Capture a CTRL+C key combo so we can restore the console's foreground color back to normal when exiting
            ConsoleInteractor.ResetColorOnCancelKeyPress();

            try
            {
                ConsoleUserInterfaceOptions uiOptions = new ConsoleUserInterfaceOptions()
                {
                    IsAutoPlayActive = false,
                    IsKeepRunningAfterStoryFinishedNeeded = options.IsKeepRunningAfterStoryFinishedNeeded,
                    IsJsonOutputNeeded = options.IsJsonOutputNeeded
                };
                UserInterface.Begin(story, parsedFiction, uiOptions);
            }
            catch (Runtime.StoryException e)
            {
                if (e.Message.Contains("Missing function binding"))
                {
                    Errors.Add(e.Message);

                    // If you get an error while playing, just print immediately
                    PrintAllMessages();
                }
                else
                {
                    throw e;
                }
            }
            catch (Exception e)
            {
                string storyPath = "<END>";
                var path = story.state.currentPathString;
                if (path != null)
                {
                    storyPath = path.ToString();
                }
                throw new Exception(e.Message + " (Internal story path: " + storyPath + ")", e);
            }
        }

        /// <summary>Writes the compiled story to a json file.</summary>
        /// <param name="story">The story.</param>
        /// <param name="options">The options.</param>
        public void WriteStoryToJsonFile(Runtime.IStory story, CommandLineToolOptions options)
        {
            // Compile mode
            var jsonStr = story.ToJson();

            try
            {
                FileSystemInteractor.WriteAllTextToFile(options.RootedOutputFilePath, jsonStr, System.Text.Encoding.UTF8);

                OutputManager.ShowExportComplete(options);

            }
            catch
            {
                ConsoleInteractor.WriteErrorMessage("Could not write to output file '{0}'", options.RootedOutputFilePath);
                ConsoleInteractor.EnvironmentExitWithCodeError1();
            }
        }

        #endregion Run

        #region Error handling

        /// <summary>Prints all messages.</summary>
        private void PrintAllMessages()
        {
            OutputManager.PrintAllMessages(AuthorMessages, Warnings, Errors);

            AuthorMessages.Clear();
            Warnings.Clear();
            Errors.Clear();
        }

        #endregion Error handling
    }
}