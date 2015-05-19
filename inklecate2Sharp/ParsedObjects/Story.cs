using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public class Story : Parsed.Object
	{
		private List<object> content { get; set; }

		public Story (List<object> toplevelObjects)
		{
			content = toplevelObjects;
		}

		public object ExportRuntime()
		{
			return null;
		}
	}
}

