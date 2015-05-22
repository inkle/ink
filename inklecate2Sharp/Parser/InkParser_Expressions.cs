using System;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
		protected Parsed.Object Expression()
		{
			string str = ParseString("LOGIC_HERE");
			return new Parsed.Text (str);
		}
	}
}

