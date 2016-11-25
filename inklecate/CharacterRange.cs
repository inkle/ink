using System.Collections.Generic;

namespace Ink
{
	/// <summary>
	/// A class representing a character range. Allows for lazy-loading a corresponding <see cref="CharacterSet">character set</see>.
	/// </summary>
	internal sealed class CharacterRange
	{
		public static CharacterRange Define(char start, char end)
		{
			return new CharacterRange (start, end);
		}

		private readonly char start;
		private readonly char end;
		private readonly CharacterSet correspondingCharSet = new CharacterSet();

		private CharacterRange (char start, char end)
		{
			this.start = start;
			this.end = end;
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
				correspondingCharSet.AddRange (start, end);
			}
			return correspondingCharSet;
		}

		public char Start { get { return start; } }
		public char End { get { return end; } }
	}	
}
