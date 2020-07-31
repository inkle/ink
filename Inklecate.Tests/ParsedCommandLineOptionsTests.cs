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
    public class ParsedCommandLineOptionsTests
    {
        public class IsInputPathNotGivenTests
        {
            [Fact]
            public void With_NoInputPathGiven()
            {
                // Arrange
                var parsedCommandLineOptions = new ParsedCommandLineOptions();

                // Act
                var isInputPathNotGiven = parsedCommandLineOptions.IsInputPathGiven;

                // Assert
                isInputPathNotGiven.Should().BeFalse("because there was no input file given");
            }

            [Fact]
            public void With_InputPathGiven()
            {
                // Arrange
                var parsedCommandLineOptions = new ParsedCommandLineOptions() { InputFilePath = "test.ink" };

                // Act
                var isInputPathNotGiven = parsedCommandLineOptions.IsInputPathGiven;

                // Assert
                isInputPathNotGiven.Should().BeTrue("because there was an input file given");
            }
        }
    }
}
