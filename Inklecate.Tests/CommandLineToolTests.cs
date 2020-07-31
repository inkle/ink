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
    public class CommandLineToolTests
    {
        public class ParseArgumentsTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var tool = new CommandLineTool();

                // Act
                tool.ParseArguments(null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_OnlyInputFile()
            {
                // Arrange
                var options = new ParsedCommandLineOptions();
                const string InputFilePath = "test.ink";

                string[] args = new string[] { InputFilePath };
                var tool = new CommandLineTool();

                // Act
                tool.ParseArguments(args, options);

                // Assert
                options.Should().NotBeNull("because the parsing should succeed");

                options.InputFilePath.Should().BeEquivalentTo(InputFilePath);

                options.OutputFilePath.Should().BeNull("because none was given");
                options.IsCountAllVisitsNeeded.Should().BeFalse("because the count all visits flag was not set");
                options.IsPlayMode.Should().BeFalse("because the playmode flag was not set");
                options.IsVerboseMode.Should().BeFalse("because the verbose flag was not set");
                options.IsKeepOpenAfterStoryFinishNeeded.Should().BeFalse("because the keep running after finished flag was not set");
            }

            [Fact]
            public void With_InputFileAndOutputFile()
            {
                // Arrange
                var options = new ParsedCommandLineOptions();
                const string ArgumentString = "-o output test.ink";
                string[] args = ArgumentString.Split(" ");
                var tool = new CommandLineTool();

                // Act
                tool.ParseArguments(args, options);

                // Assert
                options.Should().NotBeNull("because the parsing should succeed");

                options.InputFilePath.Should().BeEquivalentTo("test.ink");
                options.OutputFilePath.Should().Be("output");

                options.IsCountAllVisitsNeeded.Should().BeFalse("because the count all visits flag was not set");
                options.IsPlayMode.Should().BeFalse("because the playmode flag was not set");
                options.IsVerboseMode.Should().BeFalse("because the verbose flag was not set");
                options.IsKeepOpenAfterStoryFinishNeeded.Should().BeFalse("because the keep running after finished flag was not set");
            }

            [Fact]
            public void With_CountAllVisitsAndOutputFile()
            {
                // Arrange
                var options = new ParsedCommandLineOptions();
                const string ArgumentString = "-c test.ink";
                string[] args = ArgumentString.Split(" ");
                var tool = new CommandLineTool();

                // Act
                tool.ParseArguments(args, options);

                // Assert
                options.Should().NotBeNull("because the parsing should succeed");

                options.InputFilePath.Should().BeEquivalentTo("test.ink");

                options.OutputFilePath.Should().BeNull("because none was given");
                options.IsCountAllVisitsNeeded.Should().BeTrue("because the count all visits flag was set");
                options.IsPlayMode.Should().BeFalse("because the playmode flag was not set");
                options.IsVerboseMode.Should().BeFalse("because the verbose flag was not set");
                options.IsKeepOpenAfterStoryFinishNeeded.Should().BeFalse("because the keep running after finished flag was not set");
            }

            [Fact]
            public void With_PlayModeAndOutputFile()
            {
                // Arrange
                var options = new ParsedCommandLineOptions();
                const string ArgumentString = "-p test.ink";
                string[] args = ArgumentString.Split(" ");
                var tool = new CommandLineTool();

                // Act
                tool.ParseArguments(args, options);

                // Assert
                options.Should().NotBeNull("because the parsing should succeed");

                options.InputFilePath.Should().BeEquivalentTo("test.ink");

                options.OutputFilePath.Should().BeNull("because none was given");
                options.IsCountAllVisitsNeeded.Should().BeFalse("because the count all visits flag was not set");
                options.IsPlayMode.Should().BeTrue("because the playmode flag was set");
                options.IsVerboseMode.Should().BeFalse("because the verbose flag was not set");
                options.IsKeepOpenAfterStoryFinishNeeded.Should().BeFalse("because the keep running after finished flag was not set");
            }

            [Fact]
            public void With_VerboseAndOutputFile()
            {
                // Arrange
                var options = new ParsedCommandLineOptions();
                const string ArgumentString = "-v test.ink";
                string[] args = ArgumentString.Split(" ");
                var tool = new CommandLineTool();

                // Act
                tool.ParseArguments(args, options);

                // Assert
                options.Should().NotBeNull("because the parsing should succeed");

                options.InputFilePath.Should().BeEquivalentTo("test.ink");

                options.OutputFilePath.Should().BeNull("because none was given");
                options.IsCountAllVisitsNeeded.Should().BeFalse("because the count all visits flag was not set");
                options.IsPlayMode.Should().BeFalse("because the playmode flag was not set");
                options.IsVerboseMode.Should().BeTrue("because the verbose flag was set");
                options.IsKeepOpenAfterStoryFinishNeeded.Should().BeFalse("because the keep running after finished flag was not set");
            }

            [Fact]
            public void With_KeepRunningAfterStoryFinishedAndOutputFileTest()
            {
                // Arrange
                var options = new ParsedCommandLineOptions();
                const string ArgumentString = "-k test.ink";
                string[] args = ArgumentString.Split(" ");
                var tool = new CommandLineTool();

                // Act
                tool.ParseArguments(args, options);

                // Assert
                options.Should().NotBeNull("because the parsing should succeed");

                options.InputFilePath.Should().BeEquivalentTo("test.ink");

                options.OutputFilePath.Should().BeNull("because none was given");
                options.IsCountAllVisitsNeeded.Should().BeFalse("because the count all visits flag was not set");
                options.IsPlayMode.Should().BeFalse("because the playmode flag was not set");
                options.IsVerboseMode.Should().BeFalse("because the verbose flag was not set");
                options.IsKeepOpenAfterStoryFinishNeeded.Should().BeTrue("because the keep running after finished flag was set");
            }
        }

        public class ProcesOutputFilePathTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var tool = new CommandLineTool();

                // Act
                tool.ProcesOutputFilePath(null, null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_RootedOutputFilePath()
            {
                // Arrange
                const string outputFilePath = @"C:\Test\testfile.ink.json";
                const string startingDirectory = @"C:\Test";
                var parsedOptions = new ParsedCommandLineOptions() { OutputFilePath = outputFilePath };
                var processedOptions = new CommandLineToolOptions();
                var tool = new CommandLineTool();

                // Act
                tool.ProcesOutputFilePath(parsedOptions, processedOptions, startingDirectory);

                // Assert
                parsedOptions.Should().NotBeNull("because the parsed options object was given");
                processedOptions.Should().NotBeNull("because the processed options object was given");
                processedOptions.RootedOutputFilePath.Should().Be(outputFilePath, "because it was given");
            }

            [Fact]
            public void With_OutputFile()
            {
                // Arrange
                const string outputFilePath = @"testfile.ink.json";
                const string startingDirectory = @"C:\Test";
                var parsedOptions = new ParsedCommandLineOptions() { OutputFilePath = outputFilePath };
                var processedOptions = new CommandLineToolOptions();
                var tool = new CommandLineTool();

                // Act
                tool.ProcesOutputFilePath(parsedOptions, processedOptions, startingDirectory);

                // Assert
                parsedOptions.Should().NotBeNull("because the parsed options object was given");
                processedOptions.Should().NotBeNull("because the processed options object was given");
                processedOptions.RootedOutputFilePath.Should().Be(@"C:\Test\testfile.ink.json", "because it was given");
            }

            [Fact]
            public void With_RootedInputFilePath()
            {
                // Arrange
                const string inputFilePath = @"C:\Test\rooted_generating_testfile.ink";
                const string startingDirectory = @"C:\Test";
                var parsedOptions = new ParsedCommandLineOptions() { InputFilePath = inputFilePath };
                var processedOptions = new CommandLineToolOptions();
                var tool = new CommandLineTool();

                // Act
                tool.ProcesOutputFilePath(parsedOptions, processedOptions, startingDirectory);

                // Assert
                parsedOptions.Should().NotBeNull("because the parsed options object was given");
                processedOptions.Should().NotBeNull("because the processed options object was given");
                processedOptions.RootedOutputFilePath.Should().Be(@"C:\Test\rooted_generating_testfile.ink.json", "because it was given");
            }

            [Fact]
            public void With_InputFile()
            {
                // Arrange
                const string inputFilePath = @"generating_testfile.ink";
                const string startingDirectory = @"C:\Test";
                var parsedOptions = new ParsedCommandLineOptions() { InputFilePath = inputFilePath };
                var processedOptions = new CommandLineToolOptions();
                var tool = new CommandLineTool();

                // Act
                tool.ProcesOutputFilePath(parsedOptions, processedOptions, startingDirectory);

                // Assert
                parsedOptions.Should().NotBeNull("because the parsed options object was given");
                processedOptions.Should().NotBeNull("because the processed options object was given");
                processedOptions.RootedOutputFilePath.Should().Be(@"C:\Test\generating_testfile.ink.json", "because it was given");
            }
        }

        public class ProcesInputFilePathTests
        {
            [Fact]
            public void With_NullArguments()
            {
                // Arrange
                var tool = new CommandLineTool();

                // Act
                tool.ProcesInputFilePath(null, null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_RootedInputFilePath()
            {
                // Arrange
                const string inputFilePath = @"C:\Test\testfile.ink";
                const string startingDirectory = @"C:\Test";
                var parsedOptions = new ParsedCommandLineOptions() { InputFilePath = inputFilePath };
                var processedOptions = new CommandLineToolOptions();
                var tool = new CommandLineTool();

                // Act
                tool.ProcesInputFilePath(parsedOptions, processedOptions, startingDirectory);

                // Assert
                parsedOptions.Should().NotBeNull("because the parsed options object was given");
                processedOptions.Should().NotBeNull("because the processed options object was given");

                processedOptions.InputFilePath.Should().Be(inputFilePath, "because it was given");
                processedOptions.InputFileName.Should().Be(@"testfile.ink", "because that is the filename part of the path");
                processedOptions.RootedInputFilePath.Should().Be(@"C:\Test\testfile.ink", "because combines the starting directory with the filename");
                processedOptions.InputFileDirectory.Should().Be(startingDirectory, "because the starting directory should be the default");
            }

            [Fact]
            public void With_InputFile()
            {
                // Arrange
                const string inputFilePath = @"testfile.ink";
                const string startingDirectory = @"C:\SomeFolder";
                var parsedOptions = new ParsedCommandLineOptions() { InputFilePath = inputFilePath };
                var processedOptions = new CommandLineToolOptions();
                var tool = new CommandLineTool();

                // Act
                tool.ProcesInputFilePath(parsedOptions, processedOptions, startingDirectory);

                // Assert
                parsedOptions.Should().NotBeNull("because the parsed options object was given");
                processedOptions.Should().NotBeNull("because the processed options object was given");

                processedOptions.InputFilePath.Should().Be(inputFilePath, "because it was given");
                processedOptions.InputFileName.Should().Be(@"testfile.ink", "because that is the filename part of the path");
                processedOptions.RootedInputFilePath.Should().Be(@"C:\SomeFolder\testfile.ink", "because combines the starting directory with the filename");
                processedOptions.InputFileDirectory.Should().Be(startingDirectory, "because the starting directory should be the default");
            }
        }

        public class ProcesFlagsTests
        {
            [Fact]
            public void WithoutArguments()
            {
                // Arrange
                var tool = new CommandLineTool();

                // Act
                tool.ProcesFlags(null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_AllParsedOptions()
            {
                // Arrange
                const bool isCountAllVisitsNeeded = true;
                const bool isPlayMode = true;
                const bool isVerboseMode = true;
                const bool isKeepRunningAfterStoryFinishedNeeded = true;
                var pluginNames = new List<string>();
                var parsedOptions = new ParsedCommandLineOptions() 
                {
                    IsCountAllVisitsNeeded = isCountAllVisitsNeeded,
                    IsPlayMode = isPlayMode,
                    IsVerboseMode = isVerboseMode,
                    IsKeepOpenAfterStoryFinishNeeded = isKeepRunningAfterStoryFinishedNeeded,
                    PluginNames = pluginNames,
                };
                var processedOptions = new CommandLineToolOptions();
                var tool = new CommandLineTool();

                // Act
                tool.ProcesFlags(parsedOptions, processedOptions);

                // Assert
                parsedOptions.Should().NotBeNull("because the parsed options object was given");
                processedOptions.Should().NotBeNull("because the processed options object was given");

                processedOptions.IsCountAllVisitsNeeded.Should().Be(isCountAllVisitsNeeded, "because it was given");
                processedOptions.IsPlayMode.Should().Be(isPlayMode, "because it was given");
                processedOptions.IsVerboseMode.Should().Be(isVerboseMode, "because it was given");
                processedOptions.IsKeepRunningAfterStoryFinishedNeeded.Should().Be(isKeepRunningAfterStoryFinishedNeeded, "because it was given");
                processedOptions.PluginNames.Should().BeEquivalentTo(pluginNames, "because it was given");
            }
        }

        public class ReadFileTextTests
        {
            [Fact]
            public void WithoutArguments()
            {
                // Arrange
                var tool = new CommandLineTool();

                // Act
                tool.ReadFileText(null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_MockedFileSystemInteraction()
            {
                // Arrange
                string inputFileDirectory = @"c:\Test";
                string inputFileName = "test.ink";
                var tool = new CommandLineTool();
                tool.FileSystemInteractor = Substitute.For<IFileSystemInteractable>();

                // Act
                string fileContents = tool.ReadFileText(inputFileDirectory, inputFileName);

                // Assert
                fileContents.Should().BeNullOrEmpty("because the file system interactor was substituted");
            }

            [Fact]
            public void With_ExceptionThrowingSetCurrentDirectory()
            {
                // Arrange
                string inputFileDirectory = @"c:\Test";
                string inputFileName = "test.ink";
                var fileSystemInteractorMock = Substitute.For<IFileSystemInteractable>();
                fileSystemInteractorMock.When(x => x.SetCurrentDirectory(Arg.Any<string>()))
                    .Do(x => throw new System.IO.DirectoryNotFoundException());
                var tool = new CommandLineTool();
                tool.FileSystemInteractor = fileSystemInteractorMock;

                // Act
                string fileContents = tool.ReadFileText(inputFileDirectory, inputFileName);

                // Assert
                fileContents.Should().BeNullOrEmpty("because the file system interactor was substituted");
            }

            [Fact]
            public void With_ExceptionThrowingReadFile()
            {
                // Arrange
                string inputFileDirectory = @"c:\Test";
                string inputFileName = "test.ink";
                var fileSystemInteractorMock = Substitute.For<IFileSystemInteractable>();
                fileSystemInteractorMock.When(x => x.ReadAllTextFromFile(Arg.Any<string>()))
                    .Do(x => throw new System.IO.DirectoryNotFoundException());
                var tool = new CommandLineTool();
                tool.FileSystemInteractor = fileSystemInteractorMock;

                // Act
                string fileContents = tool.ReadFileText(inputFileDirectory, inputFileName);

                // Assert
                fileContents.Should().BeNullOrEmpty("because the file system interactor was substituted");
            }
        }

        public class ProgramConstructorTests
        {
            // The blasted program constructor is hard to test, because it does too much.
            // Having a constructor that can fail is also a bad idea, because it leads to half initalized objects.
            // It the damn Ink.IFileHandler interface that makes trouble with the static nature of the Main function.
            // It's beter to change it to events with the event sender, eventargs pattern, but that would imply changing the compiler.

            [Fact]
            public void With_NullArguments()
            {
                // Arrange

                // Act
                //var inklecate = new Ink.Inklecate.Program(null, null);

                // Assert
                // Without arguments the function should process nothing.
            }

            [Fact]
            public void With_NullInputFileAndDirectory()
            {
                // Arrange
                var options = new CommandLineToolOptions();

                // Act
                //var inklecate = new Ink.Inklecate.Program(options, null);

                // Assert
                // Without arguments the function should process nothing.
            }
        }
    }
}
