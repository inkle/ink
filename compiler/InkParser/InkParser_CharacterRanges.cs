using Ink.Parsed;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Ink
{
    public partial class InkParser
    {
		public static readonly CharacterRange LatinBasic = 
			CharacterRange.Define ('\u0041', '\u007A', excludes: new CharacterSet().AddRange('\u005B', '\u0060'));
		public static readonly CharacterRange LatinExtendedA = CharacterRange.Define('\u0100', '\u017F'); // no excludes here
		public static readonly CharacterRange LatinExtendedB = CharacterRange.Define('\u0180', '\u024F'); // no excludes here
		public static readonly CharacterRange Greek = 
			CharacterRange.Define('\u0370', '\u03FF', excludes: new CharacterSet().AddRange('\u0378','\u0385').AddCharacters("\u0374\u0375\u0378\u0387\u038B\u038D\u03A2"));
		public static readonly CharacterRange Cyrillic = 
			CharacterRange.Define('\u0400', '\u04FF', excludes: new CharacterSet().AddRange('\u0482', '\u0489'));
		public static readonly CharacterRange Armenian = 
			CharacterRange.Define('\u0530', '\u058F', excludes: new CharacterSet().AddCharacters("\u0530").AddRange('\u0557', '\u0560').AddRange('\u0588', '\u058E'));
		public static readonly CharacterRange Hebrew = 
			CharacterRange.Define('\u0590', '\u05FF', excludes: new CharacterSet());
		public static readonly CharacterRange Arabic = 
			CharacterRange.Define('\u0600', '\u06FF', excludes: new CharacterSet());
		public static readonly CharacterRange Korean =
			CharacterRange.Define('\uAC00', '\uD7AF', excludes: new CharacterSet());

        private void ExtendIdentifierCharacterRanges(CharacterSet identifierCharSet)
        {
            var characterRanges = ListAllCharacterRanges();

            foreach (var charRange in characterRanges)
            {
                identifierCharSet.AddCharacters(charRange.ToCharacterSet());
            }
        }

        /// <summary>
        /// Gets an array of <see cref="CharacterRange" /> representing all of the currently supported
        /// non-ASCII character ranges that can be used in identifier names.
        /// </summary>
        /// <returns>
        /// An array of <see cref="CharacterRange" /> representing all of the currently supported
        /// non-ASCII character ranges that can be used in identifier names.
        /// </returns>
        public static CharacterRange[] ListAllCharacterRanges() {
            return new CharacterRange[] {
                LatinBasic,
                LatinExtendedA,
                LatinExtendedB,
                Arabic,
                Armenian,
                Cyrillic,
                Greek,
                Hebrew,
                Korean,
            };
        }
	}
}

