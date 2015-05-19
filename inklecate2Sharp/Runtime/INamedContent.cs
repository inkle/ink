using System;

namespace inklecate2Sharp
{
	public interface INamedContent
	{
		string name { get; }
		bool hasValidName { get; }
	}
}

