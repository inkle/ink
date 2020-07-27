using System;
using System.Collections.Generic;
using System.Text;

namespace Ink
{
    public class UserInteractionResult
    {
        public bool IsInputStreamClosed { get; set; }
        public bool IsExitRequested { get; set; }
        public bool IsValidChoice { get; set; }
        public int ChosenIdex { get; set; } = -1;
        public string DivertedPath { get; set; }
        public string Output { get; set; }
    }
}
