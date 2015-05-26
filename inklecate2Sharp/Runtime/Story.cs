using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Runtime
{
	public class Story : Runtime.Object
	{
		public Path currentPath { get; protected set; }
        public List<Runtime.Object> outputStream;
        public Dictionary<string, Runtime.Object> variables { get; protected set; }

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

            variables = new Dictionary<string, Runtime.Object> ();

			_callStack = new List<Path> ();
		}

		public Runtime.Object ContentAtPath(Path path)
		{
			return _rootContainer.ContentAtPath (path);
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
                    bool isLogicOrFlowControl = PerformLogicAndFlowControl(currentContentObj);
                        
                    // Content to add to evaluation stack or the output stream
                    if( !isLogicOrFlowControl ) {
                        
                        // Expression evaluation content
                        if( _inExpressionEvaluation ) {
                            _evaluationStack.Add(currentContentObj);
                        }

                        // Output stream content (i.e. not expression evaluation)
                        else {
                            outputStream.Add(currentContentObj);
                        }
                    }

                    // Increment the content pointer, following diverts if necessary
					NextContent();

                    // Any push to the call stack should be done after the increment to the content pointer,
                    // so that when returning from the stack, it returns to the content after the push instruction
                    if( currentContentObj is StackPush ) {
						PushToCallStack();
					}
				}

			} while(currentContentObj != null && currentPath != null);
		}

        /// <summary>
        /// Checks whether contentObj is a control or flow object rather than a piece of content, 
        /// and performs the required command if necessary.
        /// </summary>
        /// <returns><c>true</c> if object was logic or flow control, <c>false</c> if it's normal content.</returns>
        /// <param name="contentObj">Content object.</param>
        private bool PerformLogicAndFlowControl(Runtime.Object contentObj)
        {
            if( contentObj == null ) {
                return false;
            }

            // Redirection?
            if( contentObj is Divert ) {
                Divert currentDivert = (Divert) contentObj;
                _divertedPath = currentDivert.targetPath;
                return true;
            }

            // Stack push?
            else if( contentObj is StackPush ) {

                // Actual stack push will be performed after Step in main loop
                return true;
            }

            // Start/end an expression evaluation? Or print out the result?
            else if( contentObj is EvaluationCommand ) {
                var evalCommand = (EvaluationCommand) contentObj;

                switch( evalCommand.commandType ) {
                case EvaluationCommand.CommandType.Start:
                    Debug.Assert(_inExpressionEvaluation == false, "Already in expression evaluation?");
                    _inExpressionEvaluation = true;
                    break;
                case EvaluationCommand.CommandType.End:
                    Debug.Assert(_inExpressionEvaluation == true, "Not in expression evaluation mode");
                    _inExpressionEvaluation = false;
                    break;
                case EvaluationCommand.CommandType.Output:
                    var output = PopEvaluationStack ();

                    // TODO: Should we really always blanket convert to string?
                    // It would be okay to have numbers in the output stream the
                    // only problem is when exporting text for viewing, it skips over numbers etc.
                    var text = new Text (output.ToString ());

                    outputStream.Add(text);
                    break;
                }

                return true;
            }

            // Variable assignment
            else if( contentObj is VariableAssignment ) {
                var varAss = (VariableAssignment) contentObj;
                var assignedVal = PopEvaluationStack();
                variables[varAss.variableName] = assignedVal;
                return true;
            }

            // Variable reference
            else if( contentObj is VariableReference ) {
                var varRef = (VariableReference)contentObj;
                _evaluationStack.Add( variables[varRef.name] );
                return true;
            }

            // Native function call
            else if( contentObj is NativeFunctionCall ) {
                var func = (NativeFunctionCall) contentObj;
                var funcParams = PopEvaluationStack(func.numberOfParamters);
                var result = func.Call(funcParams);
                _evaluationStack.Add(result);
                return true;
            }

            // No control content, must be ordinary content
            return false;
        }

		public void ContinueFromPath(Path path)
		{
			currentPath = path;
			Continue ();
		}

		public void ContinueWithChoiceIndex(int choiceIdx)
		{
			var choices = this.currentChoices;
			Debug.Assert (choiceIdx >= 0 && choiceIdx < choices.Count);

			var choice = choices [choiceIdx];

			outputStream.Add (new ChosenChoice (choice));

			ContinueFromPath (choice.pathOnChoice);
		}

        protected Runtime.Object PopEvaluationStack()
        {
            var obj = _evaluationStack.Last ();
            _evaluationStack.RemoveAt (_evaluationStack.Count - 1);
            return obj;
        }

        protected List<Runtime.Object> PopEvaluationStack(int numberOfObjects)
        {
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

		private void NextContent()
		{
			// Divert step?
			if (_divertedPath != null) {
				currentPath = _divertedPath;
				_divertedPath = null;
				return;
			}

			// Can we increment successfully?
			currentPath = _rootContainer.IncrementPath (currentPath);
			if (currentPath == null) {

				// Failed to increment, so we've run out of content
				// Try to pop call stack if possible
				if (_callStack.Count > 0) {

					// Pop currentPath from the call stack
					currentPath = _callStack.Last ();
					_callStack.RemoveAt (_callStack.Count - 1);

					// Step past the point where we last called out
					NextContent ();
				}
			}
		}

		private void PushToCallStack()
		{
			_callStack.Add (currentPath);
		}

		private Container _rootContainer;
		private Path _divertedPath;

		private List<Path> _callStack;

        private List<Runtime.Object> _evaluationStack;

        private bool _inExpressionEvaluation;
	}
}

