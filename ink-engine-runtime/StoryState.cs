using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public Runtime.Object divertedTargetObject { get; set; }
        public Dictionary<string, int> visitCounts { get; private set; }
        public Dictionary<string, int> turnIndices { get; private set; }
        public int currentTurnIndex { get; private set; }
        public int storySeed { get; private set; }
        public bool didSafeExit { get; set; }

        public Story story { get; set; }

        public Path currentPath { 
            get { 
                if (currentContentObject == null)
                    return null;

                return currentContentObject.path;
            } 
            set {
                if (value != null)
                    currentContentObject = story.ContentAtPath (value);
                else
                    currentContentObject = null;
            }
        }

        public Runtime.Object currentContentObject {
            
            get {
                return callStack.currentElement.currentObject;
            }
            set {
                callStack.currentElement.currentObject = value;
            }
        }

        public Container currentContainer {
            get {
                return callStack.currentElement.currentContainer;
            }
        }
            
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



        public StoryState (Story story)
        {
            this.story = story;

            _outputStream = new List<Runtime.Object> ();

            evaluationStack = new List<Runtime.Object> ();

            callStack = new CallStack (story.rootContentContainer);
            variablesState = new VariablesState (callStack);

            visitCounts = new Dictionary<string, int> ();
            turnIndices = new Dictionary<string, int> ();
            currentTurnIndex = -1;

            // Seed the shuffle random numbers
            int timeSeed = DateTime.Now.Millisecond;
            storySeed = (new Random (timeSeed)).Next () % 100;

            currentChoices = new List<ChoiceInstance> ();

            GoToStart();
        }

        internal void GoToStart()
        {
            callStack.currentElement.currentContainer = story.mainContentContainer;
            callStack.currentElement.currentContentIndex = 0;
        }

        // Warning: Any Runtime.Object content referenced within the StoryState will
        // be re-referenced rather than cloned. This is generally okay though since
        // Runtime.Objects are treated as immutable after they've been set up.
        // (e.g. we don't edit a Runtime.Text after it's been created an added.)
        // I wonder if there's a sensible way to enforce that..??
        public StoryState Copy()
        {
            var copy = new StoryState(story);

            copy.outputStream.AddRange(_outputStream);
            copy.currentChoices.AddRange(currentChoices);

            if (hasError) {
                copy.currentErrors = new List<string> ();
                copy.currentErrors.AddRange (currentErrors); 
            }

            copy.callStack = new CallStack (callStack);

            copy._currentRightGlue = _currentRightGlue;

            copy.variablesState = new VariablesState (copy.callStack);
            copy.variablesState.CopyFrom (variablesState);

            copy.evaluationStack.AddRange (evaluationStack);

            if (divertedTargetObject != null)
                copy.divertedTargetObject = divertedTargetObject;

            copy.visitCounts = new Dictionary<string, int> (visitCounts);
            copy.turnIndices = new Dictionary<string, int> (turnIndices);
            copy.currentTurnIndex = currentTurnIndex;
            copy.storySeed = storySeed;

            copy.didSafeExit = didSafeExit;

            return copy;
        }

        public JToken jsonToken
        {
            get {

                var obj = new JObject ();
                obj ["callstackThreads"] = callStack.GetJsonToken();
                obj ["variablesState"] = variablesState.jsonToken;

                // TODO: Can we skip the evaluation stack, since theoretically,
                // it's only ever used temporarily?
                obj ["evalStack"] = Json.ListToJArray (evaluationStack);

                obj ["outputStream"] = Json.ListToJArray (_outputStream);

                return obj;
            }
            set {
                callStack.SetJsonToken (value ["callstackThreads"], story);
                variablesState.jsonToken = value["variablesState"];

                // TODO: Can we skip the evaluation stack, since theoretically,
                // it's only ever used temporarily?
                evaluationStack = Json.JArrayToRuntimeObjList ((JArray)value ["evalStack"]);

                _outputStream = Json.JArrayToRuntimeObjList ((JArray)value ["outputStream"]);
            }
        }

        public void ResetErrors()
        {
            currentErrors = null;
        }
            
        public void ResetOutput()
        {
            _outputStream.Clear ();
        }

        // Push to output stream, but split out newlines in text for consistency
        // in dealing with them later.
        public void PushToOutputStream(Runtime.Object obj)
        {
            var text = obj as Text;
            if (text) {
                var listText = TrySplittingHeadTailWhitespace (text);
                if (listText != null) {
                    foreach (var textObj in listText) {
                        PushToOutputStreamIndividual (textObj);
                    }
                    return;
                }
            }

            PushToOutputStreamIndividual (obj);
        }

        // At both the start and the end of the string, split out the new lines like so:
        //
        //  "   \n  \n     \n  the string \n is awesome \n     \n     "
        //      ^-----------^                           ^-------^
        // 
        // Excess newlines are converted into single newlines, and spaces discarded.
        // Outside spaces are significant and retained. "Interior" newlines within 
        // the main string are ignored, since this is for the purpose of gluing only.
        //
        //  - If no splitting is necessary, null is returned.
        //  - A newline on its own is returned in an list for consistency.
        List<Runtime.Text> TrySplittingHeadTailWhitespace(Runtime.Text single)
        {
            string str = single.text;

            int headFirstNewlineIdx = -1;
            int headLastNewlineIdx = -1;
            for (int i = 0; i < str.Length; ++i) {
                char c = str [i];
                if (c == '\n') {
                    if (headFirstNewlineIdx == -1)
                        headFirstNewlineIdx = i;
                    headLastNewlineIdx = i;
                }
                else if (c == ' ' || c == '\t')
                    continue;
                else
                    break;
            }

            int tailLastNewlineIdx = -1;
            int tailFirstNewlineIdx = -1;
            for (int i = 0; i < str.Length; ++i) {
                char c = str [i];
                if (c == '\n') {
                    if (tailLastNewlineIdx == -1)
                        tailLastNewlineIdx = i;
                    tailFirstNewlineIdx = i;
                }
                else if (c == ' ' || c == '\t')
                    continue;
                else
                    break;
            }

            // No splitting to be done?
            if (headFirstNewlineIdx == -1 && tailLastNewlineIdx == -1)
                return null;
                
            var listTexts = new List<Runtime.Text> ();
            int innerStrStart = 0;
            int innerStrEnd = str.Length;

            if (headFirstNewlineIdx != -1) {
                if (headFirstNewlineIdx > 0) {
                    var leadingSpaces = new Text (str.Substring (0, headFirstNewlineIdx));
                    listTexts.Add(leadingSpaces);
                }
                listTexts.Add (new Text ("\n"));
                innerStrStart = headLastNewlineIdx + 1;
            }

            if (tailLastNewlineIdx != -1) {
                innerStrEnd = tailFirstNewlineIdx;
            }

            if (innerStrEnd > innerStrStart) {
                var innerStrText = str.Substring (innerStrStart, innerStrEnd - innerStrStart);
                listTexts.Add (new Text (innerStrText));
            }

            if (tailLastNewlineIdx != -1 && tailFirstNewlineIdx > headLastNewlineIdx) {
                listTexts.Add (new Text ("\n"));
                if (tailLastNewlineIdx < str.Length - 1) {
                    int numSpaces = (str.Length - tailLastNewlineIdx) - 1;
                    var trailingSpaces = new Text (str.Substring (tailLastNewlineIdx + 1, numSpaces));
                    listTexts.Add(trailingSpaces);
                }
            }

            return listTexts;
        }

        void PushToOutputStreamIndividual(Runtime.Object obj)
        {
            var glue = obj as Runtime.Glue;
            var text = obj as Runtime.Text;

            bool includeInOutput = true;

            if (glue) {
                
                // Found matching left-glue for right-glue? Close it.
                bool foundMatchingLeftGlue = glue.isLeft && _currentRightGlue && glue.parent == _currentRightGlue.parent;
                if (foundMatchingLeftGlue) {
                    _currentRightGlue = null;
                }

                // Left/Right glue is auto-generated for inline expressions 
                // where we want to absorb newlines but only in a certain direction.
                // "Bi" glue is written by the user in their ink with <>
                if (glue.isLeft || glue.isBi) {
                    TrimNewlinesFromOutputStream(stopAndRemoveRightGlue:foundMatchingLeftGlue);
                }

                // New right-glue
                bool isNewRightGlue = glue.isRight && _currentRightGlue == null;
                if (isNewRightGlue) {
                    _currentRightGlue = glue;
                }

                includeInOutput = glue.isBi || isNewRightGlue;
            }

            else if( text ) {

                if (currentGlueIndex != -1) {

                    // Absorb any new newlines if there's existing glue
                    // in the output stream.
                    // Also trim any extra whitespace (spaces/tabs) if so.
                    if (text.isNewline) {
                        TrimFromExistingGlue ();
                        includeInOutput = false;
                    } 

                    // Able to completely reset when 
                    else if (text.isNonWhitespace) {
                        RemoveExistingGlue ();
                        _currentRightGlue = null;
                    }
                } else if (text.isNewline) {
                    if (outputStreamEndsInNewline || !outputStreamContainsContent)
                        includeInOutput = false;
                }
            }

            if (includeInOutput) {
                _outputStream.Add (obj);
            }
        }

        void TrimNewlinesFromOutputStream(bool stopAndRemoveRightGlue)
        {
            int removeWhitespaceFrom = -1;
            int rightGluePos = -1;
            bool foundNonWhitespace = false;

            // Work back from the end, and try to find the point where
            // we need to start removing content. There are two ways:
            //  - Start from the matching right-glue (because we just saw a left-glue)
            //  - Simply work backwards to find the first newline in a string of whitespace
            int i = _outputStream.Count-1;
            while (i >= 0) {
                var obj = _outputStream [i];
                var cmd = obj as ControlCommand;
                var txt = obj as Text;
                var glue = obj as Glue;

                if (cmd || (txt && txt.isNonWhitespace)) {
                    foundNonWhitespace = true;
                    if( !stopAndRemoveRightGlue )
                        break;
                } else if (stopAndRemoveRightGlue && glue && glue.isRight) {
                    rightGluePos = i;
                    break;
                } else if (txt && txt.isNewline && !foundNonWhitespace) {
                    removeWhitespaceFrom = i;
                }
                i--;
            }

            // Remove the whitespace
            if (removeWhitespaceFrom >= 0) {
                i=removeWhitespaceFrom;
                while(i < _outputStream.Count) {
                    var text = _outputStream [i] as Text;
                    if (text) {
                        _outputStream.RemoveAt (i);
                    } else {
                        i++;
                    }
                }
            }

            // Remove the glue (it will come before the whitespace,
            // so index is still valid)
            if (stopAndRemoveRightGlue && rightGluePos > -1)
                _outputStream.RemoveAt (rightGluePos);
        }

        void TrimFromExistingGlue()
        {
            int i = currentGlueIndex;
            while (i < _outputStream.Count) {
                var txt = _outputStream [i] as Text;
                if (txt && !txt.isNonWhitespace)
                    _outputStream.RemoveAt (i);
                else
                    i++;
            }
        }


        // Only called when non-whitespace is appended
        void RemoveExistingGlue()
        {
            for (int i = _outputStream.Count - 1; i >= 0; i--) {
                var c = _outputStream [i];
                if (c is Glue) {
                    _outputStream.RemoveAt (i);
                } else if( c is ControlCommand ) { // e.g. BeginString
                    break;
                }
            }
        }

        int currentGlueIndex {
            get {
                for (int i = _outputStream.Count - 1; i >= 0; i--) {
                    var c = _outputStream [i];
                    var glue = c as Glue;
                    if (glue)
                        return i;
                    else if (c is ControlCommand) // e.g. BeginString
                        break;
                }
                return -1;
            }
        }

        public bool outputStreamEndsInNewline {
            get {
                if (_outputStream.Count > 0) {

                    for (int i = _outputStream.Count - 1; i >= 0; i--) {
                        var obj = _outputStream [i];
                        if (obj is ControlCommand) // e.g. BeginString
                            break;
                        var text = _outputStream [i] as Text;
                        if (text) {
                            if (text.isNewline)
                                return true;
                            else if (text.isNonWhitespace)
                                break;
                        }
                    }
                }

                return false;
            }
        }

        public bool outputStreamContainsContent {
            get {
                foreach (var content in _outputStream) {
                    if (content is Text)
                        return true;
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
            currentContentObject = null;

            while (callStack.canPopThread)
                callStack.PopThread ();

            while (callStack.canPop)
                callStack.Pop ();

            currentChoices.Clear();

            didSafeExit = true;
        }

        public void SetChosenPath(Path path)
        {
            // Changing direction, assume we need to clear current set of choices
            currentChoices.Clear ();

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
        Runtime.Glue _currentRightGlue;
    }
}

