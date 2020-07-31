using System;
using System.Collections.Generic;
using System.Text;
using Ink.Inklecate.Interaction;

namespace Ink.Inklecate.OutputManagement
{
    public class ConsoleToolOutputManager : IToolOutputManagable
    {
        public IConsoleInteractable ConsoleInteractor { get; set; }

        public ConsoleToolOutputManager(IConsoleInteractable consoleInteractor)
        {
            ConsoleInteractor = consoleInteractor;
        }

        public void ShowExportComplete(CommandLineToolOptions options)
        {
            // We don't show export complete in the console
        }

        public void ShowStats(CommandLineToolOptions options, Stats stats)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("Words: ").Append(stats.words);
            builder.Append("Knots: ").Append(stats.knots);
            builder.Append("Stitches: ").Append(stats.stitches);
            builder.Append("Functions: ").Append(stats.functions);
            builder.Append("Choices: ").Append(stats.choices);
            builder.Append("Gathers: ").Append(stats.gathers);
            builder.Append("Diverts: ").Append(stats.diverts);

            ConsoleInteractor.WriteInformation(builder.ToString());
        }

        public void ShowCompileSuccess(CommandLineToolOptions options, bool compileSuccess)
        {
            // We don't show compile success in the console
        }

        public void PrintAllMessages(List<string> authorMessages, List<string> warnings, List<string> errors)
        {
            // Human consumption
            PrintIssues(authorMessages, ConsoleColor.Green);
            PrintIssues(warnings, ConsoleColor.Blue);
            PrintIssues(errors, ConsoleColor.Red);
        }

        void PrintIssues(List<string> messageList, ConsoleColor colour)
        {
            ConsoleInteractor.SetConsoleForegroundColor(colour);
            foreach (string msg in messageList)
            {
                ConsoleInteractor.WriteWarning(msg);
            }
            ConsoleInteractor.ResetConsoleColor();
        }
    }
}
