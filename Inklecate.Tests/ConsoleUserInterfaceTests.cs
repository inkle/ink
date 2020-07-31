using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
using NSubstitute.ExceptionExtensions;

using Ink;
using Ink.Inklecate;
using Ink.Inklecate.Interaction;
using Ink.Inklecate.OutputManagement;
using Ink.Runtime;
using Ink.Parsed;

namespace Ink.Inklecate.Tests
{
    public class ConsoleUserInterfaceTests
    {
        public class ConstructorTests
        {
            [Fact]
            public void With_NullArgument()
            {
                // Arrange

                // Act
                var consoleUI = new ConsoleUserInterface();

                // Assert
                consoleUI.Should().NotBeNull("because it was just created.");
            }
        }

        public class BeginTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.Begin(null, null, null);

                // Assert
                consoleUI.Should().NotBeNull("because it was just created.");
            }

            [Fact]
            public void With_NoCurrentChoices()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                story.HasCurrentChoices.Returns(x => false);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.When(x => x.SetOutputFormat(options)).DoNotCallBase();
                consoleUI.When(x => x.EvaluateStory(story, options)).DoNotCallBase();

                // Act
                consoleUI.Begin(story, parsedFiction, options);

                // Assert
                consoleUI.Received(0).RunStoryUntilContinuationPoint(story, parsedFiction, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_WithLoopStopByCurrentChoices()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                story.HasCurrentChoices.Returns(x => true, x => false);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.When(x => x.SetOutputFormat(options)).DoNotCallBase();
                consoleUI.When(x => x.EvaluateStory(story, options)).DoNotCallBase();
                consoleUI.Configure().RunStoryUntilContinuationPoint(story, parsedFiction, options).Returns(x => true, x => true);

                // Act
                consoleUI.Begin(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).RunStoryUntilContinuationPoint(story, parsedFiction, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_WithLoopStopByContinuationPoints()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                story.HasCurrentChoices.Returns(x => true, x => true, x => true);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.When(x => x.SetOutputFormat(options)).DoNotCallBase();
                consoleUI.When(x => x.EvaluateStory(story, options)).DoNotCallBase();
                consoleUI.Configure().RunStoryUntilContinuationPoint(story, parsedFiction, options).Returns(x => true, x => false);

                // Act
                consoleUI.Begin(story, parsedFiction, options);

                // Assert
                consoleUI.Received(2).RunStoryUntilContinuationPoint(story, parsedFiction, options); // the do while loop should only have run 1 time
            }
        }

        public class RunStoryUntilContinuationPointTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.RunStoryUntilContinuationPoint(null, null, null);

