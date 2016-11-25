using System.Collections.Generic;
using System.Linq;

namespace Ink
{
	/// <summary>
	/// A class representing a character range. Allows for lazy-loading a corresponding <see cref="CharacterSet">character set</see>.
	/// </summary>
	internal sealed class CharacterRange
	{
		public static CharacterRange Define(char start, char end, IEnumerable<char> excludes = null)
		{
			return new CharacterRange (start, end, excludes);
		}

		private readonly char start;
		private readonly char end;
		private readonly IEnumerable<char> exludes;
		private readonly CharacterSet correspondingCharSet = new CharacterSet();

		private CharacterRange (char start, char end, IEnumerable<char> excludes)
		{
			this.start = start;
			this.end = end;
			this.exludes = exludes == null ? string.Empty : exludes;
		}

		/// <summary>
		/// Returns a <see cref="CharacterSet">character set</see> instance corresponding to the character range
		/// represented by the current instance.
		/// </summary>
		/// <remarks>
		/// The internal character set is created once and cached in memory.
		/// </remarks>
		/// <returns>The char set.</returns>
		public CharacterSet ToCharacterSet ()
		{
			if (correspondingCharSet.Count == 0) 
			{
				for (char c = start; c <= end; c++)
				{
					if (exludes.Contains(c))
					{
						continue;
					}
					correspondingCharSet.Add(c);
				}
			}
			return correspondingCharSet;
		}

		public char Start { get { return start; } }
		public char End { get { return end; } }
	}	
}
