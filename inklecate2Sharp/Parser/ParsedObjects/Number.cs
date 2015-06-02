using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	// TODO: Handle other number types
	public class Number : Parsed.Expression
	{
		public int value;
		
		public Number(int value)
		{
			this.value = value;
		}

        public Number(float value)
        {
            // TODO: SEPARATE INTO NEW CLASS!
            this.value = (int)value;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
		{
            container.AddContent (new Runtime.LiteralInt(value));
		}
         
	}
}

