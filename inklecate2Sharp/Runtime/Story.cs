using System;

namespace inklecate2Sharp.Runtime
{
	public class Story : Runtime.Object
	{
		public Story (Container rootContainer)
		{
			_rootContainer = rootContainer;
		}

		public Runtime.Object ContentAtPath(Path path)
		{
			return _rootContainer.ContentAtPath (path);
		}

		private Container _rootContainer;
	}
}

