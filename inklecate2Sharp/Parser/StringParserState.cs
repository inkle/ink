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

        public bool errorReportedAlreadyInScope {
            get {
                return currentElement.reportedErrorInScope;
            }
        }

        public int stackHeight {
            get {
                return _stack.Count;
            }
        }
					
		public class Element {
			public int characterIndex;
			public int lineIndex;
            public bool reportedErrorInScope;
            public int uniqueId { get { return _uniqueId; } set { _uniqueId = value; } }

			public Element() {
                _uniqueIdCounter++;
                _uniqueId = _uniqueIdCounter;
            }

            public Element(int characterIndex, int lineIndex) : this() {
				this.characterIndex = characterIndex;
				this.lineIndex = lineIndex;
			}

            public Element Copy()
			{
				return new Element (characterIndex, lineIndex);
			}

            int _uniqueId;

            static int _uniqueIdCounter;
		}

		public StringParserState ()
		{
			_stack = new List<Element> ();

			// Default element
			_stack.Add (new Element ());
		}

		public int Push()
		{
            var newEl = this.currentElement.Copy ();

            _stack.Add (newEl);

            return newEl.uniqueId;
		}

        public void Pop(int expectedRuleId)
		{
			if (_stack.Count == 1) {
				throw new System.Exception ("Attempting to remove final stack element is illegal! Mismatched Begin/Succceed/Fail?");
			}

            if (_stack.Last ().uniqueId != expectedRuleId)
                throw new System.Exception ("Mismatched rule IDs - do you have mismatched Begin/Succeed/Fail?");

			// Restore state
			_stack.RemoveAt (_stack.Count - 1);
		}

        public Element Peek(int expectedRuleId)
		{
            var lastEl = _stack.Last ();

            if (lastEl.uniqueId != expectedRuleId)
                throw new System.Exception ("Mismatched rule IDs - do you have mismatched Begin/Succeed/Fail?");

            return lastEl;
		}

        public Element PeekPenultimate()
        {
            if (_stack.Count >= 2) {
                return _stack [_stack.Count - 2];
            } else {
                return null;
            }
        }

		// Reduce stack height while maintaining currentElement
		// Remove second last element: i.e. "squash last two elements together"
		public void Squash()
		{
			if (_stack.Count < 2) {
				throw new System.Exception ("Attempting to remove final stack element is illegal! Mismatched Begin/Succceed/Fail?");
			}

            var penultimateEl = _stack [_stack.Count - 2];
            var penultimateUniqueId = penultimateEl.uniqueId;
            _stack.Last ().uniqueId = penultimateUniqueId;
				
			_stack.RemoveAt (_stack.Count - 2);
		}

        public void NoteErrorReported()
        {
            foreach (var el in _stack) {
                el.reportedErrorInScope = true;
            }
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

