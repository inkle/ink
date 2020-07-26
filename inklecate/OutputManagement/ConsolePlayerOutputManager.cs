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
            Console.ForegroundColor = ConsoleColor.Blue;

            // Add extra newline to ensure that the choice is
            // on a separate line.
            Console.WriteLine();

            int i = 1;
            foreach (var choice in choices)
            {
                Console.WriteLine("{0}: {1}", i, choice.text);
                i++;
            }
        }

        public void RequestInput(ConsoleUserInterfaceOptions options)
        {
            // Prompt
            Console.Write("?> ");
        }
        public void ShowStreamError(ConsoleUserInterfaceOptions options)
        {
            Console.WriteLine("<User input stream closed.>");

        }

        public void ShowOutputResult(ConsoleUserInterfaceOptions options, Compiler.CommandLineInputResult result)
        {
            if (result.output != null)
            {
                Console.WriteLine(result.output);
            }
        }

        public void ShowChoiceOutOffRange(ConsoleUserInterfaceOptions options)
        {
            Console.WriteLine("Choice out of range");
        }




        public void ShowCurrentText(IStory story, ConsoleUserInterfaceOptions options)
        {
            Console.Write(story.currentText);
        }

        public void ShowTags(ConsoleUserInterfaceOptions options, List<string> tags)
        {
            Console.WriteLine("# tags: " + string.Join(", ", tags));
        }

        public void ShowWarningsAndErrors(List<string> warnings, List<string> errors, ConsoleUserInterfaceOptions options)
        {
            if (errors.Count > 0)
            {
                foreach (var errorMsg in errors)
                {
                    Console.WriteLine(errorMsg, ConsoleColor.Red);
                }
            }

            if (warnings.Count > 0)
            {
                foreach (var warningMsg in warnings)
                {
                    Console.WriteLine(warningMsg, ConsoleColor.Blue);
                }
            }
        }

        public void ShowEndOfStory(ConsoleUserInterfaceOptions options)
        {
            Console.WriteLine("--- End of story ---");
        }
    }
}
