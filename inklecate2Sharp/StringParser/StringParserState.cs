
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
                return _numElements;
            }
        }
					
		public class Element {
			public int characterIndex;
			public int lineIndex;
            public bool reportedErrorInScope;
            public int uniqueId;

			public Element() {

            }

            public void CopyFrom(Element fromElement)
            {
                _uniqueIdCounter++;
                this.uniqueId = _uniqueIdCounter;
                this.characterIndex = fromElement.characterIndex;
                this.lineIndex = fromElement.lineIndex;
                reportedErrorInScope = false;
            }

            public void SquashFrom(Element fromElement)
            {
                this.characterIndex = fromElement.characterIndex;
                this.lineIndex = fromElement.lineIndex;
                this.reportedErrorInScope = fromElement.reportedErrorInScope;
            }

            static int _uniqueIdCounter;
		}

		public StringParserState ()
		{
            const int kExpectedMaxStackDepth = 200;
            _stack = new Element[kExpectedMaxStackDepth];

            for (int i = 0; i < kExpectedMaxStackDepth; ++i) {
                _stack [i] = new Element ();
            }

            _numElements = 1;
		}

		public int Push()
		{
            if (_numElements >= _stack.Length)
                throw new System.Exception ("Stack overflow in parser state");

            var prevElement = _stack [_numElements - 1];
            var newElement = _stack[_numElements];
            _numElements++;

            newElement.CopyFrom (prevElement);

            return newElement.uniqueId;
		}

        public void Pop(int expectedRuleId)
		{
            if (_numElements == 1) {
				throw new System.Exception ("Attempting to remove final stack element is illegal! Mismatched Begin/Succceed/Fail?");
			}

            if ( currentElement.uniqueId != expectedRuleId)
                throw new System.Exception ("Mismatched rule IDs - do you have mismatched Begin/Succeed/Fail?");

			// Restore state
            _numElements--;
		}

        public Element Peek(int expectedRuleId)
		{
            if (currentElement.uniqueId != expectedRuleId)
                throw new System.Exception ("Mismatched rule IDs - do you have mismatched Begin/Succeed/Fail?");

            return _stack[_numElements-1];
		}

        public Element PeekPenultimate()
        {
            if (_numElements >= 2) {
                return _stack [_numElements - 2];
            } else {
                return null;
            }
        }

		// Reduce stack height while maintaining currentElement
		// Remove second last element: i.e. "squash last two elements together"
		public void Squash()
		{
            if (_numElements < 2) {
				throw new System.Exception ("Attempting to remove final stack element is illegal! Mismatched Begin/Succceed/Fail?");
			}

            var penultimateEl = _stack [_numElements - 2];
            var lastEl = _stack [_numElements - 1];

            penultimateEl.SquashFrom (lastEl);
				
            _numElements--;
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
                return _stack [_numElements - 1];
			}
		}

        private Element[] _stack;
        private int _numElements;
	}
}

