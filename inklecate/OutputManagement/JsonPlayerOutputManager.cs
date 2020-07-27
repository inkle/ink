using System;
using System.Collections.Generic;
using System.Text;
using Ink.Inklecate.Interaction;
using Ink.Runtime;

namespace Ink.Inklecate.OutputManagement
{
    public class JsonPlayerOutputManager : IPlayerOutputManagable
    {
        public IConsoleInteractable ConsoleInteractor { get; set; }

        public JsonPlayerOutputManager(IConsoleInteractable consoleInteractor)
        {
            ConsoleInteractor = consoleInteractor;
        }

        public void ShowChoices(List<Ink.Runtime.Choice> choices, ConsoleUserInterfaceOptions options)
        {
            var writer = new Runtime.SimpleJson.Writer();
            writer.WriteObjectStart();
            writer.WritePropertyStart("choices");
            writer.WriteArrayStart();
            foreach (var choice in choices)
            {
                writer.Write(choice.text);
            }
            writer.WriteArrayEnd();
            writer.WritePropertyEnd();
            writer.WriteObjectEnd();
            ConsoleInteractor.WriteJsonInformation(writer.ToString());
        }

        public void RequestInput(ConsoleUserInterfaceOptions options)
        {
            // Johnny Five, he's alive!
            ConsoleInteractor.WriteJsonInformation("{\"needInput\": true}");
        }
        
        public string GetUserInput()
        {
            return Console.ReadLine();
        }
        
        public void ShowStreamError(ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.WriteJsonError("{\"close\": true}");
        }

        public void ShowOutputResult(Compiler.CommandLineInputResult result, ConsoleUserInterfaceOptions options)
        {
            if (result.output != null)
            {
                var writer = new Runtime.SimpleJson.Writer();
                writer.WriteObjectStart();
                writer.WriteProperty("cmdOutput", result.output);
                writer.WriteObjectEnd();
                ConsoleInteractor.WriteJsonError(writer.ToString());
            }
        }

        public void ShowChoiceOutOffRange(ConsoleUserInterfaceOptions options)
        {
            // fail silently in json mode
        }




        public void ShowCurrentText(IStory story, ConsoleUserInterfaceOptions options)
        {
            var writer = new Runtime.SimpleJson.Writer();
            writer.WriteObjectStart();
            writer.WriteProperty("text", story.currentText);
            writer.WriteObjectEnd();
            ConsoleInteractor.WriteJsonInformation(writer.ToString());
        }

        public void ShowTags(List<string> tags, ConsoleUserInterfaceOptions options)
        {
            var writer = new Runtime.SimpleJson.Writer();
            writer.WriteObjectStart();
            writer.WritePropertyStart("tags");
            writer.WriteArrayStart();
            foreach (var tag in tags)
            {
                writer.Write(tag);
            }
            writer.WriteArrayEnd();
            writer.WritePropertyEnd();
            writer.WriteObjectEnd();
            ConsoleInteractor.WriteJsonInformation(writer.ToString());
        }

        public void ShowWarningsAndErrors(List<string> warnings, List<string> errors, ConsoleUserInterfaceOptions options)
        {
            Runtime.SimpleJson.Writer issueWriter = null;
            issueWriter = new Runtime.SimpleJson.Writer();
            issueWriter.WriteObjectStart();
            issueWriter.WritePropertyStart("issues");
            issueWriter.WriteArrayStart();

            if (errors.Count > 0)
            {
                foreach (var errorMsg in errors)
                {
                    issueWriter.Write(errorMsg);
                }
            }
            if (warnings.Count > 0)
            {
                foreach (var warningMsg in warnings)
                {
                    issueWriter.Write(warningMsg);
                }
            }

            issueWriter.WriteArrayEnd();
            issueWriter.WritePropertyEnd();
            issueWriter.WriteObjectEnd();
            ConsoleInteractor.WriteJsonError(issueWriter.ToString());
        }

        public void ShowEndOfStory(ConsoleUserInterfaceOptions options)
        {
            ConsoleInteractor.WriteJsonInformation("{\"end\": true}");
        }
    }
}
