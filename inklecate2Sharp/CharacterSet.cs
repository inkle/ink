using System;
using System.Collections.Generic;

namespace inklecate2Sharp
{
	public class CharacterSet : HashSet<char>
	{
		public CharacterSet ()
		{
		}

		public CharacterSet(string str)
		{
			AddStringCharacters (str);
		}

		public void AddRange(char start, char end)
		{
			for(char c=start; c<=end; ++c) {
				Add (c);
			}
		}

		public void AddStringCharacters(string str)
		{
			foreach (char c in str) {
				Add (c);
			}
		}

	}
}

