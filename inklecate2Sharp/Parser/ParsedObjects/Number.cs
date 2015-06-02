using System;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	// TODO: Handle other number types
	public class Number : Parsed.Expression
	{
		public object value;
		
		public Number(object value)
		{
            if (value is int || value is float) {
                this.value = value;
            } else {
                throw new System.Exception ("Unexpected object type in Number");
            }
		}

        public override void GenerateIntoContainer (Runtime.Container container)
		{
            if (value is int) {
                container.AddContent (new Runtime.LiteralInt ((int)value));
            } else if (value is float) {
                container.AddContent (new Runtime.LiteralFloat ((float)value));
            }
		}
         
	}
}

