using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Runtime
{
	public class Story : Runtime.Object
	{
		public Path currentPath { get; protected set; }
		public List<object> outputStream;

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

			outputStream = new List<object> ();

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
					
					// Convert path to get first leaf content
					Container currentContainer = currentContentObj as Container;
					if( currentContainer != null ) {
						currentPath = currentPath.PathByAppendingPath(currentContainer.pathToFirstLeafContent);
						currentContentObj = ContentAtPath(currentPath);
					}

					// Redirection?
					Divert currentDivert = currentContentObj as Divert;
					if( currentDivert != null ) {
						_divertedPath = currentDivert.targetPath;
					}

					// Stack push?
					// Defer it so that the path that's saved is *after the stack push
					else if( currentContentObj is StackPush ) {
						shouldStackPush = true;
					}

					// Content
					else {
						outputStream.Add(currentContentObj);
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

		private void PushToCallStack()
		{
			_callStack.Add (currentPath);
		}

		private Container _rootContainer;
		private Path _divertedPath;

		private List<Path> _callStack;
	}
}

