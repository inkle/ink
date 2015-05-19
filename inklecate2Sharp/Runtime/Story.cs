using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Runtime
{
	public class Story : Runtime.Object
	{
		public Path currentPath { get; protected set; }
		public List<object> outputStream;

		public Story (Container rootContainer)
		{
			_rootContainer = rootContainer;

			outputStream = new List<object> ();
		}

		public Runtime.Object ContentAtPath(Path path)
		{
			return _rootContainer.ContentAtPath (path);
		}

		public void Begin()
		{
			currentPath = Path.ToFirstElement ();
			Continue ();
		}

		public void Continue()
		{
			Runtime.Object currentContentObj = null;
			do {

				currentContentObj = ContentAtPath(currentPath);
				if( currentContentObj != null ) {
					
					// Convert path to get first leaf content
					Container currentContainer = currentContentObj as Container;
					if( currentContainer != null ) {
						currentPath = currentPath.PathByAppendingPath(currentContainer.pathToFirstLeafContent);
						currentContentObj = ContentAtPath(currentPath);
					}

					// Content
					outputStream.Add(currentContentObj);

					Step();
				}

			} while(currentContentObj != null && currentPath != null);
		}

		public void Step()
		{
			// TODO: Redirection
			// Can we increment successfully?
			currentPath = _rootContainer.IncrementPath (currentPath);
			if (currentPath == null) {

				// TODO: Try to recover by popping call stack
			}
		}

		private Container _rootContainer;
	}
}

