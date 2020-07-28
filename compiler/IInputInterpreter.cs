
namespace Ink
{
    public interface IInputInterpreter
    {
        InputInterpretationResult ReadCommandLineInput(string userInput, Parsed.Fiction parsedFiction, Runtime.Story runtimeStory);
    }
}