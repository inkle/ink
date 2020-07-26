using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Inklecate.Interaction
{
    public class ConsoleInteractor : IConsoleInteractable
    {    //
         // Reading up on event can be done here (The links may change in time. Do a search then) 
         // 
         // Distinguishing Delegates and Events
         // https://docs.microsoft.com/en-us/dotnet/csharp/distinguish-delegates-events
         // Handling and raising events
         // https://docs.microsoft.com/en-us/dotnet/standard/events/
         // The Updated .NET Core Event Pattern
         // https://docs.microsoft.com/en-us/dotnet/csharp/modern-events
         // EventArgs Class
         // https://docs.microsoft.com/en-us/dotnet/api/system.eventargs?view=netcore-3.1
         // EventArgs.Empty Field
         // https://docs.microsoft.com/en-us/dotnet/api/system.eventargs.empty?view=netcore-3.1
         // Events and routed events overview
         // https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh758286(v=win.10)
         // In the above mentioned article there is a chapter "Removing event handlers in code" containing the following:
         //It's not usually necessary to remove event handlers in code, even if you added them in code. The object lifetime behavior for most Windows Runtime objects such as pages and controls will destroy the objects when they are disconnected from the main Window and its visual tree, and any delegate references are destroyed too. .NET does this through garbage collection and Windows Runtime with C++/CX uses weak references by default.
         //There are some rare cases where you do want to remove event handlers explicitly.These include:
         // - Handlers you added for static events, which can't get garbage-collected in a conventional way. Examples of static events in the Windows Runtime API are the events of the CompositionTarget and Clipboard classes.
         // - Test code where you want the timing of handler removal to be immediate, or code where you what to swap old/new event handlers for an event at run time.
         // - The implementation of a custom remove accessor.
         // - Custom static events.
         // - Handlers for page navigations.


        public void ResetColorOnCancelKeyPress()
        {
            Console.CancelKeyPress += ConsoleCancelKeyPressHandler;
        }

        /// <summary>Handles the CancelKeyPress event of the Console control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ConsoleCancelEventArgs" /> instance containing the event data.</param>
        private void ConsoleCancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            ResetConsoleColor();
        }

        public void ResetConsoleColor()
        {
            Console.ResetColor();
        }

        public void SetEncodingToUtF8()
        {
            // Set console's output encoding to UTF-8
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public void SetConsoleForegroundColor(ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
        }
        
        public void WriteWarning(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteWarning(string message, object value)
        {
            Console.WriteLine(message, value);
        }

        public void WriteInformation(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteErrorMessage(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }

        public void WriteErrorMessage(string errorMessage, object value)
        {
            Console.WriteLine(errorMessage, value);
        }

        public void WriteJsonMessage(string name, object value)
        {
            Console.WriteLine("{\"{0}\": {1}}", name, value);
        }

        public void EnvironmentExitWithCodeError1()
        {
            const int ExitCodeError = 1;
            Environment.Exit(ExitCodeError);
        }
    }
}
