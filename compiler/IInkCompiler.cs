
namespace Ink
{
    public interface IInkCompiler
    {
        Parsed.Story parsedStory { get; }

        Runtime.Story Compile();
        Parsed.Story Parse();
        Compiler.CommandLineInputResult ReadCommandLineInput(string userInput);
        void RetrieveDebugSourceForLatestContent();
    }
}