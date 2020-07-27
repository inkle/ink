using Ink.Inklecate.Interaction;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Inklecate.OutputManagement
{
    public interface IPlayerOutputManagable
    {
        IConsoleInteractable ConsoleInteractor { get; set; }

        void ShowChoices(List<Ink.Runtime.Choice> choices, ConsoleUserInterfaceOptions options);

        void RequestInput(ConsoleUserInterfaceOptions options);
        string GetUserInput();
        
        void ShowStreamError(ConsoleUserInterfaceOptions options);

        void ShowOutputResult(Compiler.CommandLineInputResult result, ConsoleUserInterfaceOptions options);

        void ShowChoiceOutOffRange(ConsoleUserInterfaceOptions options);





        void ShowCurrentText(IStory story, ConsoleUserInterfaceOptions options);

        void ShowTags(List<string> tags, ConsoleUserInterfaceOptions options);

        void ShowWarningsAndErrors(List<string> warnings, List<string> errors, ConsoleUserInterfaceOptions options);

        void ShowEndOfStory(ConsoleUserInterfaceOptions options);
    }
}
