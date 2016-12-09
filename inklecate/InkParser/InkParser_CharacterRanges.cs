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

		internal static readonly CharacterRange LatinBasic = 
			CharacterRange.Define ('\u0030', '\u007A', excludes: new CharacterSet().AddRange('\u003A', '\u003F').AddRange('\u005B', '\u0060'));
		internal static readonly CharacterRange LatinExtendedA = CharacterRange.Define('\u0100', '\u017F'); // no excludes here
		internal static readonly CharacterRange LatinExtendedB = CharacterRange.Define('\u0180', '\u024F'); // no excludes here
		internal static readonly CharacterRange Greek = 
           	CharacterRange.Define('\u0370', '\u03FF', excludes: new CharacterSet().AddCharacters("\u0374\u0375\u037A\u0387").AddRange('\u037E', '\u0385'));
		internal static readonly CharacterRange Cyrillic = 
			CharacterRange.Define('\u0400', '\u04FF', excludes: new CharacterSet().AddRange('\u0482', '\u0489'));
		internal static readonly CharacterRange Armenian = 
			CharacterRange.Define('\u0530', '\u058F', excludes: new CharacterSet().AddCharacters("\u0530").AddRange('\u0557', '\u0560').AddRange('\u0588', '\u058E'));
		internal static readonly CharacterRange Hebrew = 
			CharacterRange.Define('\u0590', '\u05FF', excludes: new CharacterSet());
		internal static readonly CharacterRange Arabic = 
			CharacterRange.Define('\u0600', '\u06FF', excludes: new CharacterSet());

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

		HashSet<string> _enabledCharacterRanges = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		readonly IDictionary<string, CharacterRange> _characterRangesByName = new Dictionary<string, CharacterRange>(StringComparer.OrdinalIgnoreCase)
		{
			// Basic Latin and aliases
			{ "Basic Latin", 			LatinBasic },
			{ "Latin", 					LatinBasic },
			{ "Latin Basic",			LatinBasic },
			{ "latin-basic", 			LatinBasic },
			//
			{ "Latin Extended A", 		LatinExtendedA },
			{ "Latin Extended-A", 		LatinExtendedA },
			{ "latin-ext-a", 			LatinExtendedA },
			//
			{ "Latin Extended B", 		LatinExtendedB },
			{ "Latin Extended-B", 		LatinExtendedB },
			{ "latin-ext-b", 			LatinExtendedB },
			//
			{ "Greek", 					Greek },
			//
			{ "Cyrillic", 				Cyrillic },
			//
			{ "Armenian", 				Armenian },
			//
			{ "Hebrew", 				Hebrew },
			//
			{ "Arabic", 				Arabic },
			// and so on
		};
    }
}

