using Ink.Parsed;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Ink
{
    internal partial class InkParser
    {

		internal const string EnableCharacterRangeStatement = "ENABLE CHRANGE";

		protected string EnableCharacterRange()
		{
			Whitespace ();

			if (ParseString (EnableCharacterRangeStatement) == null)
				return null;

			Whitespace ();

			var charRange = (string) Expect(() => ParseUntilCharactersFromString ("\n\r"), "name for character range to enable.");
			charRange = charRange.TrimEnd (' ', '\t');

			if (!_characterRangesByName.ContainsKey (charRange)) 
			{
				// If the char range is not defined we should print a warning. In case there are invalid identifiers, 
				// we will allow the default ink parsing to fail when detected, so that the corresponding line nuber 
				// is presented in the error the user receives.
				Warning ("Specified character range \"{0}\" does not exist. Some identifiers may not be parseable.", charRange);
			}

			// We do not care now if the range is added multiple times, hash set will take care for us of duplicates
			// Thus may have to change later if we need to disable character ranges, but this currently does not make much sense.
			_enabledCharacterRanges.Add (charRange);

			return charRange;
		}

		HashSet<string> _enabledCharacterRanges = new HashSet<string>();

		IDictionary<string, CharacterRange> _characterRangesByName = new Dictionary<string, CharacterRange>(StringComparer.OrdinalIgnoreCase)
		{
			{ "Basic Latin", 		CharacterRange.Define('\u0020','\u007F') },
			{ "Latin Supplement", 	CharacterRange.Define('\u00A0','\u00FF') },
			{ "Latin Extended A", 	CharacterRange.Define('\u0100','\u017F') },
			{ "Latin Extended B", 	CharacterRange.Define('\u0180','\u024F') },
			{ "Cyrillic", 			CharacterRange.Define('\u0400','\u04FF') },
			// and so on
		};
    }
}

