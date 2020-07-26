using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Inklecate.Interaction
{
    public interface IConsoleInteractable
    {
        void SetEncodingToUtF8();

        void EnvironmentExitWithCodeError1();


        void ResetColorOnCancelKeyPress();

        void SetConsoleForegroundColor(ConsoleColor colour);

        void ResetConsoleColor();

        void WriteEmptyLine();

        void WriteErrorMessage(string errorMessage);

        void WriteErrorMessage(string errorMessage, object value);

        void WriteWarning(string message);

        void WriteWarning(string message, object value);

        void WriteInformation(string message);
        void WriteInformation(string name, object value, object secondValue);

        void WriteJson(string json);

        void WriteJsonInformation(string json);

        void WriteJsonWarning(string json);

        void WriteJsonError(string json);
        void WriteJsonNameValuePair(string name, object value);
    }
}
