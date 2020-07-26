using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Ink.Inklecate;

namespace Ink.Inklecate.Tests
{
    public class ProcessedOptionsTests
    {
        public class IsInputFileJsonTests
        {
            [Fact]
            public void With_JsonFile()
            {
                // Arrange
                var processedOptions = new CommandLineToolOptions() { InputFileName = "test.json" };

                // Act
                var isInputFileJson = processedOptions.IsInputFileJson;

                // Assert
                isInputFileJson.Should().BeTrue("because the given file has a json extension");
            }

            [Fact]
            public void With_InkFile()
            {
                // Arrange
                var processedOptions = new CommandLineToolOptions() { InputFileName = "test.ink" };

                // Act
                var isInputFileJson = processedOptions.IsInputFileJson;

                // Assert
                isInputFileJson.Should().BeFalse("because the given file has no json extension");
            }
        }
    }
}
