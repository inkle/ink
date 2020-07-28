
namespace Ink
{
    public interface IInkCompiler
    {
        Parsed.Fiction ParsedFiction { get; }

        Runtime.Story Compile();
        Parsed.Fiction Parse();
        InputInterpretationResult InterpretCommandLineInput(string userInput);
        void RetrieveDebugSourceForLatestContent();
    }
}