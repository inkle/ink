using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Inklecate.OutputManagement
{
    public interface IPlayerOutputManagable
    {
        void ShowChoices(List<Ink.Runtime.Choice> choices, ConsoleUserInterfaceOptions options);

        void RequestInput(ConsoleUserInterfaceOptions options);
        void ShowStreamError(ConsoleUserInterfaceOptions options);

        void ShowOutputResult(ConsoleUserInterfaceOptions options, Compiler.CommandLineInputResult result);

        void ShowChoiceOutOffRange(ConsoleUserInterfaceOptions options);





        void ShowCurrentText(IStory story, ConsoleUserInterfaceOptions options);

        void ShowTags(ConsoleUserInterfaceOptions options, List<string> tags);

        void ShowWarningsAndErrors(List<string> warnings, List<string> errors, ConsoleUserInterfaceOptions options);

        void ShowEndOfStory(ConsoleUserInterfaceOptions options);
    }
}
