using System;
using System.Collections.Generic;
using System.IO;

namespace Ink
{
    public delegate void StoryErrorEventHandler(object sender, StoryErrorEventArgs e);
    public class StoryErrorEventArgs : EventArgs
    {
        public StoryErrorType ErrorType { get; set; }
        public string Message { get; set; }
    }
}

