
namespace Ink
{
    public interface IInkCompiler
    {
        Parsed.Fiction ParsedFiction { get; }

        Runtime.Story Compile();
        Parsed.Fiction Parse();
        InputInterpretationResult ReadCommandLineInput(string userInput);
        void RetrieveDebugSourceForLatestContent();
    }
}