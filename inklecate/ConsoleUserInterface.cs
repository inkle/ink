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
using System.Linq.Expressions;

namespace Ink.Inklecate
{
    /// <summary>The ConsoleUserInterface class encapsulates the functionality of the user interface run in the console.</summary>
    public class ConsoleUserInterface
    {
        #region Properties

        public IConsoleInteractable ConsoleInteractor { get; set; } = new ConsoleInteractor();
        public IChoiceGeneratable ChoiceGenerator { get; set; } = new ChoiceGenerator();
        public IPlayerOutputManagable OutputManager { get; set; } = null; // default null because determined by flag

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

            SetOutputFormat(options);

            // Add a handeler for the story errors
            story.StoryError += StoryErrorHandler;


            EvaluateStory(story, options);

            bool continueAfterThisPoint = false;
            while (!continueAfterThisPoint && (story.HasCurrentChoices || options.IsKeepRunningAfterStoryFinishedNeeded))
            {
                continueAfterThisPoint = RunStoryUntilContinuationPoint(story, options);
            }
        }

        /// <summary>Runs the story until continuation point.</summary>
        /// <param name="story">The story.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private bool RunStoryUntilContinuationPoint(IStory story, ConsoleUserInterfaceOptions options)
        {
            var choices = story.currentChoices;

            if (options.IsAutoPlayActive)
            {
                // autoPlay: Pick random choice
                var choiceIndex = ChoiceGenerator.GetRandomChoice(choices.Count);

                Console.ResetColor();
            }
            else
            {
                // Normal: Ask user for choice number
                OutputManager.ShowChoices(choices, options);

                var uiResult = GetPropperUserInteractionResult(choices, options);
                if (uiResult == null)
                    return false;

                Console.ResetColor();

                if (uiResult.IsInputStreamClosed)
                {
                    return false;
                }
                else if (uiResult.IsValidChoice)
                {
                    story.ChooseChoiceIndex(uiResult.ChosenIdex);
                }
                else if (uiResult.DivertedPath != null)
                {
                    story.ChoosePathString(uiResult.DivertedPath);
                    uiResult.DivertedPath = null;
                }
            }

            EvaluateStory(story, options);

            return true;
        }

        /// <summary>Gets a propper user interaction result.</summary>
        /// <param name="choices">The choices.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public UserInteractionResult GetPropperUserInteractionResult(List<Choice> choices, ConsoleUserInterfaceOptions options)
        {
            if (choices == null || options == null)
                return null;
            
            UserInteractionResult uiResult;
            do
            {
                uiResult = GetUserInteractionResult(choices, options);
            }
            while (   !(uiResult == null                    // if the uiResult is null something is seriously wrong and we stop asking and go on.
                     || uiResult.IsInputStreamClosed        // We keep asking the user again for input as long as the stream is active
                     || uiResult.IsExitRequested            // no exit is requested 
                     || uiResult.IsValidChoice              // no valid choice was made
                     || uiResult.DivertedPath != null));    // and no diverted path was present

            return uiResult;
        }

        /// <summary>Gets the user interaction result.</summary>
        /// <param name="choices">The choices.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public virtual UserInteractionResult GetUserInteractionResult(List<Choice> choices, ConsoleUserInterfaceOptions options)
        {
            if (choices == null || options == null)
                return null;

            var uiResult = new UserInteractionResult();

            OutputManager.RequestInput(options);

            var userInput = OutputManager.GetUserInput();

            // If we have null user input, it means that we're
            // "at the end of the stream", or in other words, the input
            // stream has closed, so there's nothing more we can do.
            // We return immediately, since otherwise we get into a busy
            // loop waiting for user input.
            if (userInput == null)
            {
                OutputManager.ShowStreamError(options);
                uiResult.IsInputStreamClosed = true;
            }
            else
            {
                var result = Compiler.ReadCommandLineInput(userInput);
                if (result == null)
                    return null;

                ProcessCommandLineInputResult(uiResult, result, choices);

                if (uiResult.Output != null)
                {
                    OutputManager.ShowOutputResult(result, options);
                }

                if (!uiResult.IsValidChoice)
                {
                    // The choice is only valid if it's a valid index.
                    OutputManager.ShowChoiceOutOffRange(options);
                }
            }

            return uiResult;
        }

        /// <summary>Processes the command line input result.</summary>
        /// <param name="uiResult">The UI result.</param>
        /// <param name="result">The result.</param>
        /// <param name="choices">The choices.</param>
        public virtual void ProcessCommandLineInputResult(UserInteractionResult uiResult, Compiler.CommandLineInputResult result, List<Choice> choices)
        {
            if (uiResult == null || result == null)
                return;
            
            uiResult.IsExitRequested = result.requestsExit;
            uiResult.ChosenIdex = result.choiceIdx;
            uiResult.DivertedPath = result.divertedPath;
            uiResult.Output = result.output;
            

            if (choices != null && result.choiceIdx >= 0 && result.choiceIdx < choices.Count)
            {
                // The choice is only valid if it's a valid index.
                uiResult.IsValidChoice = true;
            }
        }

        /// <summary>Sets the output format.</summary>
        /// <param name="options">The options.</param>
        public void SetOutputFormat(ConsoleUserInterfaceOptions options)
        {
            // Instrument the story with the kind of output that is requested.
            if (options == null || !options.IsJsonOutputNeeded)
                OutputManager = new ConsolePlayerOutputManager(ConsoleInteractor);
            else
                OutputManager = new JsonPlayerOutputManager(ConsoleInteractor);
        }

        #endregion Player interaction

        #region Story Evaluation

        /// <summary>Evaluates the story.</summary>
        /// <param name="story">The story.</param>
        /// <param name="options">The options.</param>
        public virtual void EvaluateStory(IStory story, ConsoleUserInterfaceOptions options)
        {
            if (story == null || options == null)
                return;

            while (story.canContinue)
            {
                EvaluateNextStoryLine(story, options);
            }

            if (!story.HasCurrentChoices && options.IsKeepRunningAfterStoryFinishedNeeded)
            {
                OutputManager.ShowEndOfStory(options);
            }
        }

        /// <summary>Evaluates the next story line.</summary>
        /// <param name="story">The story.</param>
        /// <param name="options">The options.</param>
        public virtual void EvaluateNextStoryLine(IStory story, ConsoleUserInterfaceOptions options)
        {
            if (story == null || options == null)
                return;

            story.Continue();

            Compiler.RetrieveDebugSourceForLatestContent();

            OutputManager.ShowCurrentText(story, options);

            if (story.HasCurrentTags)
            {
                OutputManager.ShowTags(story.currentTags, options);
            }

            if ((Errors.Count > 0 || Warnings.Count > 0))
            {
                OutputManager.ShowWarningsAndErrors(Warnings, Errors, options);
            }

            Errors.Clear();
            Warnings.Clear();
        }

        #endregion Story Evaluation

        #region Event handling

        /// <summary>Handles the StoryError event of the Story control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StoryErrorEventArgs" /> instance containing the event data.</param>
        public void StoryErrorHandler(object sender, StoryErrorEventArgs e)
        {
            if (e == null)
                return;

            if (e.ErrorType == StoryErrorType.Error)
                Errors.Add(e.Message);
            else
                Warnings.Add(e.Message);
        }

        #endregion Event handling
    }
}