using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Inklecate.Interaction
{
    public interface IConsoleInteractable
    {
        void ResetColorOnCancelKeyPress();

        void SetEncodingToUtF8();

        void SetConsoleForegroundColor(ConsoleColor colour);

        void ResetConsoleColor();

        void WriteErrorMessage(string errorMessage);

        void WriteErrorMessage(string errorMessage, object value);

        void WriteWarning(string message);

        void WriteWarning(string message, object value);

        void WriteInformation(string message);

        void WriteJsonMessage(string name, object value);

        void EnvironmentExitWithCodeError1();
    }
}
