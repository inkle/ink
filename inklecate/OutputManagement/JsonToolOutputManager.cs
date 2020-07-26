using System;
using System.Collections.Generic;
using System.Text;
using Ink.Inklecate.Interaction;

namespace Ink.Inklecate.OutputManagement
{
    public class JsonToolOutputManager : IToolOutputManagable
    {
        public IConsoleInteractable ConsoleInteractor { get; set; }

        public JsonToolOutputManager(IConsoleInteractable consoleInteractor)
        {
            ConsoleInteractor = consoleInteractor;
        }

        public void ShowExportComplete(CommandLineToolOptions options)
        {
            ConsoleInteractor.WriteJsonNameValuePair("export-complete", true);
        }

        public void ShowStats(CommandLineToolOptions options, Stats stats)
        {
            var writer = new Runtime.SimpleJson.Writer();

            writer.WriteObjectStart();
            writer.WritePropertyStart("stats");

            writer.WriteObjectStart();
            writer.WriteProperty("words", stats.words);
            writer.WriteProperty("knots", stats.knots);
            writer.WriteProperty("stitches", stats.stitches);
            writer.WriteProperty("functions", stats.functions);
            writer.WriteProperty("choices", stats.choices);
            writer.WriteProperty("gathers", stats.gathers);
            writer.WriteProperty("diverts", stats.diverts);
            writer.WriteObjectEnd();

            writer.WritePropertyEnd();
            writer.WriteObjectEnd();

            ConsoleInteractor.WriteJsonInformation(writer.ToString());
        }

        public void ShowCompileSuccess(CommandLineToolOptions options, bool compileSuccess)
        {
            ConsoleInteractor.WriteJsonNameValuePair("compile-success", compileSuccess);
        }


        public void PrintAllMessages(List<string> authorMessages, List<string> warnings, List<string> errors)
        {
            // { "issues": ["ERROR: blah", "WARNING: blah"] }

            var writer = new Runtime.SimpleJson.Writer();

            writer.WriteObjectStart();
            writer.WritePropertyStart("issues");
            writer.WriteArrayStart();
            foreach (string msg in authorMessages)
            {
                writer.Write(msg);
            }
            foreach (string msg in warnings)
            {
                writer.Write(msg);
            }
            foreach (string msg in errors)
            {
                writer.Write(msg);
            }
            writer.WriteArrayEnd();
            writer.WritePropertyEnd();
            writer.WriteObjectEnd();

            ConsoleInteractor.WriteJsonInformation(writer.ToString());
        }
    }
}
