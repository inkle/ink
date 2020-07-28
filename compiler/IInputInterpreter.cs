
namespace Ink
{
    public interface IInputInterpreter
    {
        InputInterpretationResult ReadCommandLineInput(string userInput, Parsed.Story parsedStory, Runtime.Story runtimeStory);
    }
}