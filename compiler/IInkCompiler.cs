
namespace Ink
{
    public interface IInkCompiler
    {
        Parsed.Story parsedStory { get; }

        Runtime.Story Compile();
        Parsed.Story Parse();
        InputInterpretationResult ReadCommandLineInput(string userInput);
        void RetrieveDebugSourceForLatestContent();
    }
}