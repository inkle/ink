using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Ink.Runtime
{
    internal class StoryState
    {
        // REMEMBER! REMEMBER! REMEMBER!
        // When adding state, update the Copy method
        // REMEMBER! REMEMBER! REMEMBER!

        public List<Runtime.Object> outputStream { get { return _outputStream; } }
        public List<ChoiceInstance> currentChoices { get; private set; }
        public List<string> currentErrors { get; private set; }
        public VariablesState variablesState { get; private set; }
        public CallStack callStack { get; private set; }
        public List<Runtime.Object> evaluationStack { get; private set; }
        public Path divertedPath { get; set; }
        public Dictionary<string, int> visitCounts { get; private set; }
        public Dictionary<string, int> turnIndices { get; private set; }
        public int currentTurnIndex { get; private set; }
        public int storySeed { get; private set; }
        public bool didSafeExit { get; set; }

        public Path currentPath { 
            get { 
                return callStack.currentElement.path; 
            } 
            set {
                callStack.currentElement.path = value;
            }
        }

        public Path previousPath { get; set; }

        public bool hasError
        {
            get {
                return currentErrors != null && currentErrors.Count > 0;
            }
        }

        public string currentText
        {
            get 
            {
                var sb = new StringBuilder ();

                foreach (var outputObj in _outputStream) {
                    var textContent = outputObj as Text;
                    if (textContent != null) {
                        sb.Append(textContent.text);
                    }
                }

                return sb.ToString ();
            }
        }

        public bool inExpressionEvaluation {
            get {
                return callStack.currentElement.inExpressionEvaluation;
            }
            set {
                callStack.currentElement.inExpressionEvaluation = value;
            }
        }



        public StoryState ()
        {
            _outputStream = new List<Runtime.Object> ();

            evaluationStack = new List<Runtime.Object> ();

            callStack = new CallStack ();
            variablesState = new VariablesState (callStack);

            visitCounts = new Dictionary<string, int> ();
            turnIndices = new Dictionary<string, int> ();
            currentTurnIndex = -1;

            // Seed the shuffle random numbers
            int timeSeed = DateTime.Now.Millisecond;
            storySeed = (new Random (timeSeed)).Next () % 100;

            currentChoices = new List<ChoiceInstance> ();

            // Go to start
            currentPath = Path.ToFirstElement ();
        }

        // Warning: Any Runtime.Object content referenced within the StoryState will
        // be re-referenced rather than cloned. This is generally okay though since
        // Runtime.Objects are treated as immutable after they've been set up.
        // (e.g. we don't edit a Runtime.Text after it's been created an added.)
        // I wonder if there's a sensible way to enforce that..??
        public StoryState Copy()
        {
            var copy = new StoryState();

            copy.outputStream.AddRange(_outputStream);
            copy.currentChoices.AddRange(currentChoices);

            if (hasError) {
                copy.currentErrors = new List<string> ();
                copy.currentErrors.AddRange (currentErrors); 
            }

            copy.callStack = new CallStack (callStack);

            copy.variablesState = new VariablesState (copy.callStack);
            copy.variablesState.CopyFrom (variablesState);

            copy.evaluationStack.AddRange (evaluationStack);

            if( divertedPath != null )
                copy.divertedPath = new Path (divertedPath.componentsString);

            if( previousPath != null )
                copy.previousPath = new Path (previousPath.componentsString);

            copy.visitCounts = new Dictionary<string, int> (visitCounts);
            copy.turnIndices = new Dictionary<string, int> (turnIndices);
            copy.currentTurnIndex = currentTurnIndex;
            copy.storySeed = storySeed;

            copy.didSafeExit = didSafeExit;

            return copy;
        }

        public void ResetErrors()
        {
            currentErrors = null;
        }
            
        public void ResetOutput()
        {
            _outputStream.Clear ();
        }
            
        public void PushToOutputStream(Runtime.Object obj)
        {
            // Glue: absorbs newlines both before and after it,
            // causing two piece of inline text to stay on the same line.
            bool outputStreamEndsInGlue = false;
            int glueIdx = -1;
            if (_outputStream.Count > 0) {
                outputStreamEndsInGlue = _outputStream[_outputStream.Count-1] is Glue;
                glueIdx = _outputStream.Count - 1;
            }

            if (obj is Text) {
                var text = (Text)obj;

                bool canAppendNewline = !outputStreamEndsInNewline && !outputStreamEndsInGlue && _outputStream.Count != 0;

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
                        _outputStream.Add(new Text ("\n"));
                    }

                    // Remove newlines from end
                    lengthBeforeTrim = trimmedText.Length;
                    trimmedText = text.text.TrimEnd ('\n');

                    // Anything left or was it just pure newlines?
                    if (trimmedText.Length > 0) {

                        // Add main text to output stream
                        _outputStream.Add(new Text (trimmedText));

                        // Add single trailing newline if necessary
                        if (trimmedText.Length != lengthBeforeTrim) {
                            _outputStream.Add(new Text ("\n"));
                        }
                    }

                    return;
                }

            } 

            // New glue: remove any existing trailing newline from output stream
            else if (obj is Glue) {
                TrimNewlinesFromOutputStreamEnd ();
            }

            // Only remove an existing glue if we're definitely now
            // adding something new on top, since it's served its purpose.
            if( outputStreamEndsInGlue ) 
                _outputStream.RemoveAt (glueIdx);

            _outputStream.Add(obj);
        }

        // Called when Glue is appended
        // Cut through ' ' and '\t' to reach newlines
        //  - When earliest newline from the end is found, trim all 
        //    whitespace from the end up to and including that newline.
        //  - If no newline is found, don't remove anything
        // Bear in mind that text may be split across multiple text objects.
        // e.g.:
        //   "hello world  \n  \n    "
        //                 ^ trim from here to the end
        void TrimNewlinesFromOutputStreamEnd()
        {
            bool foundNonWhitespace = false;
            int lastNewlineObjIdx = -1;
            int lastNewlineCharIdx = -1;

            // Find last newline
            for (int i = _outputStream.Count - 1; i >= 0; --i) {

                var outputObj = _outputStream [i];
                if( outputObj is Text ) {

                    var text = (Text)outputObj;

                    for(int ci = text.text.Length-1; ci>=0; --ci) {
                        var c = text.text [ci];
                        if (c == ' ' || c == '\t') {
                            continue;
                        } 

                        else if (c == '\n') {
                            lastNewlineObjIdx = i;
                            lastNewlineCharIdx = ci;
                        }

                        // Non-whitespace
                        else {
                            foundNonWhitespace = true;
                            break;
                        }
                    }

                    if (foundNonWhitespace) {
                        break;
                    }

                }

                // Non-text
                else {
                    break;
                }
            }

            if (lastNewlineObjIdx >= 0) {
                int firstEntireObjToRemove = lastNewlineObjIdx + 1;
                if (lastNewlineCharIdx == 0) {
                    firstEntireObjToRemove = lastNewlineObjIdx;
                }

                int entireObjCountToRemove = _outputStream.Count - firstEntireObjToRemove;
                if (entireObjCountToRemove > 0) {
                    _outputStream.RemoveRange (firstEntireObjToRemove, entireObjCountToRemove);
                }

                if (lastNewlineCharIdx > 0) {
                    Text textToTrim = (Text)_outputStream [lastNewlineObjIdx];
                    textToTrim.text = textToTrim.text.Substring (0, lastNewlineCharIdx);
                }
            }


        }

        public bool outputStreamEndsInNewline {
            get {
                if (_outputStream.Count > 0) {
                    var text = _outputStream[_outputStream.Count-1] as Text;
                    if (text) {
                        return text.text == "\n";
                    }
                }

                return false;
            }
        }

        public bool inStringEvaluation {
            get {
                for (int i = _outputStream.Count - 1; i >= 0; i--) {
                    var cmd = _outputStream [i] as ControlCommand;
                    if (cmd && cmd.commandType == ControlCommand.CommandType.BeginString) {
                        return true;
                    }
                }

                return false;
            }
        }

        public void PushEvaluationStack(Runtime.Object obj)
        {
            evaluationStack.Add(obj);
        }

        public Runtime.Object PopEvaluationStack()
        {
            var obj = evaluationStack [evaluationStack.Count - 1];
            evaluationStack.RemoveAt (evaluationStack.Count - 1);
            return obj;
        }

        public Runtime.Object PeekEvaluationStack()
        {
            return evaluationStack [evaluationStack.Count - 1];
        }

        public List<Runtime.Object> PopEvaluationStack(int numberOfObjects)
        {
            if(numberOfObjects > evaluationStack.Count) {
                throw new System.Exception ("trying to pop too many objects");
            }

            var popped = evaluationStack.GetRange (evaluationStack.Count - numberOfObjects, numberOfObjects);
            evaluationStack.RemoveRange (evaluationStack.Count - numberOfObjects, numberOfObjects);
            return popped;
        }


        public void ForceEndFlow()
        {
            currentPath = null;

            while (callStack.canPopThread)
                callStack.PopThread ();

            while (callStack.canPop)
                callStack.Pop ();

            didSafeExit = true;
        }

        public void SetChosenPath(Path path)
        {
            // Changing direction, assume we need to clear current set of choices
            currentChoices.Clear ();

            previousPath = currentPath;

            currentPath = path;

            currentTurnIndex++;
        }

        public void AddError(string message)
        {
            // TODO: Could just add to output?
            if (currentErrors == null) {
                currentErrors = new List<string> ();
            }

            currentErrors.Add (message);
        }

        // REMEMBER! REMEMBER! REMEMBER!
        // When adding state, update the Copy method
        // REMEMBER! REMEMBER! REMEMBER!
            
        List<Runtime.Object> _outputStream;
    }
}

