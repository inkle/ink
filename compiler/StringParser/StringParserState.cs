
namespace Ink
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

        public int characterInLineIndex {
            get { return currentElement.characterInLineIndex; }
            set { currentElement.characterInLineIndex = value; }
        }

        public uint customFlags {
            get { return currentElement.customFlags; }
            set { currentElement.customFlags = value; }
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
            public int characterInLineIndex;
			public int lineIndex;
            public bool reportedErrorInScope;
            public int uniqueId;
            public uint customFlags;

			public Element() {

            }

            public void CopyFrom(Element fromElement)
            {
                _uniqueIdCounter++;
                this.uniqueId = _uniqueIdCounter;
                this.characterIndex = fromElement.characterIndex;
                this.characterInLineIndex = fromElement.characterInLineIndex;
                this.lineIndex = fromElement.lineIndex;
                this.customFlags = fromElement.customFlags;
                this.reportedErrorInScope = false;
            }

            // Squash is used when succeeding from a rule,
            // so only the state information we wanted to carry forward is
            // retained. e.g. characterIndex and lineIndex are global,
            // however uniqueId is specific to the individual rule,
            // and likewise, custom flags are designed for the temporary
            // state of the individual rule too.
            public void SquashFrom(Element fromElement)
            {
                this.characterIndex = fromElement.characterIndex;
                this.characterInLineIndex = fromElement.characterInLineIndex;
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
        // Used when succeeding from a rule (and ONLY when succeeding, since
        // the state of the top element is retained).
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

