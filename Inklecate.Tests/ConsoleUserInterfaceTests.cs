using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Ink.Inklecate;
using Ink.Inklecate.Interaction;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Ink;
using Ink.Inklecate.OutputManagement;
using Ink.Runtime;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;

namespace Inklecate.Tests
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
                var consoleUI = new ConsoleUserInterface(null);

                // Assert
                consoleUI.Compiler.Should().BeNull("because the null value was given as the compiler parameter.");
            }
        }

        public class BeginTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);

                // Act
                consoleUI.Begin(null, null);

                // Assert
            }

            [Fact]
            public void With_StoryMockAndDefaultOptions()
            {
                // Arrange
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>(compiler);
                consoleUI.WhenForAnyArgs(x => x.EvaluateStory(default, default)).DoNotCallBase();

                var story = Substitute.For<IStory>();
                var options = new ConsoleUserInterfaceOptions();

                // Act
                consoleUI.Begin(story, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
            }
        }

        public class GetPropperUserInteractionResultTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);

                // Act
                consoleUI.GetPropperUserInteractionResult(null, null);

                // Assert
            }

            [Fact]
            public void With_NullUserInteractionResult()
            {
                // Arrange
                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult();

                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>(compiler);
                consoleUI.Configure().GetUserInteractionResult(choices, options).Returns(x => null);

                // Act
                consoleUI.GetPropperUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                consoleUI.Received(1).GetUserInteractionResult(choices, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_ClosedInputStream()
            {
                // Arrange
                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    IsInputStreamClosed = true
                };

                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>(compiler);
                consoleUI.Configure().GetUserInteractionResult(choices, options).Returns(uiResult);

                // Act
                consoleUI.GetPropperUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                consoleUI.Received(1).GetUserInteractionResult(choices, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_ExitRequested()
            {
                // Arrange
                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    IsExitRequested = true
                };

                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>(compiler);
                consoleUI.Configure().GetUserInteractionResult(choices, options).Returns(uiResult);

                // Act
                consoleUI.GetPropperUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                consoleUI.Received(1).GetUserInteractionResult(choices, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_ValidChoice()
            {
                // Arrange
                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    IsValidChoice = true
                };

                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>(compiler);
                consoleUI.Configure().GetUserInteractionResult(choices, options).Returns(uiResult);

                // Act
                consoleUI.GetPropperUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                consoleUI.Received(1).GetUserInteractionResult(choices, options); // the do while loop should only have run 1 time
            }

            [Fact]
            public void With_DivertedPath()
            {
                // Arrange
                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();
                var uiResult = new UserInteractionResult()
                {
                    DivertedPath = "testpath"
                };

                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>(compiler);
                consoleUI.Configure().GetUserInteractionResult(choices, options).Returns(uiResult);

                // Act
                consoleUI.GetPropperUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                consoleUI.Received(1).GetUserInteractionResult(choices, options); // the do while loop should only have run 1 time
            }
        }
    

        public class GetUserInteractionResultTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);

                // Act
                consoleUI.GetUserInteractionResult(null, null);

                // Assert
            }

            [Fact]
            public void With_NullUserInput()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                outputManager.GetUserInput().Returns(x => null);
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                uiResult.IsInputStreamClosed.Should().BeTrue("because recieving a null from the OutputManager should be considert as the stream having closed.");
            }

            [Fact]
            public void With_UserInput_And_NullCompilerReadCommandLineInput()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var testInput = "TestInput";
                outputManager.GetUserInput().Returns(x => testInput);
                var compiler = Substitute.For<IInkCompiler>();
                compiler.ReadCommandLineInput(testInput).Returns(x => null);
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                uiResult.Should().BeNull("because a UserInteractionResult can not be made from a null Compiler.CommandLineInputResult.");
            }

            [Fact]
            public void With_UserInputAndValidChoiceIndex()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var testInput = "TestInput";
                outputManager.GetUserInput().Returns(x => testInput);
                var compiler = Substitute.For<IInkCompiler>();
                var commandLineInputResult = new InputInterpretationResult
                {
                    requestsExit = false,
                    choiceIdx = 0, // the first valid index
                    divertedPath = null,
                    output = "TestOutput",                    
                };
                compiler.ReadCommandLineInput(testInput).Returns(x => commandLineInputResult);
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                uiResult.IsValidChoice.Should().BeTrue("because the choiceIdx 0 is correct when the choices list contains 1 item.");
                outputManager.ReceivedWithAnyArgs(1).ShowOutputResult(default, options);
            }

            [Fact]
            public void With_UserInputAndChoiceIndexBelowRange()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var testInput = "TestInput";
                outputManager.GetUserInput().Returns(x => testInput);
                var compiler = Substitute.For<IInkCompiler>();
                var commandLineInputResult = new InputInterpretationResult
                {
                    requestsExit = false,
                    choiceIdx = -1, // below range
                    divertedPath = null,
                    output = "TestOutput",
                };
                compiler.ReadCommandLineInput(testInput).Returns(x => commandLineInputResult);
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                uiResult.IsValidChoice.Should().BeFalse("because the choiceIdx -1 is never correct for a choices list.");
                outputManager.ReceivedWithAnyArgs(1).ShowOutputResult(default, options);
                outputManager.Received(1).ShowChoiceOutOffRange(options);
            }

            [Fact]
            public void With_UserInputAndChoiceIndexAboveRange()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var testInput = "TestInput";
                outputManager.GetUserInput().Returns(x => testInput);
                var compiler = Substitute.For<IInkCompiler>();
                var commandLineInputResult = new InputInterpretationResult
                {
                    requestsExit = false,
                    choiceIdx = 1, // above range
                    divertedPath = null,
                    output = "TestOutput",
                };
                compiler.ReadCommandLineInput(testInput).Returns(x => commandLineInputResult);
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                var choice = new Choice() { text = "Test" };
                var choices = new List<Choice>() { choice };
                var options = new ConsoleUserInterfaceOptions();

                // Act
                var uiResult = consoleUI.GetUserInteractionResult(choices, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
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
                var consoleUI = new ConsoleUserInterface(null);

                // Act
                consoleUI.ProcessCommandLineInputResult(null, null, null);

                // Assert
            }

            [Fact]
            public void With_ValidChoice()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);

                var uiResult = new UserInteractionResult() { IsInputStreamClosed = true };
                var path = "test path";
                var testOutput = "test output";
                var index = 0;
                var result = new InputInterpretationResult() { choiceIdx = index, divertedPath = path, requestsExit = true, output = testOutput };
                var choice = new Choice();
                var choices = new List<Choice>() { choice };

                // Act
                consoleUI.ProcessCommandLineInputResult(uiResult, result, choices);

                // Assert
                uiResult.ChosenIdex.Should().Be(index, "because the choiceIdx was set to 1.");
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
                var consoleUI = new ConsoleUserInterface(null);

                var uiResult = new UserInteractionResult() { IsInputStreamClosed = true };
                var path = "test path";
                var testOutput = "test output";
                var index = -1;
                var result = new InputInterpretationResult() { choiceIdx = index, divertedPath = path, requestsExit = true, output = testOutput };
                var choice = new Choice();
                var choices = new List<Choice>() { choice };

                // Act
                consoleUI.ProcessCommandLineInputResult(uiResult, result, choices);

                // Assert
                uiResult.ChosenIdex.Should().Be(index, "because the choiceIdx was set to 1.");
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
                var consoleUI = new ConsoleUserInterface(null);

                var uiResult = new UserInteractionResult() { IsInputStreamClosed = true };
                var path = "test path";
                var testOutput = "test output";
                var index = 1;
                var result = new InputInterpretationResult() { choiceIdx = index, divertedPath = path, requestsExit = true, output = testOutput };
                var choice = new Choice();
                var choices = new List<Choice>() { choice };

                // Act
                consoleUI.ProcessCommandLineInputResult(uiResult, result, choices);

                // Assert
                uiResult.ChosenIdex.Should().Be(index, "because the choiceIdx was set to 1.");
                uiResult.IsExitRequested.Should().BeTrue("because the requestsExit was set to true.");
                uiResult.DivertedPath.Should().Be(path, "because the output was set to that path.");
                uiResult.Output.Should().Be(testOutput, "because the output property was set to that object.");
                uiResult.IsValidChoice.Should().BeFalse("because the choiceIdx was set to 1, wich is below the range of valid choices.");
                uiResult.IsInputStreamClosed.Should().BeTrue("because the IsInputStreamClosed property was set to true.");
            }
        }

        public class EvaluateStoryTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler);

                // Act
                consoleUI.EvaluateStory(null, null);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
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
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                // Act
                consoleUI.EvaluateStory(story, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                outputManager.Received().ShowEndOfStory(options);
            }

            [Fact]
            public void EvaluateOnce()
            {
                var options = new ConsoleUserInterfaceOptions();
                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);

                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = Substitute.ForPartsOf<ConsoleUserInterface>(compiler);
                consoleUI.WhenForAnyArgs(x => x.EvaluateNextStoryLine(story, options)).DoNotCallBase();

                // Act
                consoleUI.EvaluateStory(story, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                consoleUI.Received().EvaluateNextStoryLine(story, options);
            }
        }

        public class EvaluateNextStoryLineTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler);

                // Act
                consoleUI.EvaluateNextStoryLine(null, null);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
            }

            [Fact]
            public void With_DefaultArguments()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                var story = Substitute.For<IStory>();
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                story.Received().Continue();
                compiler.Received().RetrieveDebugSourceForLatestContent();
                outputManager.Received().ShowCurrentText(story, options);
            }

            [Fact]
            public void With_Tag()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };

                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);
                var tags = new List<string>() { "SomeTag" };
                story.currentTags.Returns(x => tags);
                story.HasCurrentTags.Returns(x => true);
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                story.Received().Continue();
                compiler.Received().RetrieveDebugSourceForLatestContent();
                outputManager.Received().ShowCurrentText(story, options);
                outputManager.Received().ShowTags(tags, options);
            }

            [Fact]
            public void With_Warning()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };
                consoleUI.Warnings.Add("SomeWarning");

                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                story.Received().Continue();
                compiler.Received().RetrieveDebugSourceForLatestContent();
                outputManager.Received().ShowCurrentText(story, options);
                outputManager.Received().ShowWarningsAndErrors(consoleUI.Warnings, consoleUI.Errors, options);
            }

            [Fact]
            public void With_Error()
            {
                // Arrange
                var outputManager = Substitute.For<IPlayerOutputManagable>();
                var compiler = Substitute.For<IInkCompiler>();
                var consoleUI = new ConsoleUserInterface(compiler) { OutputManager = outputManager };
                consoleUI.Errors.Add("SomeError");

                var story = Substitute.For<IStory>();
                story.canContinue.Returns(x => true, x => false);
                var options = new ConsoleUserInterfaceOptions() { IsKeepRunningAfterStoryFinishedNeeded = false };

                // Act
                consoleUI.EvaluateNextStoryLine(story, options);

                // Assert
                consoleUI.Compiler.Should().Be(compiler, "because the compiler object was given as the compiler parameter.");
                story.Received().Continue();
                compiler.Received().RetrieveDebugSourceForLatestContent();
                outputManager.Received().ShowCurrentText(story, options);
                outputManager.Received().ShowWarningsAndErrors(consoleUI.Warnings, consoleUI.Errors, options);
            }
        }

        public class SetOuputFormatTests
        {
            [Fact]
            public void With_NullArgument()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                consoleUI.ConsoleInteractor = consoleInteractor;

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
                var consoleUI = new ConsoleUserInterface(null);
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                consoleUI.ConsoleInteractor = consoleInteractor;
                var options = new ConsoleUserInterfaceOptions() { IsJsonOutputNeeded = true };

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
                var consoleUI = new ConsoleUserInterface(null);
                var consoleInteractor = Substitute.For<IConsoleInteractable>();
                consoleUI.ConsoleInteractor = consoleInteractor;
                var options = new ConsoleUserInterfaceOptions() { IsJsonOutputNeeded = false };

                // Act
                consoleUI.SetOutputFormat(options);

                // Assert
                consoleUI.OutputManager.Should().BeOfType<ConsolePlayerOutputManager>("because without the need for Json output the output should be human readable on the console.");
                consoleUI.OutputManager.ConsoleInteractor.Should().Be(consoleInteractor, "because the console interactor object was set on the console user interface and the output manager should have gotten it from there.");
            }
        }

        public class StoryErrorHandlerTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);

                // Act
                consoleUI.StoryErrorHandler(null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_ErrorMessage()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);

                // Act
                const string message = "Test";
                StoryErrorEventArgs e = new StoryErrorEventArgs() { ErrorType = StoryErrorType.Error, Message = message };
                consoleUI.StoryErrorHandler(null, e);

                // Assert
                consoleUI.Errors.Should().HaveCount(1, "because only 1 message was added.");
                consoleUI.Errors.Should().ContainMatch(message, "because only 1 message was added.");
            }

            [Fact]
            public void With_WarningMessage()
            {
                // Arrange
                var consoleUI = new ConsoleUserInterface(null);

                // Act
                const string message = "Test";
                StoryErrorEventArgs e = new StoryErrorEventArgs() { ErrorType = StoryErrorType.Warning, Message = message };
                consoleUI.StoryErrorHandler(null, e);

                // Assert
                consoleUI.Warnings.Should().HaveCount(1, "because only 1 message was added.");
                consoleUI.Warnings.Should().ContainMatch(message, "because only 1 message was added.");
            }
        }
    }
}
