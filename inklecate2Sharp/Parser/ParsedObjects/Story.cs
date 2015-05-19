using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public class Story : ContainerBase
	{
		public Story (List<object> toplevelObjects) : base(null, toplevelObjects)
		{
		}

		public object ExportRuntime()
		{
			return null;
		}
	}
}

