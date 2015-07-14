using System.Collections.Generic;

namespace Inklewriter
{
	public class CharacterSet : HashSet<char>
	{
		public CharacterSet ()
		{
		}

		public CharacterSet(string str)
		{
            AddCharacters (str);
		}

        public CharacterSet(CharacterSet charSetToCopy)
        {
            AddCharacters (charSetToCopy);
        }

		public void AddRange(char start, char end)
		{
			for(char c=start; c<=end; ++c) {
				Add (c);
			}
		}

        // IEnumerable<char> automatically makes it compatible with:
        //  - string
        //  - another CharacterSet
        public void AddCharacters(IEnumerable<char> chars)
		{
            foreach (char c in chars) {
				Add (c);
			}
		}

	}
}

