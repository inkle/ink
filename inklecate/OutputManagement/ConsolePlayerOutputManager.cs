using System;
using System.Collections.Generic;
using System.Text;
using Ink.Inklecate.Interaction;
using Ink.Runtime;

namespace Ink.Inklecate.OutputManagement
{
    public class ConsolePlayerOutputManager : IPlayerOutputManagable
    {
        public IConsoleInteractable ConsoleInteractor { get; set; }

        public ConsolePlayerOutputManager(IConsoleInteractable consoleInteractor)
        {
            ConsoleInteractor = consoleInteractor;
        }

        public void ShowChoices(List<Ink.Runtime.Choice> choices, ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.SetConsoleForegroundColor(ConsoleColor.Blue);

            // Add extra newline to ensure that the choice is
            // on a separate line.
            ConsoleInteractor.WriteEmptyLine();

            int i = 1;
            foreach (var choice in choices)
            {
                ConsoleInteractor.WriteInformation("{0}: {1}", i, choice.text);
                i++;
            }
        }

        public void RequestInput(ConsoleUserInterfaceOptions options)
        {
            // Prompt
            ConsoleInteractor.WriteInformation("?> ");
        }
        public string GetUserInput()
        {
            return Console.ReadLine();
        }
        
        public void ShowStreamError(ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.WriteErrorMessage("<User input stream closed.>");

        }

        public void ShowOutputResult(Compiler.CommandLineInputResult result, ConsoleUserInterfaceOptions options)
        {
            if (result.output != null)
            {
                ConsoleInteractor.WriteErrorMessage(result.output);
            }
        }

        public void ShowChoiceOutOffRange(ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.WriteWarning("Choice out of range");
        }




        public void ShowCurrentText(IStory story, ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.WriteInformation(story.currentText);
        }

        public void ShowTags(List<string> tags, ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.WriteInformation("# tags: " + string.Join(", ", tags));
        }

        public void ShowWarningsAndErrors(List<string> warnings, List<string> errors, ConsoleUserInterfaceOptions options)
        {
            if (errors.Count > 0)
            {
                foreach (var errorMsg in errors)
                {
                    ConsoleInteractor.WriteErrorMessage(errorMsg, ConsoleColor.Red);
                }
            }

            if (warnings.Count > 0)
            {
                foreach (var warningMsg in warnings)
                {
                    ConsoleInteractor.WriteWarning(warningMsg, ConsoleColor.Blue);
                }
            }
        }

        public void ShowEndOfStory(ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.WriteInformation("--- End of story ---");
        }
    }
}
