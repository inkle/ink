using Ink.Parsed;
using Ink.Runtime;

namespace Ink.Inklecate
{
    public interface IConsoleUserInterface
    {
        void Begin(IStory story, IFiction parsedFiction, ConsoleUserInterfaceOptions options);
    }
}