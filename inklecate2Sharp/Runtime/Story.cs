using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace Inklewriter.Runtime
{
	public class Story : Runtime.Object
	{
        public Path currentPath { 
            get { 
                return _callStack.currentElement.path; 
            } 
            protected set {
                _callStack.currentElement.path = value;
            }
        }

        public List<Runtime.Object> outputStream;

        public Dictionary<string, Runtime.Object> variables { 
            get { 
                return _callStack.currentElement.variables; 
            } 
        }

		public List<Choice> currentChoices
		{
			get 
			{
				return CurrentOutput<Choice> ();
			}
		}

		public string currentText
		{
			get 
			{
				return string.Join(separator:"", values: CurrentOutput<Runtime.Text> ());
			}
		}

		public Story (Container rootContainer)
		{
			_rootContainer = rootContainer;

            outputStream = new List<Runtime.Object> ();

            _evaluationStack = new List<Runtime.Object> ();

            _callStack = new CallStack ();

            _sequenceCounts = new Dictionary<string, int> ();

            // Seed the shuffle random numbers
            int timeSeed = DateTime.Now.Millisecond;
            _storySeed = (new Random (timeSeed)).Next () % 100;
		}

		public Runtime.Object ContentAtPath(Path path)
		{
			return rootContainer.ContentAtPath (path);
		}

		public void Begin()
		{
			currentPath = Path.ToFirstElement ();
			Continue ();
		}

		public void Continue()
		{
			Runtime.Object currentContentObj = null;
			do {

				currentContentObj = ContentAtPath(currentPath);
				if( currentContentObj != null ) {
                					
					// Convert path to get first leaf content
					Container currentContainer = currentContentObj as Container;
					if( currentContainer != null ) {
						currentPath = currentPath.PathByAppendingPath(currentContainer.pathToFirstLeafContent);
						currentContentObj = ContentAtPath(currentPath);
					}

                    // Is the current content object:
                    //  - Normal content
                    //  - Or a logic/flow statement - if so, do it
                    bool stopFlow;
                    bool isLogicOrFlowControl = PerformLogicAndFlowControl(currentContentObj, out stopFlow);
                    if( stopFlow ) {
                        currentContentObj = null;
                        currentPath = null;
                        break;
                    }

                    // Choice with condition?
                    bool shouldAddObject = true;
                    var choice = currentContentObj as Choice;
                    if( choice != null && choice.hasCondition ) {
                        var conditionValue = PopEvaluationStack();
                        shouldAddObject = IsTruthy(conditionValue);
                    }

                    // Error?
                    if( currentContentObj is Error ) {
                        var err = (Error)currentContentObj;
                        Error(err.message, err.useEndLineNumber);
                        return;
                    }

                    // Content to add to evaluation stack or the output stream
                    if( !isLogicOrFlowControl && shouldAddObject ) {
                        
                        // Expression evaluation content
                        if( inExpressionEvaluation ) {
                            PushEvaluationStack(currentContentObj);
                        }

                        // Output stream content (i.e. not expression evaluation)
                        else {
                            PushToOutputStream(currentContentObj);
                        }
                    }

                    // Increment the content pointer, following diverts if necessary
					NextContent();

                    // Any push to the call stack should be done after the increment to the content pointer,
                    // so that when returning from the stack, it returns to the content after the push instruction
                    bool isStackPush = currentContentObj is ControlCommand && ((ControlCommand)currentContentObj).commandType == ControlCommand.CommandType.StackPush;
                    if( isStackPush ) {
                        _callStack.Push();
					}

				}

			} while(currentContentObj != null && currentPath != null);
		}

        // Does the expression result represented by this object evaluate to true?
        // e.g. is it a Number that's not equal to 1?
        bool IsTruthy(Runtime.Object obj)
        {
            bool truthy = false;
            if (obj is Literal) {
                var literal = (Literal)obj;
                return literal.isTruthy;
            }
            return truthy;
        }

        /// <summary>
        /// Checks whether contentObj is a control or flow object rather than a piece of content, 
        /// and performs the required command if necessary.
        /// </summary>
        /// <returns><c>true</c> if object was logic or flow control, <c>false</c> if it's normal content.</returns>
        /// <param name="contentObj">Content object.</param>
        private bool PerformLogicAndFlowControl(Runtime.Object contentObj, out bool stopFlow)
        {
            stopFlow = false;

            if( contentObj == null ) {
                return false;
            }

            // Divert
            if (contentObj is Divert) {
                
                Divert currentDivert = (Divert)contentObj;
                if (currentDivert.hasVariableTarget) {
                    var varName = currentDivert.variableDivertName;
                    var varContents = _callStack.GetVariableWithName (varName);

                    if ( !(varContents is LiteralDivertTarget) ) {
                        Error("Tried to divert to a target from a variable, but the variable ("+varName+") didn't contain a divert target, it contained '"+varContents+"'");
                    }

                    var target = (LiteralDivertTarget)varContents;
                    currentDivert = target.divert;

                }

                _divertedPath = currentDivert.targetPath;

                if (_divertedPath == null) {
                    Error ("Divert resolution failed: " + currentDivert);
                }

                Assert (_divertedPath != null, "diverted path is null");
                return true;
            } 

            // Branch (conditional divert)
            else if (contentObj is Branch) {
                var branch = (Branch)contentObj;
                var conditionValue = PopEvaluationStack();

                if ( IsTruthy(conditionValue) )
                    _divertedPath = branch.trueDivert.targetPath;
                else if (branch.falseDivert != null)
                    _divertedPath = branch.falseDivert.targetPath;
                
                return true;
            }

            // Start/end an expression evaluation? Or print out the result?
            else if( contentObj is ControlCommand ) {
                var evalCommand = (ControlCommand) contentObj;

                switch (evalCommand.commandType) {

                case ControlCommand.CommandType.EvalStart:
                    Assert (inExpressionEvaluation == false, "Already in expression evaluation?");
                    inExpressionEvaluation = true;
                    break;

                case ControlCommand.CommandType.EvalEnd:
                    Assert (inExpressionEvaluation == true, "Not in expression evaluation mode");
                    inExpressionEvaluation = false;
                    break;

                case ControlCommand.CommandType.EvalOutput:

                    // If the expression turned out to be empty, there may not be anything on the stack
                    if (_evaluationStack.Count > 0) {
                        
                        var output = PopEvaluationStack ();

                        // Functions may evaluate to Void, in which case we skip output
                        if (!(output is Void)) {
                            // TODO: Should we really always blanket convert to string?
                            // It would be okay to have numbers in the output stream the
                            // only problem is when exporting text for viewing, it skips over numbers etc.
                            var text = new Text (output.ToString ());

                            outputStream.Add (text);
                        }

                    }
                    break;

                // Actual stack push/pop will be performed after Step in main loop
                case ControlCommand.CommandType.StackPush:
                    break;

                case ControlCommand.CommandType.StackPop:
                    if (_callStack.canPop) {
                        _callStack.Pop ();
                    } else {
                        stopFlow = true;
                    }
                    break;

                case ControlCommand.CommandType.NoOp:
                    break;

                case ControlCommand.CommandType.Duplicate:
                    PushEvaluationStack (PeekEvaluationStack ());
                    break;

                case ControlCommand.CommandType.ChoiceCount:
                    var choiceCount = currentChoices.Count;
                    PushEvaluationStack (new Runtime.LiteralInt (choiceCount));
                    break;

                case ControlCommand.CommandType.SequenceCount:
                    var count = currentSequenceCount;
                    PushEvaluationStack (new LiteralInt (count));
                    break;

                case ControlCommand.CommandType.SequenceIncrement:
                    currentSequenceCount++;
                    break;

                case ControlCommand.CommandType.SequenceShuffleIndex:
                    var shuffleIndex = NextSequenceShuffleIndex ();
                    PushEvaluationStack (new LiteralInt (shuffleIndex));
                    break;

                default:
                    Error ("unhandled ControlCommand: " + evalCommand);
                    break;
                }

                return true;
            }

            // Variable assignment
            else if( contentObj is VariableAssignment ) {
                var varAss = (VariableAssignment) contentObj;
                var assignedVal = PopEvaluationStack();

                // When in temporary evaluation, don't create new variables purely within
                // the temporary context, but attempt to create them globally
                var prioritiseHigherInCallStack = _temporaryEvaluationContainer != null;

                _callStack.SetVariable (varAss.variableName, assignedVal, varAss.isNewDeclaration, prioritiseHigherInCallStack);

                return true;
            }

            // Variable reference
            else if( contentObj is VariableReference ) {
                var varRef = (VariableReference)contentObj;
                var varContents = _callStack.GetVariableWithName (varRef.name);
                if (varContents == null) {
                    
                    Error("Uninitialised variable: " + varRef.name);
                }
                _evaluationStack.Add( varContents );
                return true;
            }

            // Native function call
            else if( contentObj is NativeFunctionCall ) {
                var func = (NativeFunctionCall) contentObj;
                var funcParams = PopEvaluationStack(func.numberOfParameters);
                var result = func.Call(funcParams);
                _evaluationStack.Add(result);
                return true;
            }

            // No control content, must be ordinary content
            return false;
        }

        // TODO: Add choice marker is a hack, do it a better way!
        // The problem is that ContinueFromPath may be called externally,
        // and if it is then it wouldn't have a ChosenChoice to mark where
        // the last chunk of content ended
        public void ContinueFromPath(Path path, bool addChoiceMarker = true)
		{
            if (addChoiceMarker) {
                outputStream.Add (new ChosenChoice (null));
            }

            _previousPath = currentPath;

			currentPath = path;
			Continue ();
		}

		public void ContinueWithChoiceIndex(int choiceIdx)
		{
			var choices = this.currentChoices;
			Assert (choiceIdx >= 0 && choiceIdx < choices.Count, "choice out of range");

			var choice = choices [choiceIdx];

			outputStream.Add (new ChosenChoice (choice));

			ContinueFromPath (choice.pathOnChoice, addChoiceMarker:false);
		}

        public Runtime.Object EvaluateExpression(Runtime.Container exprContainer)
        {
            int startCallStackHeight = _callStack.elements.Count;

            _callStack.Push ();

            _temporaryEvaluationContainer = exprContainer;

            currentPath = Path.ToFirstElement ();

            int evalStackHeight = _evaluationStack.Count;

            try {
                Continue ();
            } catch(System.Exception e) {
                Console.WriteLine (e.Message);
            }

            _temporaryEvaluationContainer = null;

            // Should have fallen off the end of the Container, which should
            // have auto-popped, but just in case we didn't for some reason,
            // manually pop to restore the state (including currentPath).
            if (_callStack.elements.Count > startCallStackHeight) {
                _callStack.Pop ();
            }

            int endStackHeight = _evaluationStack.Count;
            if (endStackHeight > evalStackHeight) {
                return PopEvaluationStack ();
            } else {
                return null;
            }

        }

        protected void PushToOutputStream(Runtime.Object obj)
        {
            // Glue: absorbs newlines both before and after it,
            // causing two piece of inline text to stay on the same line.
            bool outputStreamEndsInGlue = false;
            if (outputStream.Count > 0) {
                outputStreamEndsInGlue = outputStream.Last () is Glue;
            }

            if (obj is Text) {
                var text = (Text)obj;

                bool canAppendNewline = !outputStreamEndsInNewline && !outputStreamEndsInGlue;

                // Newline: don't allow more than one
                if (text.text == "\n") {
                    if( !canAppendNewline )
                        return;
                } 

                // General text: 
                else {

                    // Remove newlines from start, and add as a single newline Text
                    var lengthBeforeTrim = text.text.Length;
                    var trimmedText = text.text.TrimStart ('\n');
                    if (trimmedText.Length != lengthBeforeTrim && canAppendNewline) {
                        outputStream.Add(new Text ("\n"));
                    }

                    // Remove newlines from end
                    lengthBeforeTrim = trimmedText.Length;
                    trimmedText = text.text.TrimEnd ('\n');

                    // Anything left or was it just pure newlines?
                    if (trimmedText.Length > 0) {
                        
                        // Add main text to output stream
                        outputStream.Add(new Text (trimmedText));

                        // Add single trailing newline if necessary
                        if (trimmedText.Length != lengthBeforeTrim) {
                            outputStream.Add(new Text ("\n"));
                        }
                    }
                        
                    return;
                }

            } 

            // New glue: remove any existing trailing newline from output stream
            else if (obj is Glue) {
                if (outputStreamEndsInNewline) {
                    outputStream.RemoveAt (outputStream.Count - 1);
                }
            }

            // Only remove an existing glue if we're definitely now
            // adding something new on top, since it's served its purpose.
            if( outputStreamEndsInGlue ) 
                outputStream.RemoveAt (outputStream.Count - 1);
            
            outputStream.Add(obj);
        }

        bool outputStreamEndsInNewline {
            get {
                if (outputStream.Count > 0) {
                    var text = outputStream.Last () as Text;
                    if (text != null) {
                        return text.text == "\n";
                    }
                }

                return false;
            }
        }

        protected void PushEvaluationStack(Runtime.Object obj)
        {
            _evaluationStack.Add(obj);
        }

        protected Runtime.Object PopEvaluationStack()
        {
            var obj = _evaluationStack.Last ();
            _evaluationStack.RemoveAt (_evaluationStack.Count - 1);
            return obj;
        }

        protected Runtime.Object PeekEvaluationStack()
        {
            return _evaluationStack.Last ();
        }

        protected List<Runtime.Object> PopEvaluationStack(int numberOfObjects)
        {
            Assert (numberOfObjects <= _evaluationStack.Count, "trying to pop too many objects");
            var popped = _evaluationStack.GetRange (_evaluationStack.Count - numberOfObjects, numberOfObjects);
            _evaluationStack.RemoveRange (_evaluationStack.Count - numberOfObjects, numberOfObjects);
            return popped;
        }
			
		public List<T> CurrentOutput<T>() where T : class
		{
			List<T> result = new List<T> ();

			for (int i = outputStream.Count - 1; i >= 0; --i) {
				object outputObj = outputStream [i];

				// "Current" is defined as "since last chosen choice"
				if (outputObj is ChosenChoice) {
					break;
				}

				T outputOfType = outputObj as T;
				if (outputOfType != null) {

					// Insert rather than Add since we're iterating in reverse
					result.Insert (0, outputOfType);
				}
			}

			return result;
		}

        public virtual string BuildStringOfHierarchy()
        {
            var sb = new StringBuilder ();

            Runtime.Object currentObj = null;
            if (currentPath != null) {
                currentObj = ContentAtPath (currentPath);
            }
            rootContainer.BuildStringOfHierarchy (sb, 0, currentObj);

            return sb.ToString ();
        }

		private void NextContent()
		{
            _previousPath = currentPath;

			// Divert step?
			if (_divertedPath != null) {
				currentPath = _divertedPath;
				_divertedPath = null;

                // Diverted location has valid content?
                if (ContentAtPath (currentPath) != null) {
                    return;
                }
				
                // Otherwise, if diverted location doesn't have valid content,
                // drop down and attempt to increment.
                // This can happen if the diverted path is intentionally jumping
                // to the end of a container - e.g. a Conditional that's re-joining
			}

			// Can we increment successfully?
			currentPath = rootContainer.IncrementPath (currentPath);
			if (currentPath == null) {

				// Failed to increment, so we've run out of content
				// Try to pop call stack if possible
				if ( _callStack.canPop ) {

					// Pop from the call stack
                    _callStack.Pop();

                    // This pop was due to dropping off the end of a function that didn't return anything,
                    // so in this case, we make sure that the evaluator has something to chomp on if it needs it
                    if (inExpressionEvaluation) {
                        PushEvaluationStack (new Runtime.Void ());
                    }

					// Step past the point where we last called out
                    if (currentPath != null) {
                        NextContent ();
                    }
				}
			}
		}

        int currentSequenceCount {
            get {
                Runtime.Container closestContainer = ClosestContainerAtPath (currentPath);
                var sequencePathStr = closestContainer.path.ToString();
                int count = 0;
                _sequenceCounts.TryGetValue (sequencePathStr, out count);
                return count;
            }
            set {
                Runtime.Container closestContainer = ClosestContainerAtPath (currentPath);
                var sequencePathStr = closestContainer.path.ToString();
                _sequenceCounts [sequencePathStr] = value;
            }
        }

        // Note that this is O(n), since it re-evaluates the shuffle indices
        // from a consistent seed each time.
        // TODO: Is this the best algorithm it can be?
        int NextSequenceShuffleIndex()
        {
            var numElementsLiteral = PopEvaluationStack () as LiteralInt;
            if (numElementsLiteral == null) {
                Error ("expected number of elements in sequence for shuffle index");
                return 0;
            }

            var seqContainer = ClosestContainerAtPath (currentPath);

            int numElements = numElementsLiteral.value;

            var seqCount = currentSequenceCount;
            var loopIndex = seqCount / numElements;
            var iterationIndex = seqCount % numElements;

            // Generate the same shuffle based on:
            //  - The hash of this container, to make sure it's consistent
            //    each time the runtime returns to the sequence
            //  - How many times the runtime has looped around this full shuffle
            var seqPathStr = seqContainer.path.ToString();
            int sequenceHash = 0;
            foreach (char c in seqPathStr) {
                sequenceHash += c;
            }
            var randomSeed = sequenceHash + loopIndex + _storySeed;
            var random = new Random (randomSeed);

            var unpickedIndices = new List<int> ();
            for (int i = 0; i < numElements; ++i) {
                unpickedIndices.Add (i);
            }

            for (int i = 0; i <= iterationIndex; ++i) {
                var chosen = random.Next () % unpickedIndices.Count;
                var chosenIndex = unpickedIndices [chosen];
                unpickedIndices.RemoveAt (chosen);

                if (i == iterationIndex) {
                    return chosenIndex;
                }
            }

            throw new System.Exception ("Should never reach here");
        }

        Runtime.Container ClosestContainerAtPath(Path path)
        {
            var content = ContentAtPath (path);
            while (content != null && !(content is Container)) {
                content = content.parent;
            }
            return (Runtime.Container) content;
        }

        void Error(string message, bool useEndLineNum = false)
        {
            
            var dm = currentDebugMetadata;

            if (dm != null) {
                int lineNum = useEndLineNum ? dm.endLineNumber : dm.startLineNumber;
                message = string.Format ("Runtime error in {0} line {1}: {2}", dm.fileName, lineNum, message);
            } else {
                message = "Runtime error" +
                    ": "+message;
            }

            throw new System.Exception (message);
            // BuildStringOfHierarchy
        }

        void Assert(bool condition, string message = null, params object[] formatParams)
        {
            if (condition == false) {
                if (message == null) {
                    message = "Story assert";
                }
                if (formatParams != null && formatParams.Count() > 0) {
                    message = string.Format (message, formatParams);
                }

                Error (message);
            }
        }

        DebugMetadata currentDebugMetadata
        {
            get {
                DebugMetadata dm;

                // Try to get from the current path first
                dm = DebugMetadataAtPath(currentPath);
                if (dm != null) {
                    return dm;
                }

                // Try last path
                dm = DebugMetadataAtPath (_previousPath);
                if (dm != null) {
                    return dm;
                }

                // Move up callstack if possible
                for (int i = _callStack.elements.Count - 1; i >= 0; --i) {
                    var path = _callStack.elements [i].path;
                    dm = DebugMetadataAtPath(path);
                    if (dm != null) {
                        return dm;
                    }
                }

                // Current/previous path may not be valid if we've just had an error,
                // or if we've simply run out of content.
                // As a last resort, try to grab something from the output stream
                for (int i = outputStream.Count - 1; i >= 0; --i) {
                    var outputObj = outputStream [i];
                    dm = outputObj.debugMetadata;
                    if (dm != null) {
                        return dm;
                    }
                }

                return null;
            }
        }

        int currentLineNumber 
        {
            get {
                var dm = currentDebugMetadata;
                if (dm != null) {
                    return dm.startLineNumber;
                }
                return 0;
            }
        }

        DebugMetadata DebugMetadataAtPath(Path path)
        {
            if (path != null) {
                var currentObj = ContentAtPath (path);
                if (currentObj != null) {
                    var dm = currentObj.debugMetadata;
                    if (dm != null) {
                        return dm;
                    }
                }
            }

            return null;
        }

        Container rootContainer {
            get {
                if (_temporaryEvaluationContainer != null) {
                    return _temporaryEvaluationContainer;
                } else {
                    return _rootContainer;
                }
            }
        }

        private Container _rootContainer;
        private Container _temporaryEvaluationContainer;
        private Path _divertedPath;
            
        private CallStack _callStack;

        private Dictionary<string, int> _sequenceCounts;
        private int _storySeed;

        private List<Runtime.Object> _evaluationStack;

        private Path _previousPath;

        private bool inExpressionEvaluation {
            get {
                return _callStack.currentElement.inExpressionEvaluation;
            }
            set {
                _callStack.currentElement.inExpressionEvaluation = value;
            }
        }
	}
}

