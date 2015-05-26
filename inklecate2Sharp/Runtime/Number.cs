using System;

namespace Inklewriter.Runtime
{
    public class Number : Runtime.Object
    {
        public int value { get; set; }

        public Number (int num)
        {
            value = num;
        }

        public override string ToString ()
        {
            return value.ToString();
        }
    }
}

