
namespace Ink
{
    public interface IInputInterpreter
    {
        InputInterpretationResult InterpretCommandLineInput(string userInput, Parsed.Fiction parsedFiction, Runtime.Story runtimeStory);
    }
}