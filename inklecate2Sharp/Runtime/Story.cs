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

					bool shouldStackPush = false;
                    bool isContent = true;
					
					// Convert path to get first leaf content
					Container currentContainer = currentContentObj as Container;
					if( currentContainer != null ) {
						currentPath = currentPath.PathByAppendingPath(currentContainer.pathToFirstLeafContent);
						currentContentObj = ContentAtPath(currentPath);
					}

					// Redirection?
                    if( currentContentObj is Divert ) {
                        Divert currentDivert = (Divert) currentContentObj;
						_divertedPath = currentDivert.targetPath;
                        isContent = false;
					}

                    // Any expression evaluation
                    else if ( TryExpressionEvaluation(currentContentObj) ) {
                        isContent = false;
                    }

					// Stack push?
					// Defer it so that the path that's saved is *after the stack push
					else if( currentContentObj is StackPush ) {
						shouldStackPush = true;
                        isContent = false;
					}
                        
                    // Content to add to evaluation stack or the output stream
                    if( isContent ) {
                        
                        // Expression evaluation content
                        if( _inExpressionEvaluation ) {
                            _evaluationStack.Add(currentContentObj);
                        }

                        // Output stream content (i.e. not expression evaluation)
                        else {
                            outputStream.Add(currentContentObj);
                        }
                    }

					Step();

					if( shouldStackPush ) {
						PushToCallStack();
					}
				}

			} while(currentContentObj != null && currentPath != null);
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

		private void Step()
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
					Step ();
				}
			}
		}

        private bool TryExpressionEvaluation(Runtime.Object currentContentObj)
        {
            if( currentContentObj == null ) {
                return false;
            }

            // Get evaluated value
            EvaluationCommand evalCommand = currentContentObj as EvaluationCommand;
            if( evalCommand != null ) {

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
            VariableAssignment varAss = currentContentObj as VariableAssignment;
            if( varAss != null ) {
                var assignedVal = PopEvaluationStack();
                variables[varAss.variableName] = assignedVal;
                return true;
            }

            // Variable reference
            VariableReference varRef = currentContentObj as VariableReference;
            if( varRef != null ) {
                _evaluationStack.Add( variables[varRef.name] );
                return true;
            }

            // Native function call
            NativeFunctionCall func = currentContentObj as NativeFunctionCall;
            if( func != null ) {
                var funcParams = PopEvaluationStack(func.numberOfParamters);
                var result = func.Call(funcParams);
                _evaluationStack.Add(result);
                return true;
            }
                
            // No expression evaluation done
            return false;
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

