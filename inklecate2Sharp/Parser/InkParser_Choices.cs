using System;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser
	{
		protected Choice Choice()
		{
			BeginRule ();

			Whitespace ();

			if (ParseString ("*") == null) {
				return (Choice) FailRule ();
			}

			Whitespace ();

			if( _choicePauseCharacters == null ) {
				_choicePauseCharacters = new CharacterSet ("-");
			}
			if (_choiceEndCharacters == null) {
				_choiceEndCharacters = new CharacterSet("{\n\r");
			}

			string choiceText = ParseUntil(Divert, pauseCharacters: _choicePauseCharacters, endCharacters: _choiceEndCharacters);

			choiceText = choiceText.TrimEnd (' ');

			Whitespace ();

			var divert =  Divert ();

			return SucceedRule( new Choice(choiceText, divert) ) as Choice;

		}

		private CharacterSet _choicePauseCharacters;
		private CharacterSet _choiceEndCharacters;
	}
}

