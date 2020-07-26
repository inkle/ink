using System;
using System.Collections.Generic;

namespace Ink.Inklecate
{
    /// <summary>The ParsedCommandLineOptions class encapsulates the options
    /// parsed from the text given at the command line.</summary>
    public class ParsedCommandLineOptions
    {
        /// <summary>Gets or sets the input file path.
        /// Reading from the command line we assume it's a path and later determin if it's only a filename.</summary>
        /// <value>The input file path.</value>
        public string InputFilePath { get; set; }

        /// <summary>Gets or sets the output file path.
        /// Reading from the command line we assume it's a path and later determin if it's only a filename.</summary>
        /// <value>The output file path.</value>
        public string OutputFilePath { get; set; }

        public bool IsPlayMode { get; set; }
        public bool IsVerboseMode { get; set; }
        public bool IsCountAllVisitsNeeded { get; set; }
        public bool IsOnlyShowJsonStatsActive { get; set; }
        public bool IsJsonOutputNeeded { get; set; }
        public bool IsKeepOpenAfterStoryFinishNeeded { get; set; }

        public List<string> PluginNames { get; set; } = new List<string>();

        /// <summary>Gets a value indicating whether input path is given.</summary>
        /// <value>
        ///   <c>true</c> if the input path is given; otherwise, <c>false</c>.</value>
        public bool IsInputPathGiven
        {
            get
            {
                return !string.IsNullOrEmpty(InputFilePath);
            }
        }       
    }
}
