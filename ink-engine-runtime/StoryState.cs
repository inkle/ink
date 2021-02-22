using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

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
        public const int kInkSaveStateVersion = 9; // new: multi-flows, but backward compatible
        const int kMinCompatibleLoadVersion = 8;

        /// <summary>
        /// Callback for when a state is loaded
        /// </summary>
        public event Action onDidLoadState;

        /// <summary>
        /// Exports the current state to json format, in order to save the game.
        /// </summary>
        /// <returns>The save state in json format.</returns>
        public string ToJson() {
            var writer = new SimpleJson.Writer();
            WriteJson(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Exports the current state to json format, in order to save the game.
        /// For this overload you can pass in a custom stream, such as a FileStream.
        /// </summary>
        public void ToJson(Stream stream) {
            var writer = new SimpleJson.Writer(stream);
            WriteJson(writer);
        }

        /// <summary>
        /// Loads a previously saved state in JSON format.
        /// </summary>
        /// <param name="json">The JSON string to load.</param>
        public void LoadJson(string json)
        {
            var jObject = SimpleJson.TextToDictionary (json);
            LoadJsonObj(jObject);
            if(onDidLoadState != null) onDidLoadState();
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

            if ( _patch != null ) {
                var container = story.ContentAtPath(new Path(pathString)).container;
                if (container == null)
                    throw new Exception("Content at path not found: " + pathString);

                if( _patch.TryGetVisitCount(container, out visitCountOut) )
                    return visitCountOut;
            }

            if (_visitCounts.TryGetValue(pathString, out visitCountOut))
                return visitCountOut;

            return 0;
        }

        public int VisitCountForContainer(Container container)
        {
            if (!container.visitsShouldBeCounted)
            {
                story.Error("Read count for target (" + container.name + " - on " + container.debugMetadata + ") unknown.");
                return 0;
            }

            int count = 0;
            if (_patch != null && _patch.TryGetVisitCount(container, out count))
                return count;
                
            var containerPathStr = container.path.ToString();
            _visitCounts.TryGetValue(containerPathStr, out count);
            return count;
        }

        public void IncrementVisitCountForContainer(Container container)
        {
            if( _patch != null ) {
                var currCount = VisitCountForContainer(container);
                currCount++;
                _patch.SetVisitCount(container, currCount);
                return;
            }

            int count = 0;
            var containerPathStr = container.path.ToString();
            _visitCounts.TryGetValue(containerPathStr, out count);
            count++;
            _visitCounts[containerPathStr] = count;
        }

        public void RecordTurnIndexVisitToContainer(Container container)
        {
            if( _patch != null ) {
                _patch.SetTurnIndex(container, currentTurnIndex);
                return;
            }

            var containerPathStr = container.path.ToString();
            _turnIndices[containerPathStr] = currentTurnIndex;
        }

        public int TurnsSinceForContainer(Container container)
        {
            if (!container.turnIndexShouldBeCounted)
            {
                story.Error("TURNS_SINCE() for target (" + container.name + " - on " + container.debugMetadata + ") unknown.");
            }

            int index = 0;

            if ( _patch != null && _patch.TryGetTurnIndex(container, out index) ) {
                return currentTurnIndex - index;
            }

            var containerPathStr = container.path.ToString();
            if (_turnIndices.TryGetValue(containerPathStr, out index))
            {
                return currentTurnIndex - index;
            }
            else
            {
                return -1;
            }
        }

        public int callstackDepth {
			get {
				return callStack.depth;
			}
		}

        // REMEMBER! REMEMBER! REMEMBER!
        // When adding state, update the Copy method, and serialisation.
        // REMEMBER! REMEMBER! REMEMBER!

        public List<Runtime.Object> outputStream { 
            get { 
                return _currentFlow.outputStream; 
            } 
        }

        

		public List<Choice> currentChoices { 
			get { 
				// If we can continue generating text content rather than choices,
				// then we reflect the choice list as being empty, since choices
				// should always come at the end.
				if( canContinue ) return new List<Choice>();
				return _currentFlow.currentChoices;
			} 
		}
		public List<Choice> generatedChoices {
			get {
				return _currentFlow.currentChoices;
			}
		}

        // TODO: Consider removing currentErrors / currentWarnings altogether
        // and relying on client error handler code immediately handling StoryExceptions etc
        // Or is there a specific reason we need to collect potentially multiple
        // errors before throwing/exiting?
        public List<string> currentErrors { get; private set; }
        public List<string> currentWarnings { get; private set; }
        public VariablesState variablesState { get; private set; }
        public CallStack callStack { 
            get { 
                return _currentFlow.callStack;
            }
            // set {
            //     _currentFlow.callStack = value;
            // } 
        }

        public List<Runtime.Object> evaluationStack { get; private set; }
        public Pointer divertedPointer { get; set; }

        public int currentTurnIndex { get; private set; }
        public int storySeed { get; set; }
        public int previousRandom { get; set; }
        public bool didSafeExit { get; set; }

        public Story story { get; set; }

        /// <summary>
        /// String representation of the location where the story currently is.
        /// </summary>
        public string currentPathString {
            get {
                var pointer = currentPointer;
                if (pointer.isNull)
                    return null;
                else
                    return pointer.path.ToString();
            }
        }

        public Runtime.Pointer currentPointer {
            get {
                return callStack.currentElement.currentPointer;
            }
            set {
                callStack.currentElement.currentPointer = value;
            }
        }

        public Pointer previousPointer { 
            get {
                return callStack.currentThread.previousPointer;
            }
            set {
                callStack.currentThread.previousPointer = value;
            }
        }

		public bool canContinue {
			get {
				return !currentPointer.isNull && !hasError;
			}
		}
            
        public bool hasError
        {
            get {
                return currentErrors != null && currentErrors.Count > 0;
            }
        }

        public bool hasWarning {
            get {
                return currentWarnings != null && currentWarnings.Count > 0;
            }
        }

        public string currentText
        {
            get 
            {
				if( _outputStreamTextDirty ) {
					var sb = new StringBuilder ();

					foreach (var outputObj in outputStream) {
						var textContent = outputObj as StringValue;
						if (textContent != null) {
							sb.Append(textContent.value);
						}
					}

                    _currentText = CleanOutputWhitespace (sb.ToString ());

					_outputStreamTextDirty = false;
				}

				return _currentText;
            }
        }
		string _currentText;

        // Cleans inline whitespace in the following way:
        //  - Removes all whitespace from the start and end of line (including just before a \n)
        //  - Turns all consecutive space and tab runs into single spaces (HTML style)
        string CleanOutputWhitespace(string str)
        {
            var sb = new StringBuilder(str.Length);

            int currentWhitespaceStart = -1;
            int startOfLine = 0;

            for (int i = 0; i < str.Length; i++) {
                var c = str[i];

                bool isInlineWhitespace = c == ' ' || c == '\t';

                if (isInlineWhitespace && currentWhitespaceStart == -1)
                    currentWhitespaceStart = i;

                if (!isInlineWhitespace) {
                    if (c != '\n' && currentWhitespaceStart > 0 && currentWhitespaceStart != startOfLine) {
                        sb.Append(' ');
                    }
                    currentWhitespaceStart = -1;
                }

                if (c == '\n')
                    startOfLine = i + 1;

                if (!isInlineWhitespace)
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public List<string> currentTags 
        {
            get 
            {
				if( _outputStreamTagsDirty ) {
					_currentTags = new List<string>();

					foreach (var outputObj in outputStream) {
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

        public string currentFlowName {
            get {
                return _currentFlow.name;
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

            _currentFlow = new Flow(kDefaultFlowName, story);
            
			OutputStreamDirty();

            evaluationStack = new List<Runtime.Object> ();

            variablesState = new VariablesState (callStack, story.listDefinitions);

            _visitCounts = new Dictionary<string, int> ();
            _turnIndices = new Dictionary<string, int> ();

            currentTurnIndex = -1;

            // Seed the shuffle random numbers
            int timeSeed = DateTime.Now.Millisecond;
            storySeed = (new Random (timeSeed)).Next () % 100;
            previousRandom = 0;

			

            GoToStart();
        }

        public void GoToStart()
        {
            callStack.currentElement.currentPointer = Pointer.StartOf (story.mainContentContainer);
        }

        internal void SwitchFlow_Internal(string flowName)
        {
            if(flowName == null) throw new System.Exception("Must pass a non-null string to Story.SwitchFlow");
            
            if( _namedFlows == null ) {
                _namedFlows = new Dictionary<string, Flow>();
                _namedFlows[kDefaultFlowName] = _currentFlow;
            }

            if( flowName == _currentFlow.name ) {
                return;
            }

            Flow flow;
            if( !_namedFlows.TryGetValue(flowName, out flow) ) {
                flow = new Flow(flowName, story);
                _namedFlows[flowName] = flow;
            }

            _currentFlow = flow;
            variablesState.callStack = _currentFlow.callStack;

            // Cause text to be regenerated from output stream if necessary
            OutputStreamDirty();
        }

        internal void SwitchToDefaultFlow_Internal()
        {
            if( _namedFlows == null ) return;
            SwitchFlow_Internal(kDefaultFlowName);
        }

        internal void RemoveFlow_Internal(string flowName)
        {
            if(flowName == null) throw new System.Exception("Must pass a non-null string to Story.DestroyFlow");
            if(flowName == kDefaultFlowName) throw new System.Exception("Cannot destroy default flow");

            // If we're currently in the flow that's being removed, switch back to default
            if( _currentFlow.name == flowName ) {
                SwitchToDefaultFlow_Internal();
            }

            _namedFlows.Remove(flowName);
        }

        // Warning: Any Runtime.Object content referenced within the StoryState will
        // be re-referenced rather than cloned. This is generally okay though since
        // Runtime.Objects are treated as immutable after they've been set up.
        // (e.g. we don't edit a Runtime.StringValue after it's been created an added.)
        // I wonder if there's a sensible way to enforce that..??
        public StoryState CopyAndStartPatching()
        {
            var copy = new StoryState(story);

            copy._patch = new StatePatch(_patch);

            // Hijack the new default flow to become a copy of our current one
            // If the patch is applied, then this new flow will replace the old one in _namedFlows
            copy._currentFlow.name = _currentFlow.name;
            copy._currentFlow.callStack = new CallStack (_currentFlow.callStack);
            copy._currentFlow.currentChoices.AddRange(_currentFlow.currentChoices);
            copy._currentFlow.outputStream.AddRange(_currentFlow.outputStream);
            copy.OutputStreamDirty();

            // The copy of the state has its own copy of the named flows dictionary,
            // except with the current flow replaced with the copy above
            // (Assuming we're in multi-flow mode at all. If we're not then
            // the above copy is simply the default flow copy and we're done)
            if( _namedFlows != null ) {
                copy._namedFlows = new Dictionary<string, Flow>();
                foreach(var namedFlow in _namedFlows)
                    copy._namedFlows[namedFlow.Key] = namedFlow.Value;
                copy._namedFlows[_currentFlow.name] = copy._currentFlow;
            }

            if (hasError) {
                copy.currentErrors = new List<string> ();
                copy.currentErrors.AddRange (currentErrors); 
            }
            if (hasWarning) {
                copy.currentWarnings = new List<string> ();
                copy.currentWarnings.AddRange (currentWarnings); 
            }

            
            // ref copy - exactly the same variables state!
            // we're expecting not to read it only while in patch mode
            // (though the callstack will be modified)
            copy.variablesState = variablesState;
            copy.variablesState.callStack = copy.callStack;
            copy.variablesState.patch = copy._patch;

            copy.evaluationStack.AddRange (evaluationStack);

            if (!divertedPointer.isNull)
                copy.divertedPointer = divertedPointer;

            copy.previousPointer = previousPointer;

            // visit counts and turn indicies will be read only, not modified
            // while in patch mode
            copy._visitCounts = _visitCounts;
            copy._turnIndices = _turnIndices;

            copy.currentTurnIndex = currentTurnIndex;
            copy.storySeed = storySeed;
            copy.previousRandom = previousRandom;

            copy.didSafeExit = didSafeExit;

            return copy;
        }

        public void RestoreAfterPatch()
        {
            // VariablesState was being borrowed by the patched
            // state, so restore it with our own callstack.
            // _patch will be null normally, but if you're in the
            // middle of a save, it may contain a _patch for save purpsoes.
            variablesState.callStack = callStack;
            variablesState.patch = _patch; // usually null
        }

        public void ApplyAnyPatch()
        {
            if (_patch == null) return;

            variablesState.ApplyPatch();

            foreach(var pathToCount in _patch.visitCounts)
                ApplyCountChanges(pathToCount.Key, pathToCount.Value, isVisit:true);

            foreach (var pathToIndex in _patch.turnIndices)
                ApplyCountChanges(pathToIndex.Key, pathToIndex.Value, isVisit:false);

            _patch = null;
        }

        void ApplyCountChanges(Container container, int newCount, bool isVisit)
        {
            var counts = isVisit ? _visitCounts : _turnIndices;
            counts[container.path.ToString()] = newCount;
        }

        void WriteJson(SimpleJson.Writer writer)
        {
            writer.WriteObjectStart();

            // Flows
            writer.WritePropertyStart("flows");
            writer.WriteObjectStart();

            // Multi-flow
            if( _namedFlows != null ) {
                foreach(var namedFlow in _namedFlows) {
                    writer.WriteProperty(namedFlow.Key, namedFlow.Value.WriteJson);
                }
            } 
            
            // Single flow
            else {
                writer.WriteProperty(_currentFlow.name, _currentFlow.WriteJson);
            }

            writer.WriteObjectEnd();
            writer.WritePropertyEnd(); // end of flows

            writer.WriteProperty("currentFlowName", _currentFlow.name);

            writer.WriteProperty("variablesState", variablesState.WriteJson);

            writer.WriteProperty("evalStack", w => Json.WriteListRuntimeObjs(w, evaluationStack));


            if (!divertedPointer.isNull)
                writer.WriteProperty("currentDivertTarget", divertedPointer.path.componentsString);
                
            writer.WriteProperty("visitCounts", w => Json.WriteIntDictionary(w, _visitCounts));
            writer.WriteProperty("turnIndices", w => Json.WriteIntDictionary(w, _turnIndices));


            writer.WriteProperty("turnIdx", currentTurnIndex);
            writer.WriteProperty("storySeed", storySeed);
            writer.WriteProperty("previousRandom", previousRandom);

            writer.WriteProperty("inkSaveVersion", kInkSaveStateVersion);

            // Not using this right now, but could do in future.
            writer.WriteProperty("inkFormatVersion", Story.inkVersionCurrent);

            writer.WriteObjectEnd();
        }


        void LoadJsonObj(Dictionary<string, object> jObject)
        {
			object jSaveVersion = null;
			if (!jObject.TryGetValue("inkSaveVersion", out jSaveVersion)) {
                throw new Exception ("ink save format incorrect, can't load.");
            }
            else if ((int)jSaveVersion < kMinCompatibleLoadVersion) {
                throw new Exception("Ink save format isn't compatible with the current version (saw '"+jSaveVersion+"', but minimum is "+kMinCompatibleLoadVersion+"), so can't load.");
            }

            // Flows: Always exists in latest format (even if there's just one default)
            // but this dictionary doesn't exist in prev format
            object flowsObj = null;
            if (jObject.TryGetValue("flows", out flowsObj)) {
                var flowsObjDict = (Dictionary<string, object>)flowsObj;
                
                // Single default flow
                if( flowsObjDict.Count == 1 )
                    _namedFlows = null;

                // Multi-flow, need to create flows dict
                else if( _namedFlows == null )
                    _namedFlows = new Dictionary<string, Flow>();

                // Multi-flow, already have a flows dict
                else
                    _namedFlows.Clear();

                // Load up each flow (there may only be one)
                foreach(var namedFlowObj in flowsObjDict) {
                    var name = namedFlowObj.Key;
                    var flowObj = (Dictionary<string, object>)namedFlowObj.Value;

                    // Load up this flow using JSON data
                    var flow = new Flow(name, story, flowObj);

                    if( flowsObjDict.Count == 1 ) {
                        _currentFlow = new Flow(name, story, flowObj);
                    } else {
                        _namedFlows[name] = flow;
                    }
                }

                if( _namedFlows != null && _namedFlows.Count > 1 ) {
                    var currFlowName = (string)jObject["currentFlowName"];
                    _currentFlow = _namedFlows[currFlowName];
                }
            }

            // Old format: individually load up callstack, output stream, choices in current/default flow
            else {
                _namedFlows = null;
                _currentFlow.name = kDefaultFlowName;
                _currentFlow.callStack.SetJsonToken ((Dictionary < string, object > )jObject ["callstackThreads"], story);
                _currentFlow.outputStream = Json.JArrayToRuntimeObjList ((List<object>)jObject ["outputStream"]);
                _currentFlow.currentChoices = Json.JArrayToRuntimeObjList<Choice>((List<object>)jObject ["currentChoices"]);

                object jChoiceThreadsObj = null;
                jObject.TryGetValue("choiceThreads", out jChoiceThreadsObj);
                _currentFlow.LoadFlowChoiceThreads((Dictionary<string, object>)jChoiceThreadsObj, story);
            }

            OutputStreamDirty();

            variablesState.SetJsonToken((Dictionary < string, object> )jObject["variablesState"]);
            variablesState.callStack = _currentFlow.callStack;

            evaluationStack = Json.JArrayToRuntimeObjList ((List<object>)jObject ["evalStack"]);


			object currentDivertTargetPath;
			if (jObject.TryGetValue("currentDivertTarget", out currentDivertTargetPath)) {
                var divertPath = new Path (currentDivertTargetPath.ToString ());
                divertedPointer = story.PointerAtPath (divertPath);
            }
                
            _visitCounts = Json.JObjectToIntDictionary((Dictionary<string, object>)jObject["visitCounts"]);
            _turnIndices = Json.JObjectToIntDictionary((Dictionary<string, object>)jObject["turnIndices"]);

            currentTurnIndex = (int)jObject ["turnIdx"];
            storySeed = (int)jObject ["storySeed"];

            // Not optional, but bug in inkjs means it's actually missing in inkjs saves
            object previousRandomObj = null;
            if( jObject.TryGetValue("previousRandom", out previousRandomObj) ) {
                previousRandom = (int)previousRandomObj;
            } else {
                previousRandom = 0;
            }
        }
            
        public void ResetErrors()
        {
            currentErrors = null;
            currentWarnings = null;
        }
            
        public void ResetOutput(List<Runtime.Object> objs = null)
        {
            outputStream.Clear ();
            if( objs != null ) outputStream.AddRange (objs);
			OutputStreamDirty();
        }

        // Push to output stream, but split out newlines in text for consistency
        // in dealing with them later.
        public void PushToOutputStream(Runtime.Object obj)
        {
            var text = obj as StringValue;
            if (text) {
                var listText = TrySplittingHeadTailWhitespace (text);
                if (listText != null) {
                    foreach (var textObj in listText) {
                        PushToOutputStreamIndividual (textObj);
                    }
                    OutputStreamDirty();
                    return;
                }
            }

            PushToOutputStreamIndividual (obj);

			OutputStreamDirty();
        }

        public void PopFromOutputStream (int count)
        {
            outputStream.RemoveRange (outputStream.Count - count, count);
            OutputStreamDirty ();
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
        //  - A newline on its own is returned in a list for consistency.
        List<Runtime.StringValue> TrySplittingHeadTailWhitespace(Runtime.StringValue single)
        {
            string str = single.value;

            int headFirstNewlineIdx = -1;
            int headLastNewlineIdx = -1;
            for (int i = 0; i < str.Length; i++) {
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
            for (int i = str.Length-1; i >= 0; i--) {
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

            // New glue, so chomp away any whitespace from the end of the stream
            if (glue) {
                TrimNewlinesFromOutputStream();
                includeInOutput = true;
            }

            // New text: do we really want to append it, if it's whitespace?
            // Two different reasons for whitespace to be thrown away:
            //   - Function start/end trimming
            //   - User defined glue: <>
            // We also need to know when to stop trimming, when there's non-whitespace.
            else if( text ) {

                // Where does the current function call begin?
                var functionTrimIndex = -1;
                var currEl = callStack.currentElement;
                if (currEl.type == PushPopType.Function) {
                    functionTrimIndex = currEl.functionStartInOuputStream;
                }

                // Do 2 things:
                //  - Find latest glue
                //  - Check whether we're in the middle of string evaluation
                // If we're in string eval within the current function, we
                // don't want to trim back further than the length of the current string.
                int glueTrimIndex = -1;
                for (int i = outputStream.Count - 1; i >= 0; i--) {
                    var o = outputStream [i];
                    var c = o as ControlCommand;
                    var g = o as Glue;

                    // Find latest glue
                    if (g) {
                        glueTrimIndex = i;
                        break;
                    } 

                    // Don't function-trim past the start of a string evaluation section
                    else if (c && c.commandType == ControlCommand.CommandType.BeginString) {
                        if (i >= functionTrimIndex) {
                            functionTrimIndex = -1;
                        }
                        break;
                    }
                }

                // Where is the most agressive (earliest) trim point?
                var trimIndex = -1;
                if (glueTrimIndex != -1 && functionTrimIndex != -1)
                    trimIndex = Math.Min (functionTrimIndex, glueTrimIndex);
                else if (glueTrimIndex != -1)
                    trimIndex = glueTrimIndex;
                else
                    trimIndex = functionTrimIndex;

                // So, are we trimming then?
                if (trimIndex != -1) {

                    // While trimming, we want to throw all newlines away,
                    // whether due to glue or the start of a function
                    if (text.isNewline) {
                        includeInOutput = false;
                    } 

                    // Able to completely reset when normal text is pushed
                    else if (text.isNonWhitespace) {

                        if( glueTrimIndex > -1 )
                            RemoveExistingGlue ();

                        // Tell all functions in callstack that we have seen proper text,
                        // so trimming whitespace at the start is done.
                        if (functionTrimIndex > -1) {
                            var callstackElements = callStack.elements;
                            for (int i = callstackElements.Count - 1; i >= 0; i--) {
                                var el = callstackElements [i];
                                if (el.type == PushPopType.Function) {
                                    el.functionStartInOuputStream = -1;
                                } else {
                                    break;
                                }
                            }
                        }
                    }
                } 

                // De-duplicate newlines, and don't ever lead with a newline
                else if (text.isNewline) {
                    if (outputStreamEndsInNewline || !outputStreamContainsContent)
                        includeInOutput = false;
                }
            }

            if (includeInOutput) {
                outputStream.Add (obj);
                OutputStreamDirty();
            }
        }

        void TrimNewlinesFromOutputStream()
        {
            int removeWhitespaceFrom = -1;

            // Work back from the end, and try to find the point where
            // we need to start removing content.
            //  - Simply work backwards to find the first newline in a string of whitespace
            // e.g. This is the content   \n   \n\n
            //                            ^---------^ whitespace to remove
            //                        ^--- first while loop stops here
            int i = outputStream.Count-1;
            while (i >= 0) {
                var obj = outputStream [i];
                var cmd = obj as ControlCommand;
                var txt = obj as StringValue;

                if (cmd || (txt && txt.isNonWhitespace)) {
                    break;
                } 
                else if (txt && txt.isNewline) {
                    removeWhitespaceFrom = i;
                }
                i--;
            }

            // Remove the whitespace
            if (removeWhitespaceFrom >= 0) {
                i=removeWhitespaceFrom;
                while(i < outputStream.Count) {
                    var text = outputStream [i] as StringValue;
                    if (text) {
                        outputStream.RemoveAt (i);
                    } else {
                        i++;
                    }
                }
            }

			OutputStreamDirty();
        }

        // Only called when non-whitespace is appended
        void RemoveExistingGlue()
        {
            for (int i = outputStream.Count - 1; i >= 0; i--) {
                var c = outputStream [i];
                if (c is Glue) {
                    outputStream.RemoveAt (i);
                } else if( c is ControlCommand ) { // e.g. BeginString
                    break;
                }
            }

			OutputStreamDirty();
        }

        public bool outputStreamEndsInNewline {
            get {
                if (outputStream.Count > 0) {

                    for (int i = outputStream.Count - 1; i >= 0; i--) {
                        var obj = outputStream [i];
                        if (obj is ControlCommand) // e.g. BeginString
                            break;
                        var text = outputStream [i] as StringValue;
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
                foreach (var content in outputStream) {
                    if (content is StringValue)
                        return true;
                }
                return false;
            }
        }

        public bool inStringEvaluation {
            get {
                for (int i = outputStream.Count - 1; i >= 0; i--) {
                    var cmd = outputStream [i] as ControlCommand;
                    if (cmd && cmd.commandType == ControlCommand.CommandType.BeginString) {
                        return true;
                    }
                }

                return false;
            }
        }

        public void PushEvaluationStack(Runtime.Object obj)
        {
            // Include metadata about the origin List for list values when
            // they're used, so that lower level functions can make use
            // of the origin list to get related items, or make comparisons
            // with the integer values etc.
            var listValue = obj as ListValue;
            if (listValue) {
                
                // Update origin when list is has something to indicate the list origin
                var rawList = listValue.value;
				if (rawList.originNames != null) {
					if( rawList.origins == null ) rawList.origins = new List<ListDefinition>();
					rawList.origins.Clear();

					foreach (var n in rawList.originNames) {
                        ListDefinition def = null;
                        story.listDefinitions.TryListGetDefinition (n, out def);
						if( !rawList.origins.Contains(def) )
							rawList.origins.Add (def);
                    }
                }
            }

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
            callStack.Reset();

			_currentFlow.currentChoices.Clear();

            currentPointer = Pointer.Null;
            previousPointer = Pointer.Null;

            didSafeExit = true;
        }

        // Add the end of a function call, trim any whitespace from the end.
        // We always trim the start and end of the text that a function produces.
        // The start whitespace is discard as it is generated, and the end
        // whitespace is trimmed in one go here when we pop the function.
        void TrimWhitespaceFromFunctionEnd ()
        {
            Debug.Assert (callStack.currentElement.type == PushPopType.Function);

            var functionStartPoint = callStack.currentElement.functionStartInOuputStream;

            // If the start point has become -1, it means that some non-whitespace
            // text has been pushed, so it's safe to go as far back as we're able.
            if (functionStartPoint == -1) {
                functionStartPoint = 0;
            }

            // Trim whitespace from END of function call
            for (int i = outputStream.Count - 1; i >= functionStartPoint; i--) {
                var obj = outputStream [i];
                var txt = obj as StringValue;
                var cmd = obj as ControlCommand;
                if (!txt) continue;
                if (cmd) break;

                if (txt.isNewline || txt.isInlineWhitespace) {
                    outputStream.RemoveAt (i);
                    OutputStreamDirty ();
                } else {
                    break;
                }
            }
        }

        public void PopCallstack (PushPopType? popType = null)
        {
            // Add the end of a function call, trim any whitespace from the end.
            if (callStack.currentElement.type == PushPopType.Function)
                TrimWhitespaceFromFunctionEnd ();

            callStack.Pop (popType);
        }

        // Don't make public since the method need to be wrapped in Story for visit counting
        public void SetChosenPath(Path path, bool incrementingTurnIndex)
        {
            // Changing direction, assume we need to clear current set of choices
			_currentFlow.currentChoices.Clear ();

            var newPointer = story.PointerAtPath (path);
            if (!newPointer.isNull && newPointer.index == -1)
                newPointer.index = 0;

            currentPointer = newPointer;

            if( incrementingTurnIndex )
                currentTurnIndex++;
        }

        public void StartFunctionEvaluationFromGame (Container funcContainer, params object[] arguments)
        {
            callStack.Push (PushPopType.FunctionEvaluationFromGame, evaluationStack.Count);
            callStack.currentElement.currentPointer = Pointer.StartOf (funcContainer);

            PassArgumentsToEvaluationStack (arguments);
        }

        public void PassArgumentsToEvaluationStack (params object [] arguments)
        {
            // Pass arguments onto the evaluation stack
            if (arguments != null) {
                for (int i = 0; i < arguments.Length; i++) {
                    if (!(arguments [i] is int || arguments [i] is float || arguments [i] is string || arguments [i] is InkList)) {
                        throw new System.ArgumentException ("ink arguments when calling EvaluateFunction / ChoosePathStringWithParameters must be int, float, string or InkList. Argument was "+(arguments [i] == null ? "null" : arguments [i].GetType().Name));
                    }

                    PushEvaluationStack (Runtime.Value.Create (arguments [i]));
                }
            }
        }
            
        public bool TryExitFunctionEvaluationFromGame ()
        {
            if( callStack.currentElement.type == PushPopType.FunctionEvaluationFromGame ) {
                currentPointer = Pointer.Null;
                didSafeExit = true;
                return true;
            }

            return false;
        }

        public object CompleteFunctionEvaluationFromGame ()
        {
            if (callStack.currentElement.type != PushPopType.FunctionEvaluationFromGame) {
                throw new Exception ("Expected external function evaluation to be complete. Stack trace: "+callStack.callStackTrace);
            }

            int originalEvaluationStackHeight = callStack.currentElement.evaluationStackHeightWhenPushed;
            
            // Do we have a returned value?
            // Potentially pop multiple values off the stack, in case we need
            // to clean up after ourselves (e.g. caller of EvaluateFunction may 
            // have passed too many arguments, and we currently have no way to check for that)
            Runtime.Object returnedObj = null;
            while (evaluationStack.Count > originalEvaluationStackHeight) {
                var poppedObj = PopEvaluationStack ();
                if (returnedObj == null)
                    returnedObj = poppedObj;
            }

            // Finally, pop the external function evaluation
            PopCallstack (PushPopType.FunctionEvaluationFromGame);

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

        public void AddError(string message, bool isWarning)
        {
            if (!isWarning) {
                if (currentErrors == null) currentErrors = new List<string> ();
                currentErrors.Add (message);
            } else {
                if (currentWarnings == null) currentWarnings = new List<string> ();
                currentWarnings.Add (message);
            }
        }

		void OutputStreamDirty()
		{
			_outputStreamTextDirty = true;
			_outputStreamTagsDirty = true;
		}

        // REMEMBER! REMEMBER! REMEMBER!
        // When adding state, update the Copy method and serialisation
        // REMEMBER! REMEMBER! REMEMBER!


        Dictionary<string, int> _visitCounts;
        Dictionary<string, int> _turnIndices;
		bool _outputStreamTextDirty = true;
		bool _outputStreamTagsDirty = true;

        StatePatch _patch;

        Flow _currentFlow;
        Dictionary<string, Flow> _namedFlows;
        const string kDefaultFlowName = "DEFAULT_FLOW";
    }
}

