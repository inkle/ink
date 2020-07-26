using System;
using System.Collections.Generic;

namespace Ink
{

    public delegate void CompilerErrorEventHandler(object sender, CompilerErrorEventArgs e);
    public class CompilerErrorEventArgs : EventArgs
    {
        public CompilerErrorType ErrorType { get; set; }
        public string Message { get; set; }
    }
}


