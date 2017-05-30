using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Ink.Runtime
{
    /// <summary>
    /// All story state information is included in the StoryState class,
    /// including global variables, read counts, the pointer to the current
    /// point in the story, the call stack (for tunnels, functions, etc),
    /// and a few other smaller bits and pieces. You can save the current
    /// state using the json serialisation functions ToJson and LoadJson.
    /// </summary>
    public class StoryState
    {
        /// <summary>
        /// The current version of the state save file JSON-based format.
        /// </summary>
        public const int kInkSaveStateVersion = 6;
        const int kMinCompatibleLoadVersion = 6;

        /// <summary>
        /// Exports the current state to json format, in order to save the game.
        /// </summary>
        /// <returns>The save state in json format.</returns>
        public string ToJson() {
            return SimpleJson.DictionaryToText (jsonToken);
        }

        /// <summary>
        /// Loads a previously saved state in JSON format.
        /// </summary>
        /// <param name="json">The JSON string to load.</param>
        public void LoadJson(string json)
        {
            jsonToken = SimpleJson.TextToDictionary (json);
        }

        /// <summary>
        /// Gets the visit/read count of a particular Container at the given path.
        /// For a knot or stitch, that path string will be in the form:
        /// 
        ///     knot
        ///     knot.stitch
        /// 
        /// </summary>
        /// <returns>The number of times the specific knot or stitch has
        /// been enountered by the ink engine.</returns>
        /// <param name="pathString">The dot-separated path string of
        /// the specific knot or stitch.</param>
        public int VisitCountAtPathString(string pathString)
        {
            int visitCountOut;
            if (visitCounts.TryGetValue (pathString, out visitCountOut))
                return visitCountOut;

            return 0;
        }

        // REMEMBER! REMEMBER! REMEMBER!
        // When adding state, update the Copy method, and serialisation.
        // REMEMBER! REMEMBER! REMEMBER!

        internal List<Runtime.Object> outputStream { get { return _outputStream; } }
		internal List<Choice> currentChoices { 
			get { 
				// If we can continue generating text content rather than choices,
				// then we reflect the choice list as being empty, since choices
				// should always come at the end.
				if( canContinue ) return new List<Choice>();
				return _currentChoices;
			} 
		}
		internal List<Choice> generatedChoices {
			get {
				return _currentChoices;
			}
		}
        internal List<string> currentErrors { get; private set; }
        internal VariablesState variablesState { get; private set; }
        internal CallStack callStack { get; set; }
        internal List<Runtime.Object> evaluationStack { get; private set; }
        internal Runtime.Object divertedTargetObject { get; set; }
        internal Dictionary<string, int> visitCounts { get; private set; }
        internal Dictionary<string, int> turnIndices { get; private set; }
        internal int currentTurnIndex { get; private set; }
        internal int storySeed { get; set; }
        internal int previousRandom { get; set; }
        internal bool didSafeExit { get; set; }

        internal Story story { get; set; }

        internal Path currentPath { 
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

        internal Runtime.Object currentContentObject {
            get {
                return callStack.currentElement.currentObject;
            }
            set {
                callStack.currentElement.currentObject = value;
            }
        }

        internal Container currentContainer {
            get {
                return callStack.currentElement.currentContainer;
            }
        }

        internal Runtime.Object previousContentObject { 
            get {
                return callStack.currentThread.previousContentObject;
            }
            set {
                callStack.currentThread.previousContentObject = value;
            }
        }

		internal bool canContinue {
			get {
				return currentContentObject != null && !hasError;
			}
		}
            
        internal bool hasError
        {
            get {
                return currentErrors != null && currentErrors.Count > 0;
            }
        }

        internal string currentText
        {
            get 
            {
				if( _outputStreamTextDirty ) {
					var sb = new StringBuilder ();

					foreach (var outputObj in _outputStream) {
						var textContent = outputObj as StringValue;
						if (textContent != null) {
							sb.Append(textContent.value);
						}
					}

					_currentText = sb.ToString ();

					_outputStreamTextDirty = false;
				}

				return _currentText;
            }
        }
		string _currentText;

        internal List<string> currentTags 
        {
            get 
            {
				if( _outputStreamTagsDirty ) {
					_currentTags = new List<string>();

					foreach (var outputObj in _outputStream) {
						var tag = outputObj as Tag;
						if (tag != null) {
							_currentTags.Add (tag.text);
						}
					}

					_outputStreamTagsDirty = false;
				}

				return _currentTags;
            }
        }
		List<string> _currentTags;

        internal bool inExpressionEvaluation {
            get {
                return callStack.currentElement.inExpressionEvaluation;
            }
            set {
                callStack.currentElement.inExpressionEvaluation = value;
            }
        }
            
        internal StoryState (Story story)
        {
            this.story = story;

            _outputStream = new List<Runtime.Object> ();
			OutputStreamDirty();

            evaluationStack = new List<Runtime.Object> ();

            callStack = new CallStack (story.rootContentContainer);
            variablesState = new VariablesState (callStack, story.listDefinitions);

            visitCounts = new Dictionary<string, int> ();
            turnIndices = new Dictionary<string, int> ();
            currentTurnIndex = -1;

            // Seed the shuffle random numbers
            int timeSeed = DateTime.Now.Millisecond;
            storySeed = (new Random (timeSeed)).Next () % 100;
            previousRandom = 0;

			_currentChoices = new List<Choice> ();

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
        internal StoryState Copy()
        {
            var copy = new StoryState(story);

            copy.outputStream.AddRange(_outputStream);
			OutputStreamDirty();

			copy._currentChoices.AddRange(_currentChoices);

            if (hasError) {
                copy.currentErrors = new List<string> ();
                copy.currentErrors.AddRange (currentErrors); 
            }

            copy.callStack = new CallStack (callStack);

            copy.variablesState = new VariablesState (copy.callStack, story.listDefinitions);
            copy.variablesState.CopyFrom (variablesState);

            copy.evaluationStack.AddRange (evaluationStack);

            if (divertedTargetObject != null)
                copy.divertedTargetObject = divertedTargetObject;

            copy.previousContentObject = previousContentObject;

            copy.visitCounts = new Dictionary<string, int> (visitCounts);
            copy.turnIndices = new Dictionary<string, int> (turnIndices);
            copy.currentTurnIndex = currentTurnIndex;
            copy.storySeed = storySeed;
            copy.previousRandom = previousRandom;

            copy.didSafeExit = didSafeExit;

            return copy;
        }
            
        /// <summary>
        /// Object representation of full JSON state. Usually you should use
        /// LoadJson and ToJson since they serialise directly to string for you.
        /// But it may be useful to get the object representation so that you
        /// can integrate it into your own serialisation system.
        /// </summary>
        public Dictionary<string, object> jsonToken
        {
            get {
				
				var obj = new Dictionary<string, object> ();

				Dictionary<string, object> choiceThreads = null;
				foreach (Choice c in _currentChoices) {
                    c.originalChoicePath = c.choicePoint.path.componentsString;
                    c.originalThreadIndex = c.threadAtGeneration.threadIndex;

					if( callStack.ThreadWithIndex(c.originalThreadIndex) == null ) {
						if( choiceThreads == null )
							choiceThreads = new Dictionary<string, object> ();

						choiceThreads[c.originalThreadIndex.ToString()] = c.threadAtGeneration.jsonToken;
					}
                }
				if( choiceThreads != null )
					obj["choiceThreads"] = choiceThreads;

                
                obj ["callstackThreads"] = callStack.GetJsonToken();
                obj ["variablesState"] = variablesState.jsonToken;

                obj ["evalStack"] = Json.ListToJArray (evaluationStack);

                obj ["outputStream"] = Json.ListToJArray (_outputStream);

				obj ["currentChoices"] = Json.ListToJArray (_currentChoices);

                if( divertedTargetObject != null )
                    obj ["currentDivertTarget"] = divertedTargetObject.path.componentsString;

                obj ["visitCounts"] = Json.IntDictionaryToJObject (visitCounts);
                obj ["turnIndices"] = Json.IntDictionaryToJObject (turnIndices);
                obj ["turnIdx"] = currentTurnIndex;
                obj ["storySeed"] = storySeed;
                obj ["previousRandom"] = previousRandom;

                obj ["inkSaveVersion"] = kInkSaveStateVersion;

                // Not using this right now, but could do in future.
                obj ["inkFormatVersion"] = Story.inkVersionCurrent;

                return obj;
            }
            set {

                var jObject = value;

				object jSaveVersion = null;
				if (!jObject.TryGetValue("inkSaveVersion", out jSaveVersion)) {
                    throw new StoryException ("ink save format incorrect, can't load.");
                }
                else if ((int)jSaveVersion < kMinCompatibleLoadVersion) {
                    throw new StoryException("Ink save format isn't compatible with the current version (saw '"+jSaveVersion+"', but minimum is "+kMinCompatibleLoadVersion+"), so can't load.");
                }

                callStack.SetJsonToken ((Dictionary < string, object > )jObject ["callstackThreads"], story);
                variablesState.jsonToken = (Dictionary < string, object> )jObject["variablesState"];

                evaluationStack = Json.JArrayToRuntimeObjList ((List<object>)jObject ["evalStack"]);

                _outputStream = Json.JArrayToRuntimeObjList ((List<object>)jObject ["outputStream"]);
				OutputStreamDirty();

				_currentChoices = Json.JArrayToRuntimeObjList<Choice>((List<object>)jObject ["currentChoices"]);

				object currentDivertTargetPath;
				if (jObject.TryGetValue("currentDivertTarget", out currentDivertTargetPath)) {
                    var divertPath = new Path (currentDivertTargetPath.ToString ());
                    divertedTargetObject = story.ContentAtPath (divertPath);
                }
                    
                visitCounts = Json.JObjectToIntDictionary ((Dictionary<string, object>)jObject ["visitCounts"]);
                turnIndices = Json.JObjectToIntDictionary ((Dictionary<string, object>)jObject ["turnIndices"]);
                currentTurnIndex = (int)jObject ["turnIdx"];
                storySeed = (int)jObject ["storySeed"];
                previousRandom = (int)jObject ["previousRandom"];

				object jChoiceThreadsObj = null;
				jObject.TryGetValue("choiceThreads", out jChoiceThreadsObj);
				var jChoiceThreads = (Dictionary<string, object>)jChoiceThreadsObj;

				foreach (var c in _currentChoices) {
					c.choicePoint = (ChoicePoint) story.ContentAtPath (new Path (c.originalChoicePath));

					var foundActiveThread = callStack.ThreadWithIndex(c.originalThreadIndex);
					if( foundActiveThread != null ) {
						c.threadAtGeneration = foundActiveThread;
					} else {
						var jSavedChoiceThread = (Dictionary <string, object>) jChoiceThreads[c.originalThreadIndex.ToString()];
						c.threadAtGeneration = new CallStack.Thread(jSavedChoiceThread, story);
					}
				}

            }
        }
            
        internal void ResetErrors()
        {
            currentErrors = null;
        }
            
        internal void ResetOutput()
        {
            _outputStream.Clear ();
			OutputStreamDirty();
        }

        // Push to output stream, but split out newlines in text for consistency
        // in dealing with them later.
        internal void PushToOutputStream(Runtime.Object obj)
        {
            var text = obj as StringValue;
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

			OutputStreamDirty();
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
        List<Runtime.StringValue> TrySplittingHeadTailWhitespace(Runtime.StringValue single)
        {
            string str = single.value;

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
                
            var listTexts = new List<Runtime.StringValue> ();
            int innerStrStart = 0;
            int innerStrEnd = str.Length;

            if (headFirstNewlineIdx != -1) {
                if (headFirstNewlineIdx > 0) {
                    var leadingSpaces = new StringValue (str.Substring (0, headFirstNewlineIdx));
                    listTexts.Add(leadingSpaces);
                }
                listTexts.Add (new StringValue ("\n"));
                innerStrStart = headLastNewlineIdx + 1;
            }

            if (tailLastNewlineIdx != -1) {
                innerStrEnd = tailFirstNewlineIdx;
            }

            if (innerStrEnd > innerStrStart) {
                var innerStrText = str.Substring (innerStrStart, innerStrEnd - innerStrStart);
                listTexts.Add (new StringValue (innerStrText));
            }

            if (tailLastNewlineIdx != -1 && tailFirstNewlineIdx > headLastNewlineIdx) {
                listTexts.Add (new StringValue ("\n"));
                if (tailLastNewlineIdx < str.Length - 1) {
                    int numSpaces = (str.Length - tailLastNewlineIdx) - 1;
                    var trailingSpaces = new StringValue (str.Substring (tailLastNewlineIdx + 1, numSpaces));
                    listTexts.Add(trailingSpaces);
                }
            }

            return listTexts;
        }

        void PushToOutputStreamIndividual(Runtime.Object obj)
        {
            var glue = obj as Runtime.Glue;
            var text = obj as Runtime.StringValue;

            bool includeInOutput = true;

            if (glue) {

                // Found matching left-glue for right-glue? Close it.
                Glue matchingRightGlue = null;
                if (glue.isLeft)
                    matchingRightGlue = MatchRightGlueForLeftGlue (glue);

                // Left/Right glue is auto-generated for inline expressions 
                // where we want to absorb newlines but only in a certain direction.
                // "Bi" glue is written by the user in their ink with <>
                if (glue.isLeft || glue.isBi) {
                    TrimNewlinesFromOutputStream(matchingRightGlue);
                }

                includeInOutput = glue.isBi || glue.isRight;
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
                    }
                } else if (text.isNewline) {
                    if (outputStreamEndsInNewline || !outputStreamContainsContent)
                        includeInOutput = false;
                }
            }

            if (includeInOutput) {
                _outputStream.Add (obj);
            }

			OutputStreamDirty();
        }

        void TrimNewlinesFromOutputStream(Glue rightGlueToStopAt)
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
                var txt = obj as StringValue;
                var glue = obj as Glue;

                if (cmd || (txt && txt.isNonWhitespace)) {
                    foundNonWhitespace = true;
                    if( rightGlueToStopAt == null )
                        break;
                } else if (rightGlueToStopAt && glue == rightGlueToStopAt) {
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
                    var text = _outputStream [i] as StringValue;
                    if (text) {
                        _outputStream.RemoveAt (i);
                    } else {
                        i++;
                    }
                }
            }

            // Remove the glue (it will come before the whitespace,
            // so index is still valid)
            // Also remove any other non-matching right glues that come after,
            // since they'll have lost their matching glues already
            if (rightGlueToStopAt && rightGluePos > -1) {
                i = rightGluePos;
                while(i < _outputStream.Count) {
                    if (_outputStream [i] is Glue && ((Glue)_outputStream [i]).isRight) {
                        _outputStream.RemoveAt (i);
                    } else {
                        i++;
                    }
                }
            }

			OutputStreamDirty();
        }

        void TrimFromExistingGlue()
        {
            int i = currentGlueIndex;
            while (i < _outputStream.Count) {
                var txt = _outputStream [i] as StringValue;
                if (txt && !txt.isNonWhitespace)
                    _outputStream.RemoveAt (i);
                else
                    i++;
            }

			OutputStreamDirty();
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

			OutputStreamDirty();
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

        Runtime.Glue MatchRightGlueForLeftGlue (Glue leftGlue)
        {
            if (!leftGlue.isLeft) return null;

            for (int i = _outputStream.Count - 1; i >= 0; i--) {
                var c = _outputStream [i];
                var g = c as Glue;
                if (g && g.isRight && g.parent == leftGlue.parent) {
                    return g;
                } else if (c is ControlCommand) // e.g. BeginString
                    break;
            }

            return null;
        }
            
        internal bool outputStreamEndsInNewline {
            get {
                if (_outputStream.Count > 0) {

                    for (int i = _outputStream.Count - 1; i >= 0; i--) {
                        var obj = _outputStream [i];
                        if (obj is ControlCommand) // e.g. BeginString
                            break;
                        var text = _outputStream [i] as StringValue;
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

        internal bool outputStreamContainsContent {
            get {
                foreach (var content in _outputStream) {
                    if (content is StringValue)
                        return true;
                }
                return false;
            }
        }

        internal bool inStringEvaluation {
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

        internal void PushEvaluationStack(Runtime.Object obj)
        {
            // Include metadata about the origin List for list values when
            // they're used, so that lower level functions can make use
            // of the origin list to get related items, or make comparisons
            // with the integer values etc.
            var listValue = obj as ListValue;
            if (listValue) {
                
                // Update origin when list is has something to indicate the list origin
                var rawList = listValue.value;
                var names = rawList.originNames;
                if (names != null) {
                    var origins = new List<ListDefinition> ();
                    foreach (var n in names) {
                        ListDefinition def = null;
                        story.listDefinitions.TryGetDefinition (n, out def);
                        if( !origins.Contains(def) )
                            origins.Add (def);
                    }
                        
                    rawList.origins = origins;
                }
            }

            evaluationStack.Add(obj);
        }

        internal Runtime.Object PopEvaluationStack()
        {
            var obj = evaluationStack [evaluationStack.Count - 1];
            evaluationStack.RemoveAt (evaluationStack.Count - 1);
            return obj;
        }

        internal Runtime.Object PeekEvaluationStack()
        {
            return evaluationStack [evaluationStack.Count - 1];
        }

        internal List<Runtime.Object> PopEvaluationStack(int numberOfObjects)
        {
            if(numberOfObjects > evaluationStack.Count) {
                throw new System.Exception ("trying to pop too many objects");
            }

            var popped = evaluationStack.GetRange (evaluationStack.Count - numberOfObjects, numberOfObjects);
            evaluationStack.RemoveRange (evaluationStack.Count - numberOfObjects, numberOfObjects);
            return popped;
        }

        /// <summary>
        /// Ends the current ink flow, unwrapping the callstack but without
        /// affecting any variables. Useful if the ink is (say) in the middle
        /// a nested tunnel, and you want it to reset so that you can divert
        /// elsewhere using ChoosePathString(). Otherwise, after finishing
        /// the content you diverted to, it would continue where it left off.
        /// Calling this is equivalent to calling -> END in ink.
        /// </summary>
        public void ForceEnd()
        {
            while (callStack.canPopThread)
                callStack.PopThread ();

            while (callStack.canPop)
                callStack.Pop ();

			_currentChoices.Clear();

            currentContentObject = null;
            previousContentObject = null;

            didSafeExit = true;
        }

        // Don't make public since the method need to be wrapped in Story for visit counting
        internal void SetChosenPath(Path path)
        {
            // Changing direction, assume we need to clear current set of choices
			_currentChoices.Clear ();

            currentPath = path;

            currentTurnIndex++;
        }

        internal void StartExternalFunctionEvaluation (Container funcContainer, params object[] arguments)
        {
            // We'll start a new callstack, so keep hold of the original,
            // as well as the evaluation stack so we know if the function 
            // returned something
            _originalCallstack = callStack;
            _originalEvaluationStackHeight = evaluationStack.Count;

            // Create a new base call stack element.
            callStack = new CallStack (funcContainer);
            callStack.currentElement.type = PushPopType.Function;

            // Change the callstack the variableState is looking at to be
            // this temporary function evaluation one. We'll restore it afterwards
            variablesState.callStack = callStack;

            // By setting ourselves in external function evaluation mode,
            // we're saying it's okay to end the flow without a Done or End,
            // but with a ~ return instead.
            _isExternalFunctionEvaluation = true;

            PassArgumentsToEvaluationStack (arguments);
        }

        internal void PassArgumentsToEvaluationStack (params object [] arguments)
        {

            // Pass arguments onto the evaluation stack
            if (arguments != null) {
                for (int i = 0; i < arguments.Length; i++) {
                    if (!(arguments [i] is int || arguments [i] is float || arguments [i] is string)) {
                        throw new System.ArgumentException ("ink arguments when calling EvaluateFunction / ChoosePathStringWithParameters must be int, float or string");
                    }

                    PushEvaluationStack (Runtime.Value.Create (arguments [i]));
                }
            }
        }
            
        internal bool TryExitExternalFunctionEvaluation ()
        {
            if (_isExternalFunctionEvaluation && callStack.elements.Count == 1 && callStack.currentElement.type == PushPopType.Function) {
                currentContentObject = null;
                didSafeExit = true;
                return true;
            }

            return false;
        }

        internal object CompleteExternalFunctionEvaluation ()
        {
            
            // Do we have a returned value?
            // Potentially pop multiple values off the stack, in case we need
            // to clean up after ourselves (e.g. caller of EvaluateFunction may 
            // have passed too many arguments, and we currently have no way to check for that)
            Runtime.Object returnedObj = null;
            while (evaluationStack.Count > _originalEvaluationStackHeight) {
                var poppedObj = PopEvaluationStack ();
                if (returnedObj == null)
                    returnedObj = poppedObj;
            }

            // Restore our own state
            callStack = _originalCallstack;
            _originalCallstack = null;
            _originalEvaluationStackHeight = 0;

            // Restore the callstack that the variablesState uses
            variablesState.callStack = callStack;

            // What did we get back?
            if (returnedObj) {
                if (returnedObj is Runtime.Void)
                    return null;

                // Some kind of value, if not void
                var returnVal = returnedObj as Runtime.Value;

                // DivertTargets get returned as the string of components
                // (rather than a Path, which isn't public)
                if (returnVal.valueType == ValueType.DivertTarget) {
                    return returnVal.valueObject.ToString ();
                }

                // Other types can just have their exact object type:
                // int, float, string. VariablePointers get returned as strings.
                return returnVal.valueObject;
            }

            return null;
        }

        internal void AddError(string message)
        {
            // TODO: Could just add to output?
            if (currentErrors == null) {
                currentErrors = new List<string> ();
            }

            currentErrors.Add (message);
        }

		void OutputStreamDirty()
		{
			_outputStreamTextDirty = true;
			_outputStreamTagsDirty = true;
		}

        // REMEMBER! REMEMBER! REMEMBER!
        // When adding state, update the Copy method and serialisation
        // REMEMBER! REMEMBER! REMEMBER!
            
        List<Runtime.Object> _outputStream;
		bool _outputStreamTextDirty = true;
		bool _outputStreamTagsDirty = true;

		List<Choice> _currentChoices;

        // Temporary state only, during externally called function evaluation
        bool _isExternalFunctionEvaluation;
        CallStack _originalCallstack;
        int _originalEvaluationStackHeight;
    }
}

