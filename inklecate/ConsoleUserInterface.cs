using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Ink.Runtime;
using Ink.Inklecate.Interaction;
using Ink.Inklecate.AutoPlay;
using Ink.Inklecate.OutputManagement;

namespace Ink.Inklecate
{
    /// <summary>The ConsoleUserInterface class encapsulates the functionality of the user interface run in the console.</summary>
    public class ConsoleUserInterface
    {
        #region Properties

        public IConsoleInteractable ConsoleInteractor { get; set; } = new ConsoleInteractor();
        public IChoiceGeneratable ChoiceGenerator { get; set; } = new ChoiceGenerator();
        public static IPlayerOutputManagable OutputManager { get; set; } = null; // default null because determined by flag

        public IInkCompiler Compiler { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        #endregion Properties

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="ConsoleUserInterface" /> class.</summary>
        /// <param name="compiler">The compiler.</param>
        public ConsoleUserInterface(IInkCompiler compiler)
        {
            Compiler = compiler;
        }

        #endregion Constructor

        #region Player interaction

        /// <summary>Begins the user interaction with the specified story.</summary>
        /// <param name="story">The story.</param>
        /// <param name="options">The options.</param>
        public void Begin(IStory story, ConsoleUserInterfaceOptions options)
        {
            if (story == null || options == null)
                return;

            SetOuputFormat(options);

            // Add a handeler for the story errors
            story.StoryError += Story_StoryError;



            EvaluateStory(story, options);

            while (story.currentChoices.Count > 0 || options.IsKeepRunningAfterStoryFinishedNeeded)
            {
                var choices = story.currentChoices;

                var choiceIdex = 0;
                bool choiceIsValid = false;
                string userDivertedPath = null;

                // autoPlay: Pick random choice
                if (options.IsAutoPlayActive)
                {
                    choiceIdex = ChoiceGenerator.GetRandomChoice(choices.Count);
                }
                else
                {
                    // Normal: Ask user for choice number
                    OutputManager.ShowChoices(choices, options);

                    do
                    {
                        OutputManager.RequestInput(options);

                        string userInput = Console.ReadLine();

                        // If we have null user input, it means that we're
                        // "at the end of the stream", or in other words, the input
                        // stream has closed, so there's nothing more we can do.
                        // We return immediately, since otherwise we get into a busy
                        // loop waiting for user input.
                        if (userInput == null)
                        {
                            OutputManager.ShowStreamError(options);
                            return;
                        }

                        var result = Compiler.ReadCommandLineInput(userInput);


                        if (result.output != null)
                        {
                            OutputManager.ShowOutputResult(options, result);
                        }

                        if (result.requestsExit)
                            return;

                        if (result.divertedPath != null)
                            userDivertedPath = result.divertedPath;

                        if (result.choiceIdx >= 0)
                        {
                            if (result.choiceIdx >= choices.Count)
                            {
                                OutputManager.ShowChoiceOutOffRange(options);
                            }
                            else
                            {
                                choiceIdex = result.choiceIdx;
                                choiceIsValid = true;
                            }
                        }

                    }
                    while (!choiceIsValid && userDivertedPath == null);

                }

                Console.ResetColor();

                if (choiceIsValid)
                {
                    story.ChooseChoiceIndex(choiceIdex);
                }
                else if (userDivertedPath != null)
                {
                    story.ChoosePathString(userDivertedPath);
                    userDivertedPath = null;
                }

                EvaluateStory(story, options);
            }
        }

        /// <summary>Sets the ouput format.</summary>
        /// <param name="options">The options.</param>
        public void SetOuputFormat(ConsoleUserInterfaceOptions options)
        {
            // Instrument the story with the kind of output that is requested.
            if (options.IsJsonOutputNeeded)
                OutputManager = new JsonPlayerOutputManager(ConsoleInteractor);
            else
                OutputManager = new ConsolePlayerOutputManager(ConsoleInteractor);
        }

        #endregion Player interaction

        #region Story Evaluation

        /// <summary>Evaluates the story.</summary>
        /// <param name="story">The story.</param>
        /// <param name="options">The options.</param>
        private void EvaluateStory(IStory story, ConsoleUserInterfaceOptions options)
        {
            while (story.canContinue)
            {
                story.Continue();

                Compiler.RetrieveDebugSourceForLatestContent();

                OutputManager.ShowCurrentText(story, options);

                var tags = story.currentTags;
                if (tags.Count > 0)
                {
                    OutputManager.ShowTags(options, tags);
                }

                if ((Errors.Count > 0 || Warnings.Count > 0))
                {
                    OutputManager.ShowWarningsAndErrors(Warnings, Errors, options);
                }

                Errors.Clear();
                Warnings.Clear();
            }

            if (story.currentChoices.Count == 0 && options.IsKeepRunningAfterStoryFinishedNeeded)
            {
                OutputManager.ShowEndOfStory(options);
            }
        }

        #endregion Story Evaluation

        #region Event handling

        /// <summary>Handles the StoryError event of the Story control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StoryErrorEventArgs" /> instance containing the event data.</param>
        private void Story_StoryError(object sender, StoryErrorEventArgs e)
        {
            if (e.ErrorType == StoryErrorType.Error)
                Errors.Add(e.Message);
            else
                Warnings.Add(e.Message);
        }

        #endregion Event handling
    }
}