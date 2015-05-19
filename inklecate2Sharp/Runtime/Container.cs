using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Runtime
{
	public class Container : Runtime.Object
	{
		public string name { get; set; }
		public List<object> content { get; protected set; }

		public Container ()
		{
			content = new List<object> ();
		}

		public void AddContent(Runtime.Object contentObj)
		{
			content.Add (contentObj);
		}

		public Runtime.Object ContentAtPath(Path path)
		{
			// TODO!
			return null;
		}
	}
}