                // Assert
                consoleUI.Should().NotBeNull("because it was just created.");
            }

            [Fact]
            public void With_Autoplay()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsAutoPlayActive = true };

                var choiceGenerator = Substitute.For<Ink.Inklecate.AutoPlay.IChoiceGeneratable>();
                choiceGenerator.GetRandomChoice(default).ReturnsForAnyArgs(1);
                var consoleInteractor = Substitute.For<IConsoleInteractable>();

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.WhenForAnyArgs(x => x.EvaluateStory(default, default)).DoNotCallBase();
                consoleUI.ChoiceGenerator = choiceGenerator;
                consoleUI.ConsoleInteractor = consoleInteractor;

                // Act
                consoleUI.RunStoryUntilContinuationPoint(story, parsedFiction, options);

                // Assert
                choiceGenerator.Received(1).GetRandomChoice(Arg.Any<int>());
                consoleInteractor.Received(1).ResetConsoleColor();
                consoleUI.Received(1).EvaluateStory(story, options);
            }

            [Fact]
            public void With_Play_And_InteractionResutNull()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsAutoPlayActive = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Interpreter = interpreter;
                consoleUI.ConsoleInteractor = consoleInteractor;
                consoleUI.OutputManager = outputManager;
                consoleUI.WhenForAnyArgs(x => x.EvaluateStory(default, default)).DoNotCallBase();
                UserInteractionResult uiResult = null;
                consoleUI.Configure().GetPropperUserInteractionResult(story, parsedFiction, options).Returns(x => uiResult);

                // Act
                consoleUI.RunStoryUntilContinuationPoint(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetPropperUserInteractionResult(story, parsedFiction, options);
                consoleInteractor.Received(1).ResetConsoleColor();
            }

            [Fact]
            public void With_Play_And_InteractionResut_With_InputStreamClosed()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsAutoPlayActive = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Interpreter = interpreter;
                consoleUI.ConsoleInteractor = consoleInteractor;
                consoleUI.OutputManager = outputManager;
                consoleUI.WhenForAnyArgs(x => x.EvaluateStory(default, default)).DoNotCallBase();
                var uiResult = new UserInteractionResult()
                {
                    IsInputStreamClosed = true
                };
                consoleUI.Configure().GetPropperUserInteractionResult(story, parsedFiction, options).Returns(x => uiResult);

                // Act
                consoleUI.RunStoryUntilContinuationPoint(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetPropperUserInteractionResult(story, parsedFiction, options);
                consoleInteractor.Received(1).ResetConsoleColor();
            }

            [Fact]
            public void With_Play_And_InteractionResut_With_ValidChoice()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsAutoPlayActive = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Interpreter = interpreter;
                consoleUI.ConsoleInteractor = consoleInteractor;
                consoleUI.OutputManager = outputManager;
                consoleUI.WhenForAnyArgs(x => x.EvaluateStory(default, default)).DoNotCallBase();
                var uiResult = new UserInteractionResult()
                {
                    IsValidChoice = true
                };
                consoleUI.Configure().GetPropperUserInteractionResult(story, parsedFiction, options).Returns(x => uiResult);

                // Act
                consoleUI.RunStoryUntilContinuationPoint(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetPropperUserInteractionResult(story, parsedFiction, options);
                consoleInteractor.Received(1).ResetConsoleColor();
                story.Received(1).ChooseChoiceIndex(uiResult.ChosenIndex);
                consoleUI.Received(1).EvaluateStory(story, options);
            }

            [Fact]
            public void With_Play_And_InteractionResut_With_DivertedPath()
            {
                // Arrange
                const string divertedPath = "Test";
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                //story.WhenForAnyArgs(x => x.ChoosePathString(divertedPath)).DoNotCallBase();
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions() { IsAutoPlayActive = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Interpreter = interpreter;
                consoleUI.ConsoleInteractor = consoleInteractor;
                consoleUI.OutputManager = outputManager;
                consoleUI.WhenForAnyArgs(x => x.EvaluateStory(default, default)).DoNotCallBase();
                var uiResult = new UserInteractionResult()
                {
                    DivertedPath = divertedPath
                };
                consoleUI.Configure().GetPropperUserInteractionResult(story, parsedFiction, options).Returns(x => uiResult);

                // Act
                consoleUI.RunStoryUntilContinuationPoint(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetPropperUserInteractionResult(story, parsedFiction, options);
                consoleInteractor.Received(1).ResetConsoleColor();
                story.Received(1).ChoosePathString(divertedPath);
                consoleUI.Received(1).EvaluateStory(story, options);
                uiResult.DivertedPath.Should().BeNull("because the diverted path should be reset after use.");
            }
        }

        public class GetPropperUserInteractionResultTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Configure().GetUserInteractionResult(null, null, null).Returns(x => null);

                // Act
                UserInteractionResult uiResult = consoleUI.GetPropperUserInteractionResult(null, null, null);

                // Assert
                uiResult.Should().BeNull("because the GetUserInteractionResult has returned null.");
            }

            [Fact]
            public void With_NullUserInteractionResult()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Configure().GetUserInteractionResult(story, parsedFiction, options).Returns(x => null);

                // Act
                consoleUI.GetPropperUserInteractionResult(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetUserInteractionResult(story, parsedFiction, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_ClosedInputStream()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    IsInputStreamClosed = true
                };

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Configure().GetUserInteractionResult(story, parsedFiction, options).Returns(x => uiResult);

                // Act
                consoleUI.GetPropperUserInteractionResult(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetUserInteractionResult(story, parsedFiction, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_ExitRequested()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    IsExitRequested = true
                };

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Configure().GetUserInteractionResult(story, parsedFiction, options).Returns(x => null);

                // Act
                consoleUI.GetPropperUserInteractionResult(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetUserInteractionResult(story, parsedFiction, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_ValidChoice()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    IsValidChoice = true
                };

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Configure().GetUserInteractionResult(story, parsedFiction, options).Returns(x => null);

                // Act
                consoleUI.GetPropperUserInteractionResult(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetUserInteractionResult(story, parsedFiction, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_DivertedPath()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    DivertedPath = "testpath"
                };

                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.Configure().GetUserInteractionResult(story, parsedFiction, options).Returns(x => null);

                // Act
                consoleUI.GetPropperUserInteractionResult(story, parsedFiction, options);

                // Assert
                consoleUI.Received(1).GetUserInteractionResult(story, parsedFiction, options); // the do while loop should only have run 1 time
            }
        }

        public class GetUserInteractionResultTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var interpreter = Substitute.For<IInputInterpreter>();
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                consoleUI.GetUserInteractionResult(null, null, null);

                // Assert
            }

            [Fact]
            public void With_NullUserInput()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                story.HasCurrentChoices.Returns(x => true, x => false);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                string userInput = null;
                outputManager.GetUserInput().Returns(x => userInput);
                var interpreter = Substitute.For<IInputInterpreter>();

                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(story, parsedFiction, options);

                // Assert
                uiResult.IsInputStreamClosed.Should().BeTrue("because recieving a null from the OutputManager should be considert as the stream having closed.");
            }

            [Fact]
            public void With_UserInput_And_InterpretCommandLineInputNullResult()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                story.HasCurrentChoices.Returns(x => true, x => false);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var userInput = "TestInput";
                outputManager.GetUserInput().Returns(x => userInput);
                var interpreter = Substitute.For<IInputInterpreter>();
                InputInterpretationResult result = null;
                interpreter.InterpretCommandLineInput(userInput, parsedFiction, story).Returns(x => result);

                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(story, parsedFiction, options);

                // Assert
                uiResult.Should().BeNull("because a UserInteractionResult can not be made from a null Compiler.CommandLineInputResult.");
            }

            [Fact]
            public void With_UserInputAndValidChoiceIndex()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                story.HasCurrentChoices.Returns(x => true, x => false);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var userInput = "TestInput";
                outputManager.GetUserInput().Returns(x => userInput);
                var interpreter = Substitute.For<IInputInterpreter>();
                var result = new InputInterpretationResult
                {
                    requestsExit = false,
                    choiceIdx = 0, // the first valid index
                    divertedPath = null,
                    output = "TestOutput",
                };
                interpreter.InterpretCommandLineInput(userInput, parsedFiction, story).Returns(x => result);

                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(story, parsedFiction, options);

                // Assert
                uiResult.IsValidChoice.Should().BeTrue("because the choiceIdx 0 is correct when the choices list contains 1 item.");
                outputManager.ReceivedWithAnyArgs(1).ShowOutputResult(default, options);
            }

            [Fact]
            public void With_UserInputAndChoiceIndexBelowRange()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                story.HasCurrentChoices.Returns(x => true, x => false);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var userInput = "TestInput";
                outputManager.GetUserInput().Returns(x => userInput);
                var interpreter = Substitute.For<IInputInterpreter>();
                var result = new InputInterpretationResult
                {
                    requestsExit = false,
                    choiceIdx = -1, // below range
                    divertedPath = null,
                    output = "TestOutput",
                };
                interpreter.InterpretCommandLineInput(userInput, parsedFiction, story).Returns(x => result);

                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(story, parsedFiction, options);

                // Assert
                uiResult.IsValidChoice.Should().BeFalse("because the choiceIdx -1 is never correct for a choices list.");
                outputManager.ReceivedWithAnyArgs(1).ShowOutputResult(default, options);
                outputManager.Received(1).ShowChoiceOutOffRange(options);
            }

            [Fact]
            public void With_UserInputAndChoiceIndexAboveRange()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);
                story.HasCurrentChoices.Returns(x => true, x => false);
                var parsedFiction = Substitute.For<Ink.Parsed.IFiction>();
                var options = new ConsoleUserInterfaceOptions();

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var userInput = "TestInput";
                outputManager.GetUserInput().Returns(x => userInput);
                var interpreter = Substitute.For<IInputInterpreter>();
                var result = new InputInterpretationResult
                {
                    requestsExit = false,
                    choiceIdx = 1, // above range
                    divertedPath = null,
                    output = "TestOutput",
                };
                interpreter.InterpretCommandLineInput(userInput, parsedFiction, story).Returns(x => result);

                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(story, parsedFiction, options);

                // Assert
                uiResult.IsValidChoice.Should().BeFalse("because the choiceIdx 0 is correct when the choices list contains 1 item.");
                outputManager.ReceivedWithAnyArgs(1).ShowOutputResult(default, options);
                outputManager.Received(1).ShowChoiceOutOffRange(options);
            }
        }

        public class ProcessCommandLineInputResultTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.ProcessCommandLineInputResult(null, null, null);

                // Assert
            }

            [Fact]
            public void With_ValidChoice()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);

                var uiResult = new UserInteractionResult() { IsInputStreamClosed = true };
                var path = "test path";
                var testOutput = "test output";
                var index = 0;
                var result = new InputInterpretationResult() { choiceIdx = index, divertedPath = path, requestsExit = true, output = testOutput };

                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.ProcessCommandLineInputResult(uiResult, result, story);

                // Assert
                uiResult.ChosenIndex.Should().Be(index, "because the choiceIdx was set to 1.");
                uiResult.IsExitRequested.Should().BeTrue("because the requestsExit was set to true.");
                uiResult.DivertedPath.Should().Be(path, "because the output was set to that path.");
                uiResult.Output.Should().Be(testOutput, "because the output property was set to that object.");
                uiResult.IsValidChoice.Should().BeTrue("because the choiceIdx was set to 0, wich is the only valid choice.");
                uiResult.IsInputStreamClosed.Should().BeTrue("because the IsInputStreamClosed property was set to true.");
            }

            [Fact]
            public void With_ChoiceBelowRange()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);

                var uiResult = new UserInteractionResult() { IsInputStreamClosed = true };
                var path = "test path";
                var testOutput = "test output";
                var index = -1;
                var result = new InputInterpretationResult() { choiceIdx = index, divertedPath = path, requestsExit = true, output = testOutput };

                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.ProcessCommandLineInputResult(uiResult, result, story);

                // Assert
                uiResult.ChosenIndex.Should().Be(index, "because the choiceIdx was set to 1.");
                uiResult.IsExitRequested.Should().BeTrue("because the requestsExit was set to true.");
                uiResult.DivertedPath.Should().Be(path, "because the output was set to that path.");
                uiResult.Output.Should().Be(testOutput, "because the output property was set to that object.");
                uiResult.IsValidChoice.Should().BeFalse("because the choiceIdx was set to -1, wich is below the range of valid choices.");
                uiResult.IsInputStreamClosed.Should().BeTrue("because the IsInputStreamClosed property was set to true.");
            }

            [Fact]
            public void With_ChoiceAboveRange()
            {
                // Arrange
                var choice = new Ink.Runtime.Choice() { text = "Test" };
                var choices = new List<Ink.Runtime.Choice>() { choice };
                var story = Substitute.For<IStory>();
                story.currentChoices.Returns(x => choices);

                var uiResult = new UserInteractionResult() { IsInputStreamClosed = true };
                var path = "test path";
                var testOutput = "test output";
                var index = 1;
                var result = new InputInterpretationResult() { choiceIdx = index, divertedPath = path, requestsExit = true, output = testOutput };

                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.ProcessCommandLineInputResult(uiResult, result, story);

                // Assert
                uiResult.ChosenIndex.Should().Be(index, "because the choiceIdx was set to 1.");
                uiResult.IsExitRequested.Should().BeTrue("because the requestsExit was set to true.");
                uiResult.DivertedPath.Should().Be(path, "because the output was set to that path.");
                uiResult.Output.Should().Be(testOutput, "because the output property was set to that object.");
                uiResult.IsValidChoice.Should().BeFalse("because the choiceIdx was set to 1, wich is below the range of valid choices.");
                uiResult.IsInputStreamClosed.Should().BeTrue("because the IsInputStreamClosed property was set to true.");
            }
        }

        public class SetOuputFormatTests
        {
            [Fact]
            public void With_NullArgument()
            {
                // Arrange
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                var consoleUI = new ConsoleUserInterface() { ConsoleInteractor = consoleInteractor };

                // Act
                consoleUI.SetOutputFormat(null);

                // Assert
                consoleUI.OutputManager.Should().BeOfType<ConsolePlayerOutputManager>("because the default output should be human readable on the console.");
                consoleUI.OutputManager.ConsoleInteractor.Should().Be(consoleInteractor, "because the console interactor object was set on the console user interface and the output manager should have gotten it from there.");
            }

            [Fact]
            public void With_IsJsonOutputNeededTrue()
            {
                // Arrange
                var options = new ConsoleUserInterfaceOptions() { IsJsonOutputNeeded = true };

                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                var consoleUI = new ConsoleUserInterface() { ConsoleInteractor = consoleInteractor };

                // Act
                consoleUI.SetOutputFormat(options);

                // Assert
                consoleUI.OutputManager.Should().BeOfType<JsonPlayerOutputManager>("because Json output is needed so the output be Json and done by a Json output manager.");
                consoleUI.OutputManager.ConsoleInteractor.Should().Be(consoleInteractor, "because the console interactor object was set on the console user interface and the output manager should have gotten it from there.");
            }

            [Fact]
            public void With_IsJsonOutputNeededFalse()
            {
                // Arrange
                var options = new ConsoleUserInterfaceOptions() { IsJsonOutputNeeded = false };

                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                var consoleUI = new ConsoleUserInterface() { ConsoleInteractor = consoleInteractor };

                // Act
                consoleUI.SetOutputFormat(options);

                // Assert
                consoleUI.OutputManager.Should().BeOfType<ConsolePlayerOutputManager>("because without the need for Json output the output should be human readable on the console.");
                consoleUI.OutputManager.ConsoleInteractor.Should().Be(consoleInteractor, "because the console interactor object was set on the console user interface and the output manager should have gotten it from there.");
            }
        }

        public class EvaluateStoryTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.EvaluateStory(null, null);

                // Assert
            }

            [Fact]
            public void With_NoContinueNoChoicesAndKeepRunning()
            {
                // Arrange
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = true };
                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => false);
                story.HasCurrentChoices.Returns(x => false);

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                consoleUI.EvaluateStory(story, options);

                // Assert
                outputManager.Received().ShowEndOfStory(options);
            }

            [Fact]
            public void EvaluateOnce()
            {
                var options = new ConsoleUserInterfaceOptions();
                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);

                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>();
                consoleUI.WhenForAnyArgs(x => x.EvaluateNextStoryLine(story, options)).DoNotCallBase();
                consoleUI.Interpreter = interpreter;

                // Act
                consoleUI.EvaluateStory(story, options);

                // Assert
                consoleUI.Received().EvaluateNextStoryLine(story, options);
            }
        }

        public class EvaluateNextStoryLineTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = new ConsoleUserInterface() { Interpreter = interpreter };

                // Act
                consoleUI.EvaluateNextStoryLine(null, null);

                // Assert
            }

            [Fact]
            public void With_DefaultArguments()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                story.Received().Continue();
                outputManager.Received().ShowCurrentText(story, options);
            }

            [Fact]
            public void With_Tag()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);
                var tags = new List<string>() { "SomeTag" };
                story.currentTags.Returns(x => tags);
                story.HasCurrentTags.Returns(x => true);
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                story.Received().Continue();
                outputManager.Received().ShowCurrentText(story, options);
                outputManager.Received().ShowTags(tags, options);
            }

            [Fact]
            public void With_Warning()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };
                consoleUI.Warnings.Add("SomeWarning");

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                story.Received().Continue();
                outputManager.Received().ShowCurrentText(story, options);
                outputManager.Received().ShowWarningsAndErrors(consoleUI.Warnings, consoleUI.Errors, options);
            }

            [Fact]
            public void With_Error()
            {
                // Arrange
                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var interpreter = Substitute.For<IInputInterpreter>();
                var consoleUI = new ConsoleUserInterface() { OutputManager = outputManager, Interpreter = interpreter };
                consoleUI.Errors.Add("SomeError");

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                story.Received().Continue();
                outputManager.Received().ShowCurrentText(story, options);
                outputManager.Received().ShowWarningsAndErrors(consoleUI.Warnings, consoleUI.Errors, options);
            }
        }

        public class StoryErrorHandlerTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.StoryErrorHandler(null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_ErrorMessage()
            {
                // Arrange
                const string message = "Test";
                StoryErrorEventArgs e = new StoryErrorEventArgs() { ErrorType = StoryErrorType.Error, Message = message };

                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.StoryErrorHandler(null, e);

                // Assert
                consoleUI.Errors.Should().HaveCount(1, "because only 1 message was added.");
                consoleUI.Errors.Should().ContainMatch(message, "because only 1 message was added.");
            }

            [Fact]
            public void With_WarningMessage()
            {
                // Arrange
                const string message = "Test";
                StoryErrorEventArgs e = new StoryErrorEventArgs() { ErrorType = StoryErrorType.Warning, Message = message };

                var consoleUI = new ConsoleUserInterface();

                // Act
                consoleUI.StoryErrorHandler(null, e);

                // Assert
                consoleUI.Warnings.Should().HaveCount(1, "because only 1 message was added.");
                consoleUI.Warnings.Should().ContainMatch(message, "because only 1 message was added.");
            }
        }
    }
}
