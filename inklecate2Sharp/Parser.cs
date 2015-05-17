using System;

namespace inklecate2Sharp
{
	public class Parser
	{
		public string inputString { get; }

		private char[] _chars;
		private int _index;

		public Parser (string str)
		{
			_chars = str.ToCharArray ();
			
			inputString = str;
		}

		public void Parse()
		{
			ParseString ("Hello");
			ParseString (" ");
			ParseString ("world");
		}

		public bool ParseString(string str)
		{
			if (str.Length > remainingLength) {
				return false;
			}

			int oldIndex = _index;

			bool success = true;
			foreach (char c in str) {
				if ( _chars[_index] != c) {
					success = false;
					break;
				}
				_index++;
			}

			if (!success) {
				_index = oldIndex;
			}

			return success;
		}

		public string remainingString
		{
			get {
				return new string(_chars, _index, remainingLength);
			}
		}

		public int remainingLength
		{
			get {
				return _chars.Length - _index;
			}
		}
	}
}

