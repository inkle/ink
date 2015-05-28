using System;
using System.Collections.Generic;
using System.Linq;

namespace Inklewriter
{
	public class StringParserState
	{
		public int lineIndex { 
			get { return currentElement.lineIndex; } 
			set { currentElement.lineIndex = value; } 
		}

		public int characterIndex { 
			get { return currentElement.characterIndex; } 
			set { currentElement.characterIndex = value; } 
		}
					
		public class Element : ICloneable {
			public int characterIndex;
			public int lineIndex;

			public Element() {}

			public Element(int characterIndex, int lineIndex) {
				this.characterIndex = characterIndex;
				this.lineIndex = lineIndex;
			}

			public object Clone()
			{
				return new Element (characterIndex, lineIndex);
			}
		}

		public StringParserState ()
		{
			_stack = new List<Element> ();

			// Default element
			_stack.Add (new Element ());
		}

		public void Push()
		{
			_stack.Add (this.currentElement.Clone () as Element);
		}

		public void Pop()
		{
			if (_stack.Count == 1) {
				throw new System.Exception ("Attempting to remove final stack element is illegal! Mismatched Begin/Succceed/Fail?");
			}

			// Restore state
			_stack.RemoveAt (_stack.Count - 1);
		}

		public Element Peek()
		{
			return _stack.Last ();
		}

		// Reduce stack height while maintaining currentElement
		// Remove second last element: i.e. "squash last two elements together"
		public void Squash()
		{
			if (_stack.Count < 2) {
				throw new System.Exception ("Attempting to remove final stack element is illegal! Mismatched Begin/Succceed/Fail?");
			}
				
			_stack.RemoveAt (_stack.Count - 2);
		}

		protected Element currentElement
		{
			get {
				return _stack.Last ();
			}
		}

		private List<Element> _stack;
	}
}

