using System;
using System.Collections.Generic;

namespace Ink.Inklecate
{
    /// <summary>The CommandLineToolOptions class encapsulates the data that is used by the tool that is started on the command line.</summary>
    public class CommandLineToolOptions
    {
        public string InputFileName { get; set; }
        public string InputFilePath { get; set; }
        public string RootedInputFilePath { get; set; }
        public string InputFileDirectory { get; set; }

        public string GeneratedOutputFilePath { get; set; }
        public string RootedOutputFilePath { get; set; }

        public bool IsPlayMode { get; set; }
        public bool IsVerboseMode { get; set; }
        public bool IsCountAllVisitsNeeded { get; set; }
        public bool IsOnlyShowJsonStatsActive { get; set; }
        public bool IsJsonOutputNeeded { get; set; }
        public bool IsKeepRunningAfterStoryFinishedNeeded { get; set; }

        public List<string> PluginNames { get; set; } = new List<string>();

        /// <summary>Gets a value indicating whether the input file is json.</summary>
        /// <value>
        ///   <c>true</c> if the input file is json; otherwise, <c>false</c>.</value>
        public bool IsInputFileJson
        {
            get
            {
                if (InputFileName == null)
                    return false;

                return InputFileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
