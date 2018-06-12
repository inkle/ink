using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Ink.Runtime
{
    /// <summary>
    /// A Story is the core class that represents a complete Ink narrative, and
    /// manages the evaluation and state of it.
    /// </summary>
	public class Story : Runtime.Object
	{
        /// <summary>
        /// The current version of the ink story file format.
        /// </summary>
        public const int inkVersionCurrent = 18;

        // Version numbers are for engine itself and story file, rather
        // than the story state save format (which is um, currently nonexistant)
        //  -- old engine, new format: always fail
        //  -- new engine, old format: possibly cope, based on this number
        // When incrementing the version number above, the question you
        // should ask yourself is:
        //  -- Will the engine be able to load an old story file from 
        //     before I made these changes to the engine?
        //     If possible, you should support it, though it's not as
        //     critical as loading old save games, since it's an
        //     in-development problem only.

        /// <summary>
        /// The minimum legacy version of ink that can be loaded by the current version of the code.
        /// </summary>
        const int inkVersionMinimumCompatible = 18;

        /// <summary>
        /// The list of Choice objects available at the current point in
        /// the Story. This list will be populated as the Story is stepped
        /// through with the Continue() method. Once canContinue becomes
        /// false, this list will be populated, and is usually
        /// (but not always) on the final Continue() step.
        /// </summary>
        public List<Choice> currentChoices
		{
			get 
			{
                // Don't include invisible choices for external usage.
                var choices = new List<Choice>();
                foreach (var c in _state.currentChoices) {
                    if (!c.isInvisibleDefault) {
                        c.index = choices.Count;
                        choices.Add (c);
                    }
                }
                return choices;
			}
		}
            
        /// <summary>
        /// The latest line of text to be generated from a Continue() call.
        /// </summary>
		public string currentText { 
            get  { 
                IfAsyncWeCant ("call currentText since it's a work in progress");
                return state.currentText; 
            } 
        }

        /// <summary>
        /// Gets a list of tags as defined with '#' in source that were seen
        /// during the latest Continue() call.
        /// </summary>
        public List<string> currentTags { 
            get { 
                IfAsyncWeCant ("call currentTags since it's a work in progress");
                return state.currentTags; 
            } 
        }

        /// <summary>
        /// Any errors generated during evaluation of the Story.
        /// </summary>
        public List<string> currentErrors { get { return state.currentErrors; } }

        /// <summary>
        /// Any warnings generated during evaluation of the Story.
        /// </summary>
        public List<string> currentWarnings { get { return state.currentWarnings; } }

        /// <summary>
        /// Whether the currentErrors list contains any errors.
        /// </summary>
        public bool hasError { get { return state.hasError; } }

        /// <summary>
        /// Whether the currentWarnings list contains any warnings.
        /// </summary>
        public bool hasWarning { get { return state.hasWarning; } }

        /// <summary>
        /// The VariablesState object contains all the global variables in the story.
        /// However, note that there's more to the state of a Story than just the
        /// global variables. This is a convenience accessor to the full state object.
        /// </summary>
        public VariablesState variablesState{ get { return state.variablesState; } }

	    internal ListDefinitionsOrigin listDefinitions {
            get {
                return _listDefinitions;
            }
        }

        /// <summary>
        /// The entire current state of the story including (but not limited to):
        /// 
        ///  * Global variables
        ///  * Temporary variables
        ///  * Read/visit and turn counts
        ///  * The callstack and evaluation stacks
        ///  * The current threads
        /// 
        /// </summary>
        public StoryState state { get { return _state; } }

        /// <summary>
        /// Start recording ink profiling information during calls to Continue on Story.
        /// Return a Profiler instance that you can request a report from when you're finished.
        /// </summary>
		public Profiler StartProfiling() {
            IfAsyncWeCant ("start profiling");
			_profiler = new Profiler();
			return _profiler;
		}

        /// <summary>
        /// Stop recording ink profiling information during calls to Continue on Story.
        /// To generate a report from the profiler, call 
        /// </summary>
		public void EndProfiling() {
			_profiler = null;
		}

        // Warning: When creating a Story using this constructor, you need to
        // call ResetState on it before use. Intended for compiler use only.
        // For normal use, use the constructor that takes a json string.
	    internal Story (Container contentContainer, List<Runtime.ListDefinition> lists = null)
		{
			_mainContentContainer = contentContainer;

            if (lists != null)
                _listDefinitions = new ListDefinitionsOrigin (lists);

            _externals = new Dictionary<string, ExternalFunction> ();
		}

        /// <summary>
        /// Construct a Story object using a JSON string compiled through inklecate.
        /// </summary>
        public Story(string jsonString) : this((Container)null)
        {
            Dictionary<string, object> rootObject = SimpleJson.TextToDictionary (jsonString);

            object versionObj = rootObject ["inkVersion"];
            if (versionObj == null)
                throw new System.Exception ("ink version number not found. Are you sure it's a valid .ink.json file?");

            int formatFromFile = (int)versionObj;
            if (formatFromFile > inkVersionCurrent) {
                throw new System.Exception ("Version of ink used to build story was newer than the current verison of the engine");
            } else if (formatFromFile < inkVersionMinimumCompatible) {
                throw new System.Exception ("Version of ink used to build story is too old to be loaded by this verison of the engine");
            } else if (formatFromFile != inkVersionCurrent) {
                System.Diagnostics.Debug.WriteLine ("WARNING: Version of ink used to build story doesn't match current version of engine. Non-critical, but recommend synchronising.");
            }
                
            var rootToken = rootObject ["root"];
            if (rootToken == null)
                throw new System.Exception ("Root node for ink not found. Are you sure it's a valid .ink.json file?");

            object listDefsObj;
            if (rootObject.TryGetValue ("listDefs", out listDefsObj)) {
                _listDefinitions = Json.JTokenToListDefinitions (listDefsObj);
            }

            _mainContentContainer = Json.JTokenToRuntimeObject (rootToken) as Container;

            ResetState ();
        }

        /// <summary>
        /// The Story itself in JSON representation.
        /// </summary>
        public string ToJsonString()
        {
            var rootContainerJsonList = (List<object>) Json.RuntimeObjectToJToken (_mainContentContainer);

            var rootObject = new Dictionary<string, object> ();
            rootObject ["inkVersion"] = inkVersionCurrent;
            rootObject ["root"] = rootContainerJsonList;

            if (_listDefinitions != null)
                rootObject ["listDefs"] = Json.ListDefinitionsToJToken (_listDefinitions);

            return SimpleJson.DictionaryToText (rootObject);
        }
            
        /// <summary>
        /// Reset the Story back to its initial state as it was when it was
        /// first constructed.
        /// </summary>
        public void ResetState()
        {
            // TODO: Could make this possible
            IfAsyncWeCant ("ResetState");

            _state = new StoryState (this);
            _state.variablesState.variableChangedEvent += VariableStateDidChangeEvent;

            ResetGlobals ();
        }

        /// <summary>
        /// Reset the runtime error and warning list within the state.
        /// </summary>
        public void ResetErrors()
        {
            _state.ResetErrors ();
        }

        /// <summary>
        /// Unwinds the callstack. Useful to reset the Story's evaluation
        /// without actually changing any meaningful state, for example if
        /// you want to exit a section of story prematurely and tell it to
        /// go elsewhere with a call to ChoosePathString(...).
        /// Doing so without calling ResetCallstack() could cause unexpected
        /// issues if, for example, the Story was in a tunnel already.
        /// </summary>
        public void ResetCallstack()
        {
            IfAsyncWeCant ("ResetCallstack");

            _state.ForceEnd ();
        }

        void ResetGlobals()
        {
            if (_mainContentContainer.namedContent.ContainsKey ("global decl")) {
                var originalPointer = state.currentPointer;

                ChoosePath (new Path ("global decl"), incrementingTurnIndex: false);

                // Continue, but without validating external bindings,
                // since we may be doing this reset at initialisation time.
                ContinueInternal ();

                state.currentPointer = originalPointer;
            }

            state.variablesState.SnapshotDefaultGlobals ();
        }

        /// <summary>
        /// Continue the story for one line of content, if possible.
        /// If you're not sure if there's more content available, for example if you
        /// want to check whether you're at a choice point or at the end of the story,
        /// you should call <c>canContinue</c> before calling this function.
        /// </summary>
        /// <returns>The line of text content.</returns>
        public string Continue()
        {
            ContinueAsync(0);
            return currentText;
        }


        /// <summary>
        /// Check whether more content is available if you were to call <c>Continue()</c> - i.e.
        /// are we mid story rather than at a choice point or at the end.
        /// </summary>
        /// <value><c>true</c> if it's possible to call <c>Continue()</c>.</value>
        public bool canContinue {
        	get {
                return state.canContinue;
            }
        }

        /// <summary>
        /// If ContinueAsync was called (with milliseconds limit > 0) then this property
        /// will return false if the ink evaluation isn't yet finished, and you need to call 
        /// it again in order for the Continue to fully complete.
        /// </summary>
        public bool asyncContinueComplete {
            get {
                return !_asyncContinueActive;
            }
        }

        /// <summary>
        /// An "asnychronous" version of Continue that only partially evaluates the ink,
        /// with a budget of a certain time limit. It will exit ink evaluation early if
        /// the evaluation isn't complete within the time limit, with the
        /// asyncContinueComplete property being false.
        /// This is useful if ink evaluation takes a long time, and you want to distribute
        /// it over multiple game frames for smoother animation.
        /// If you pass a limit of zero, then it will fully evaluate the ink in the same
        /// way as calling Continue (and in fact, this exactly what Continue does internally).
        /// </summary>
        public void ContinueAsync (float millisecsLimitAsync)
        {
            if( !_hasValidatedExternals )
                ValidateExternalBindings ();

            ContinueInternal (millisecsLimitAsync);
        }

        void ContinueInternal (float millisecsLimitAsync = 0)
        {
            if( _profiler != null )
                _profiler.PreContinue();
            
            var isAsyncTimeLimited = millisecsLimitAsync > 0;

            _recursiveContinueCount++;

            // Doing either:
            //  - full run through non-async (so not active and don't want to be)
            //  - Starting async run-through
            if (!_asyncContinueActive) {
                _asyncContinueActive = isAsyncTimeLimited;
				
                if (!canContinue) {
                    throw new StoryException ("Can't continue - should check canContinue before calling Continue");
                }

                _state.didSafeExit = false;
                _state.ResetOutput ();

                // It's possible for ink to call game to call ink to call game etc
                // In this case, we only want to batch observe variable changes
                // for the outermost call.
                if (_recursiveContinueCount == 1)
                    _state.variablesState.batchObservingVariableChanges = true;
            }

            // Start timing
            var durationStopwatch = new Stopwatch ();
            durationStopwatch.Start ();

            bool outputStreamEndsInNewline = false;
            do {

                try {
                    outputStreamEndsInNewline = ContinueSingleStep ();
                } catch(StoryException e) {
                    AddError (e.Message, useEndLineNumber:e.useEndLineNumber);
                    break;
                }
                
                if (outputStreamEndsInNewline) 
                    break;

                // Run out of async time?
                if (_asyncContinueActive && durationStopwatch.ElapsedMilliseconds > millisecsLimitAsync) {
                    break;
                }

            } while(canContinue);

            durationStopwatch.Stop ();

            // 4 outcomes:
            //  - got newline (so finished this line of text)
            //  - can't continue (e.g. choices or ending)
            //  - ran out of time during evaluation
            //  - error
            //
            // Successfully finished evaluation in time (or in error)
            if (outputStreamEndsInNewline || !canContinue) {

                // Need to rewind, due to evaluating further than we should?
                if( _stateAtLastNewline != null ) {
    				RestoreStateSnapshot (_stateAtLastNewline);
                    _stateAtLastNewline = null;
                }

                // Finished a section of content / reached a choice point?
                if( !canContinue ) {
					if (state.callStack.canPopThread)
                        AddError ("Thread available to pop, threads should always be flat by the end of evaluation?");

                    if (state.generatedChoices.Count == 0 && !state.didSafeExit && _temporaryEvaluationContainer == null) {
                        if (state.callStack.CanPop (PushPopType.Tunnel))
                            AddError ("unexpectedly reached end of content. Do you need a '->->' to return from a tunnel?");
                        else if (state.callStack.CanPop (PushPopType.Function))
                            AddError ("unexpectedly reached end of content. Do you need a '~ return'?");
                        else if (!state.callStack.canPop)
                            AddError ("ran out of content. Do you need a '-> DONE' or '-> END'?");
                        else
                            AddError ("unexpectedly reached end of content for unknown reason. Please debug compiler!");
                    }
                }

                state.didSafeExit = false;

                if (_recursiveContinueCount == 1)
                    _state.variablesState.batchObservingVariableChanges = false;

                _asyncContinueActive = false;
            }

            _recursiveContinueCount--;

            if( _profiler != null )
                _profiler.PostContinue();
        }

        bool ContinueSingleStep ()
        {
            if (_profiler != null)
                _profiler.PreStep ();

            // Run main step function (walks through content)
            Step ();

            if (_profiler != null)
                _profiler.PostStep ();

            // Run out of content and we have a default invisible choice that we can follow?
			if (!canContinue && !state.callStack.elementIsEvaluateFromGame) {
                TryFollowDefaultInvisibleChoice ();
            }

            if (_profiler != null)
                _profiler.PreSnapshot ();

            // Don't save/rewind during string evaluation, which is e.g. used for choices
            if (!state.inStringEvaluation) {

                // We previously found a newline, but were we just double checking that
                // it wouldn't immediately be removed by glue?
                if (_stateAtLastNewline != null) {

                    // Has proper text or a tag been added? Then we know that the newline
                    // that was previously added is definitely the end of the line.
                    var change = CalculateNewlineOutputStateChange (
                        _stateAtLastNewline.currentText,       state.currentText, 
                        _stateAtLastNewline.currentTags.Count, state.currentTags.Count
                    );

                    // The last time we saw a newline, it was definitely the end of the line, so we
                    // want to rewind to that point.
                    if (change == OutputStateChange.ExtendedBeyondNewline) {
                        RestoreStateSnapshot (_stateAtLastNewline);

                        // Hit a newline for sure, we're done
                        return true;
                    } 

                    // Newline that previously existed is no longer valid - e.g.
                    // glue was encounted that caused it to be removed.
                    else if (change == OutputStateChange.NewlineRemoved) {
                        _stateAtLastNewline = null;
                    }
                }

                // Current content ends in a newline - approaching end of our evaluation
                if (state.outputStreamEndsInNewline) {

                    // If we can continue evaluation for a bit:
                    // Create a snapshot in case we need to rewind.
                    // We're going to continue stepping in case we see glue or some
                    // non-text content such as choices.
                    if (canContinue) {

                        // Don't bother to record the state beyond the current newline.
                        // e.g.:
                        // Hello world\n            // record state at the end of here
                        // ~ complexCalculation()   // don't actually need this unless it generates text
                        if (_stateAtLastNewline == null)
                            _stateAtLastNewline = StateSnapshot ();
                    }

                    // Can't continue, so we're about to exit - make sure we
                    // don't have an old state hanging around.
                    else {
                        _stateAtLastNewline = null;
                    }

                }

            }

            if (_profiler != null)
                _profiler.PostSnapshot ();

            // outputStreamEndsInNewline = false
            return false;
        }




        // Assumption: prevText is the snapshot where we saw a newline, and we're checking whether we're really done
        //             with that line. Therefore prevText will definitely end in a newline.
        //
        // We take tags into account too, so that a tag following a content line:
        //   Content
        //   # tag
        // ... doesn't cause the tag to be wrongly associated with the content above.
        enum OutputStateChange
        {
        	NoChange,
        	ExtendedBeyondNewline,
        	NewlineRemoved
        }
        OutputStateChange CalculateNewlineOutputStateChange (string prevText, string currText, int prevTagCount, int currTagCount)
        {
            // Simple case: nothing's changed, and we still have a newline
            // at the end of the current content
            var newlineStillExists = currText.Length >= prevText.Length && currText [prevText.Length - 1] == '\n';
            if (prevTagCount == currTagCount && prevText.Length == currText.Length 
                && newlineStillExists)
                return OutputStateChange.NoChange;

            // Old newline has been removed, it wasn't the end of the line after all
            if (!newlineStillExists) {
                return OutputStateChange.NewlineRemoved;
            }

            // Tag added - definitely the start of a new line
            if (currTagCount > prevTagCount)
                return OutputStateChange.ExtendedBeyondNewline;

            // There must be new content - check whether it's just whitespace
            for (int i = prevText.Length; i < currText.Length; i++) {
                var c = currText [i];
                if (c != ' ' && c != '\t') {
                    return OutputStateChange.ExtendedBeyondNewline;
                }
            }

            // There's new text but it's just spaces and tabs, so there's still the potential
            // for glue to kill the newline.
            return OutputStateChange.NoChange;
        }


        /// <summary>
        /// Continue the story until the next choice point or until it runs out of content.
        /// This is as opposed to the Continue() method which only evaluates one line of
        /// output at a time.
        /// </summary>
        /// <returns>The resulting text evaluated by the ink engine, concatenated together.</returns>
        public string ContinueMaximally()
        {
            IfAsyncWeCant ("ContinueMaximally");

            var sb = new StringBuilder ();

            while (canContinue) {
                sb.Append (Continue ());
            }

            return sb.ToString ();
        }

	    internal SearchResult ContentAtPath(Path path)
        {
            return mainContentContainer.ContentAtPath (path);
        }

	    internal Runtime.Container KnotContainerWithName (string name)
        {
            INamedContent namedContainer;
            if (mainContentContainer.namedContent.TryGetValue (name, out namedContainer))
                return namedContainer as Container;
            else
                return null;
        }

	    internal Pointer PointerAtPath (Path path)
        {
            if (path.length == 0)
                return Pointer.Null;

            var p = new Pointer ();

            int pathLengthToUse = path.length;

            SearchResult result;
            if( path.lastComponent.isIndex ) {
                pathLengthToUse = path.length - 1;
                result = mainContentContainer.ContentAtPath (path, partialPathLength:pathLengthToUse);
                p.container = result.container;
                p.index = path.lastComponent.index;
            } else {
                result = mainContentContainer.ContentAtPath (path);
                p.container = result.container;
                p.index = -1;
            }

            if (result.obj == null || result.obj == mainContentContainer && pathLengthToUse > 0)
                Error ("Failed to find content at path '" + path + "', and no approximation of it was possible.");
            else if (result.approximate)
                Warning ("Failed to find content at path '" + path + "', so it was approximated to: '"+result.obj.path+"'.");

            return p;
        }

        StoryState StateSnapshot()
        {
            return state.Copy ();
        }

        void RestoreStateSnapshot(StoryState state)
        {
            _state = state;
        }
            
        void Step ()
        {
            bool shouldAddToStream = true;

            // Get current content
            var pointer = state.currentPointer;
            if (pointer.isNull) {
                return;
            }

            // Step directly to the first element of content in a container (if necessary)
            Container containerToEnter = pointer.Resolve () as Container;
            while(containerToEnter) {

                // Mark container as being entered
                VisitContainer (containerToEnter, atStart:true);

                // No content? the most we can do is step past it
                if (containerToEnter.content.Count == 0)
                    break;


                pointer = Pointer.StartOf (containerToEnter);
                containerToEnter = pointer.Resolve() as Container;
            }
            state.currentPointer = pointer;

			if( _profiler != null ) {
				_profiler.Step(state.callStack);
			}

            // Is the current content object:
            //  - Normal content
            //  - Or a logic/flow statement - if so, do it
            // Stop flow if we hit a stack pop when we're unable to pop (e.g. return/done statement in knot
            // that was diverted to rather than called as a function)
            var currentContentObj = pointer.Resolve ();
            bool isLogicOrFlowControl = PerformLogicAndFlowControl (currentContentObj);

            // Has flow been forced to end by flow control above?
            if (state.currentPointer.isNull) {
                return;
            }

            if (isLogicOrFlowControl) {
                shouldAddToStream = false;
            }

            // Choice with condition?
            var choicePoint = currentContentObj as ChoicePoint;
            if (choicePoint) {
                var choice = ProcessChoice (choicePoint);
                if (choice) {
                    state.generatedChoices.Add (choice);
                }

                currentContentObj = null;
                shouldAddToStream = false;
            }

            // If the container has no content, then it will be
            // the "content" itself, but we skip over it.
            if (currentContentObj is Container) {
                shouldAddToStream = false;
            }

            // Content to add to evaluation stack or the output stream
            if (shouldAddToStream) {

                // If we're pushing a variable pointer onto the evaluation stack, ensure that it's specific
                // to our current (possibly temporary) context index. And make a copy of the pointer
                // so that we're not editing the original runtime object.
                var varPointer = currentContentObj as VariablePointerValue;
                if (varPointer && varPointer.contextIndex == -1) {

                    // Create new object so we're not overwriting the story's own data
                    var contextIdx = state.callStack.ContextForVariableNamed(varPointer.variableName);
                    currentContentObj = new VariablePointerValue (varPointer.variableName, contextIdx);
                }

                // Expression evaluation content
                if (state.inExpressionEvaluation) {
                    state.PushEvaluationStack (currentContentObj);
                }
                // Output stream content (i.e. not expression evaluation)
                else {
                    state.PushToOutputStream (currentContentObj);
                }
            }

            // Increment the content pointer, following diverts if necessary
            NextContent ();

            // Starting a thread should be done after the increment to the content pointer,
            // so that when returning from the thread, it returns to the content after this instruction.
            var controlCmd = currentContentObj as ControlCommand;
            if (controlCmd && controlCmd.commandType == ControlCommand.CommandType.StartThread) {
                state.callStack.PushThread ();
            }
        }

        // Mark a container as having been visited
        void VisitContainer(Container container, bool atStart)
        {
            if ( !container.countingAtStartOnly || atStart ) {
                if( container.visitsShouldBeCounted )
                    IncrementVisitCountForContainer (container);

                if (container.turnIndexShouldBeCounted)
                    RecordTurnIndexVisitToContainer (container);
            }
        }

        List<Container> _prevContainers = new List<Container>();
        void VisitChangedContainersDueToDivert()
        {
            var previousPointer = state.previousPointer;
            var pointer = state.currentPointer;

            // Unless we're pointing *directly* at a piece of content, we don't do
            // counting here. Otherwise, the main stepping function will do the counting.
            if (pointer.isNull || pointer.index == -1)
                return;
            
            // First, find the previously open set of containers
			_prevContainers.Clear();
            if (!previousPointer.isNull) {
                Container prevAncestor = previousPointer.Resolve() as Container ?? previousPointer.container as Container;
                while (prevAncestor) {
					_prevContainers.Add (prevAncestor);
                    prevAncestor = prevAncestor.parent as Container;
                }
            }

            // If the new object is a container itself, it will be visited automatically at the next actual
            // content step. However, we need to walk up the new ancestry to see if there are more new containers
            Runtime.Object currentChildOfContainer = pointer.Resolve();

            // Invalid pointer? May happen if attemptingto 
            if (currentChildOfContainer == null) return;

            Container currentContainerAncestor = currentChildOfContainer.parent as Container;
			while (currentContainerAncestor && !_prevContainers.Contains(currentContainerAncestor)) {

                // Check whether this ancestor container is being entered at the start,
                // by checking whether the child object is the first.
                bool enteringAtStart = currentContainerAncestor.content.Count > 0 
                    && currentChildOfContainer == currentContainerAncestor.content [0];

                // Mark a visit to this container
                VisitContainer (currentContainerAncestor, enteringAtStart);

                currentChildOfContainer = currentContainerAncestor;
                currentContainerAncestor = currentContainerAncestor.parent as Container;
            }
        }
            
        Choice ProcessChoice(ChoicePoint choicePoint)
        {
            bool showChoice = true;

            // Don't create choice if choice point doesn't pass conditional
            if (choicePoint.hasCondition) {
                var conditionValue = state.PopEvaluationStack ();
                if (!IsTruthy (conditionValue)) {
                    showChoice = false;
                }
            }

            string startText = "";
            string choiceOnlyText = "";

            if (choicePoint.hasChoiceOnlyContent) {
                var choiceOnlyStrVal = state.PopEvaluationStack () as StringValue;
                choiceOnlyText = choiceOnlyStrVal.value;
            }

            if (choicePoint.hasStartContent) {
                var startStrVal = state.PopEvaluationStack () as StringValue;
                startText = startStrVal.value;
            }

            // Don't create choice if player has already read this content
            if (choicePoint.onceOnly) {
                var visitCount = VisitCountForContainer (choicePoint.choiceTarget);
                if (visitCount > 0) {
                    showChoice = false;
                }
            }

            // We go through the full process of creating the choice above so
            // that we consume the content for it, since otherwise it'll
            // be shown on the output stream.
            if (!showChoice) {
                return null;
            }

            var choice = new Choice ();
            choice.targetPath = choicePoint.pathOnChoice;
            choice.sourcePath = choicePoint.path.ToString ();
            choice.isInvisibleDefault = choicePoint.isInvisibleDefault;
            choice.threadAtGeneration = state.callStack.currentThread.Copy ();

            // Set final text for the choice
            choice.text = (startText + choiceOnlyText).Trim(' ', '\t');

            return choice;
        }

        // Does the expression result represented by this object evaluate to true?
        // e.g. is it a Number that's not equal to 1?
        bool IsTruthy(Runtime.Object obj)
        {
            bool truthy = false;
            if (obj is Value) {
                var val = (Value)obj;

                if (val is DivertTargetValue) {
                    var divTarget = (DivertTargetValue)val;
                    Error ("Shouldn't use a divert target (to " + divTarget.targetPath + ") as a conditional value. Did you intend a function call 'likeThis()' or a read count check 'likeThis'? (no arrows)");
                    return false;
                }

                return val.isTruthy;
            }
            return truthy;
        }

        /// <summary>
        /// Checks whether contentObj is a control or flow object rather than a piece of content, 
        /// and performs the required command if necessary.
        /// </summary>
        /// <returns><c>true</c> if object was logic or flow control, <c>false</c> if it's normal content.</returns>
        /// <param name="contentObj">Content object.</param>
        bool PerformLogicAndFlowControl(Runtime.Object contentObj)
        {
            if( contentObj == null ) {
                return false;
            }

            // Divert
            if (contentObj is Divert) {
                
                Divert currentDivert = (Divert)contentObj;

                if (currentDivert.isConditional) {
                    var conditionValue = state.PopEvaluationStack ();

                    // False conditional? Cancel divert
                    if (!IsTruthy (conditionValue))
                        return true;
                }

                if (currentDivert.hasVariableTarget) {
                    var varName = currentDivert.variableDivertName;

                    var varContents = state.variablesState.GetVariableWithName (varName);

                    if (varContents == null) {
                        Error ("Tried to divert using a target from a variable that could not be found (" + varName + ")");
                    }
                    else if (!(varContents is DivertTargetValue)) {

                        var intContent = varContents as IntValue;

                        string errorMessage = "Tried to divert to a target from a variable, but the variable (" + varName + ") didn't contain a divert target, it ";
                        if (intContent && intContent.value == 0) {
                            errorMessage += "was empty/null (the value 0).";
                        } else {
                            errorMessage += "contained '" + varContents + "'.";
                        }

                        Error (errorMessage);
                    }

                    var target = (DivertTargetValue)varContents;
                    state.divertedPointer = PointerAtPath(target.targetPath);

                } else if (currentDivert.isExternal) {
                    CallExternalFunction (currentDivert.targetPathString, currentDivert.externalArgs);
                    return true;
                } else {
                    state.divertedPointer = currentDivert.targetPointer;
                }

                if (currentDivert.pushesToStack) {
                    state.callStack.Push (
                        currentDivert.stackPushType, 
                        outputStreamLengthWithPushed:state.outputStream.Count
                    );
                }

                if (state.divertedPointer.isNull && !currentDivert.isExternal) {

                    // Human readable name available - runtime divert is part of a hard-written divert that to missing content
                    if (currentDivert && currentDivert.debugMetadata.sourceName != null) {
                        Error ("Divert target doesn't exist: " + currentDivert.debugMetadata.sourceName);
                    } else {
                        Error ("Divert resolution failed: " + currentDivert);
                    }
                }

                return true;
            } 

            // Start/end an expression evaluation? Or print out the result?
            else if( contentObj is ControlCommand ) {
                var evalCommand = (ControlCommand) contentObj;

                switch (evalCommand.commandType) {

                case ControlCommand.CommandType.EvalStart:
                    Assert (state.inExpressionEvaluation == false, "Already in expression evaluation?");
                    state.inExpressionEvaluation = true;
                    break;

                case ControlCommand.CommandType.EvalEnd:
                    Assert (state.inExpressionEvaluation == true, "Not in expression evaluation mode");
                    state.inExpressionEvaluation = false;
                    break;

                case ControlCommand.CommandType.EvalOutput:

                    // If the expression turned out to be empty, there may not be anything on the stack
                    if (state.evaluationStack.Count > 0) {
                        
                        var output = state.PopEvaluationStack ();

                        // Functions may evaluate to Void, in which case we skip output
                        if (!(output is Void)) {
                            // TODO: Should we really always blanket convert to string?
                            // It would be okay to have numbers in the output stream the
                            // only problem is when exporting text for viewing, it skips over numbers etc.
                            var text = new StringValue (output.ToString ());

                            state.PushToOutputStream (text);
                        }

                    }
                    break;

                case ControlCommand.CommandType.NoOp:
                    break;

                case ControlCommand.CommandType.Duplicate:
                    state.PushEvaluationStack (state.PeekEvaluationStack ());
                    break;

                case ControlCommand.CommandType.PopEvaluatedValue:
                    state.PopEvaluationStack ();
                    break;

                case ControlCommand.CommandType.PopFunction:
                case ControlCommand.CommandType.PopTunnel:

                    var popType = evalCommand.commandType == ControlCommand.CommandType.PopFunction ?
                        PushPopType.Function : PushPopType.Tunnel;

                    // Tunnel onwards is allowed to specify an optional override
                    // divert to go to immediately after returning: ->-> target
                    DivertTargetValue overrideTunnelReturnTarget = null;
                    if (popType == PushPopType.Tunnel) {
                        var popped = state.PopEvaluationStack ();
                        overrideTunnelReturnTarget = popped as DivertTargetValue;
                        if (overrideTunnelReturnTarget == null) {
                            Assert (popped is Void, "Expected void if ->-> doesn't override target");
                        }
                    }

                    if (state.TryExitFunctionEvaluationFromGame ()) {
                        break;
                    }
                    else if (state.callStack.currentElement.type != popType || !state.callStack.canPop) {

                        var names = new Dictionary<PushPopType, string> ();
                        names [PushPopType.Function] = "function return statement (~ return)";
                        names [PushPopType.Tunnel] = "tunnel onwards statement (->->)";

                        string expected = names [state.callStack.currentElement.type];
                        if (!state.callStack.canPop) {
                            expected = "end of flow (-> END or choice)";
                        }

                        var errorMsg = string.Format ("Found {0}, when expected {1}", names [popType], expected);

                        Error (errorMsg);
                    } 

                    else {
                        state.PopCallstack ();

                        // Does tunnel onwards override by diverting to a new ->-> target?
                        if( overrideTunnelReturnTarget )
                            state.divertedPointer = PointerAtPath (overrideTunnelReturnTarget.targetPath);
                    }

                    break;

                case ControlCommand.CommandType.BeginString:
                    state.PushToOutputStream (evalCommand);

                    Assert (state.inExpressionEvaluation == true, "Expected to be in an expression when evaluating a string");
                    state.inExpressionEvaluation = false;
                    break;

                case ControlCommand.CommandType.EndString:
                    
                    // Since we're iterating backward through the content,
                    // build a stack so that when we build the string,
                    // it's in the right order
                    var contentStackForString = new Stack<Runtime.Object> ();

                    int outputCountConsumed = 0;
                    for (int i = state.outputStream.Count - 1; i >= 0; --i) {
                        var obj = state.outputStream [i];

                        outputCountConsumed++;

                        var command = obj as ControlCommand;
                        if (command != null && command.commandType == ControlCommand.CommandType.BeginString) {
                            break;
                        }

                        if( obj is StringValue )
                            contentStackForString.Push (obj);
                    }

                    // Consume the content that was produced for this string
                    state.PopFromOutputStream (outputCountConsumed);

                    // Build string out of the content we collected
                    var sb = new StringBuilder ();
                    foreach (var c in contentStackForString) {
                        sb.Append (c.ToString ());
                    }

                    // Return to expression evaluation (from content mode)
                    state.inExpressionEvaluation = true;
                    state.PushEvaluationStack (new StringValue (sb.ToString ()));
                    break;

                case ControlCommand.CommandType.ChoiceCount:
					var choiceCount = state.generatedChoices.Count;
                    state.PushEvaluationStack (new Runtime.IntValue (choiceCount));
                    break;

                case ControlCommand.CommandType.Turns:
                    state.PushEvaluationStack (new IntValue (state.currentTurnIndex+1));
                    break;

                case ControlCommand.CommandType.TurnsSince:
                case ControlCommand.CommandType.ReadCount:
                    var target = state.PopEvaluationStack();
                    if( !(target is DivertTargetValue) ) {
                        string extraNote = "";
                        if( target is IntValue )
                            extraNote = ". Did you accidentally pass a read count ('knot_name') instead of a target ('-> knot_name')?";
                        Error("TURNS_SINCE expected a divert target (knot, stitch, label name), but saw "+target+extraNote);
                        break;
                    }
                        
                    var divertTarget = target as DivertTargetValue;
                    var container = ContentAtPath (divertTarget.targetPath).correctObj as Container;

                    int eitherCount;
                    if (container != null) {
                        if (evalCommand.commandType == ControlCommand.CommandType.TurnsSince)
                            eitherCount = TurnsSinceForContainer (container);
                        else
                            eitherCount = VisitCountForContainer (container);
                    } else {
                        if (evalCommand.commandType == ControlCommand.CommandType.TurnsSince)
                            eitherCount = -1; // turn count, default to never/unknown
                        else
                            eitherCount = 0; // visit count, assume 0 to default to allowing entry

                        Warning ("Failed to find container for " + evalCommand.ToString () + " lookup at " + divertTarget.targetPath.ToString ());
                    }
                    
                    state.PushEvaluationStack (new IntValue (eitherCount));
                    break;
                    

                case ControlCommand.CommandType.Random: {
                        var maxInt = state.PopEvaluationStack () as IntValue;
                        var minInt = state.PopEvaluationStack () as IntValue;

                        if (minInt == null)
                            Error ("Invalid value for minimum parameter of RANDOM(min, max)");

                        if (maxInt == null)
                            Error ("Invalid value for maximum parameter of RANDOM(min, max)");

                        // +1 because it's inclusive of min and max, for e.g. RANDOM(1,6) for a dice roll.
                        var randomRange = maxInt.value - minInt.value + 1;
                        if (randomRange <= 0)
                            Error ("RANDOM was called with minimum as " + minInt.value + " and maximum as " + maxInt.value + ". The maximum must be larger");

                        var resultSeed = state.storySeed + state.previousRandom;
                        var random = new Random (resultSeed);

                        var nextRandom = random.Next ();
                        var chosenValue = (nextRandom % randomRange) + minInt.value;
                        state.PushEvaluationStack (new IntValue (chosenValue));

                        // Next random number (rather than keeping the Random object around)
                        state.previousRandom = nextRandom;
                        break;
                    }

                case ControlCommand.CommandType.SeedRandom:
                    var seed = state.PopEvaluationStack () as IntValue;
                    if (seed == null)
                        Error ("Invalid value passed to SEED_RANDOM");

                    // Story seed affects both RANDOM and shuffle behaviour
                    state.storySeed = seed.value;
                    state.previousRandom = 0;

                    // SEED_RANDOM returns nothing.
                    state.PushEvaluationStack (new Runtime.Void ());
                    break;

                case ControlCommand.CommandType.VisitIndex:
                    var count = VisitCountForContainer(state.currentPointer.container) - 1; // index not count
                    state.PushEvaluationStack (new IntValue (count));
                    break;

                case ControlCommand.CommandType.SequenceShuffleIndex:
                    var shuffleIndex = NextSequenceShuffleIndex ();
                    state.PushEvaluationStack (new IntValue (shuffleIndex));
                    break;

                case ControlCommand.CommandType.StartThread:
                    // Handled in main step function
                    break;

                case ControlCommand.CommandType.Done:
                    
                    // We may exist in the context of the initial
                    // act of creating the thread, or in the context of
                    // evaluating the content.
                    if (state.callStack.canPopThread) {
                        state.callStack.PopThread ();
                    } 

                    // In normal flow - allow safe exit without warning
                    else {
                        state.didSafeExit = true;

                        // Stop flow in current thread
                        state.currentPointer = Pointer.Null;
                    }

                    break;
                
                // Force flow to end completely
                case ControlCommand.CommandType.End:
                    state.ForceEnd ();
                    break;

                case ControlCommand.CommandType.ListFromInt:
                    var intVal = state.PopEvaluationStack () as IntValue;
                    var listNameVal = state.PopEvaluationStack () as StringValue;

					if (intVal == null) { 
						throw new StoryException ("Passed non-integer when creating a list element from a numerical value."); 
					}

                    ListValue generatedListValue = null;

                    ListDefinition foundListDef;
                    if (listDefinitions.TryListGetDefinition (listNameVal.value, out foundListDef)) {
                        InkListItem foundItem;
                        if (foundListDef.TryGetItemWithValue (intVal.value, out foundItem)) {
                            generatedListValue = new ListValue (foundItem, intVal.value);
                        }
                    } else {
                        throw new StoryException ("Failed to find LIST called " + listNameVal.value);
                    }

                    if (generatedListValue == null)
                        generatedListValue = new ListValue ();

                    state.PushEvaluationStack (generatedListValue);
                    break;

                case ControlCommand.CommandType.ListRange: {
                        var max = state.PopEvaluationStack ();
                        var min = state.PopEvaluationStack ();

                        var targetList = state.PopEvaluationStack () as ListValue;

                        if (targetList == null || min == null || max == null)
                            throw new StoryException ("Expected list, minimum and maximum for LIST_RANGE");

                        // Allow either int or a particular list item to be passed for the bounds,
                        // so wrap up a function to handle this casting for us.
                        Func<Runtime.Object, int> IntBound = (obj) => {
                            var listValue = obj as ListValue;
                            if (listValue) {
                                return (int)listValue.value.maxItem.Value;
                            }

                            var intValue = obj as IntValue;
                            if (intValue) {
                                return intValue.value;
                            }

                            return -1;
                        };

                        int minVal = IntBound (min);
                        int maxVal = IntBound (max);
                        if (minVal == -1)
                            throw new StoryException ("Invalid min range bound passed to LIST_VALUE(): " + min);

                        if (maxVal == -1)
                            throw new StoryException ("Invalid max range bound passed to LIST_VALUE(): " + max);

                        // Extract the range of items from the origin list
                        ListValue result = new ListValue ();
                        var origins = targetList.value.origins;

                        if (origins != null) {
                            foreach(var origin in origins) {
                                var rangeFromOrigin = origin.ListRange (minVal, maxVal);
                                foreach (var kv in rangeFromOrigin.value) {
                                    result.value [kv.Key] = kv.Value;
                                }
                            }
                        }
                            
                        state.PushEvaluationStack (result);
                        break;
                    }

                case ControlCommand.CommandType.ListRandom: {

                        var listVal = state.PopEvaluationStack () as ListValue;
                        if (listVal == null)
                            throw new StoryException ("Expected list for LIST_RANDOM");
                        
                        var list = listVal.value;

                        InkList newList = null;

                        // List was empty: return empty list
                        if (list.Count == 0) {
                            newList = new InkList ();
                        } 

                        // Non-empty source list
                        else {
                            // Generate a random index for the element to take
                            var resultSeed = state.storySeed + state.previousRandom;
                            var random = new Random (resultSeed);

                            var nextRandom = random.Next ();
                            var listItemIndex = nextRandom % list.Count;

                            // Iterate through to get the random element
                            var listEnumerator = list.GetEnumerator ();
                            for (int i = 0; i <= listItemIndex; i++) {
                                listEnumerator.MoveNext ();
                            }
                            var randomItem = listEnumerator.Current;

                            // Origin list is simply the origin of the one element
                            newList = new InkList (randomItem.Key.originName, this);
                            newList.Add (randomItem.Key, randomItem.Value);

                            state.previousRandom = nextRandom;
                        }

                        state.PushEvaluationStack (new ListValue(newList));
                        break;
                    }

                default:
                    Error ("unhandled ControlCommand: " + evalCommand);
                    break;
                }

                return true;
            }

            // Variable assignment
            else if( contentObj is VariableAssignment ) {
                var varAss = (VariableAssignment) contentObj;
                var assignedVal = state.PopEvaluationStack();

                // When in temporary evaluation, don't create new variables purely within
                // the temporary context, but attempt to create them globally
                //var prioritiseHigherInCallStack = _temporaryEvaluationContainer != null;

                state.variablesState.Assign (varAss, assignedVal);

                return true;
            }

            // Variable reference
            else if( contentObj is VariableReference ) {
                var varRef = (VariableReference)contentObj;
                Runtime.Object foundValue = null;


                // Explicit read count value
                if (varRef.pathForCount != null) {

                    var container = varRef.containerForCount;
                    int count = VisitCountForContainer (container);
                    foundValue = new IntValue (count);
                }

                // Normal variable reference
                else {

                    foundValue = state.variablesState.GetVariableWithName (varRef.name);

                    if (foundValue == null) {
                        var defaultVal = state.variablesState.TryGetDefaultVariableValue (varRef.name);
                        if (defaultVal != null) {
                            Warning ("Variable not found in save state: '" + varRef.name + "', but seems to have been newly created. Assigning value from latest ink's declaration: " + defaultVal);
                            foundValue = defaultVal;

                            // Save for future usage, preventing future errors
                            // Only do this for variables that are known to be globals, not those that may be missing temps.
                            state.variablesState.SetGlobal(varRef.name, foundValue);
                        } else {
                            Warning ("Variable not found: '" + varRef.name + "'. Using default value of 0 (false). This can happen with temporary variables if the declaration hasn't yet been hit.");
                            foundValue = new IntValue (0);
                        }
                    }
                }

                state.PushEvaluationStack (foundValue);

                return true;
            }

            // Native function call
            else if (contentObj is NativeFunctionCall) {
                var func = (NativeFunctionCall)contentObj;
                var funcParams = state.PopEvaluationStack (func.numberOfParameters);
                var result = func.Call (funcParams);
                state.PushEvaluationStack (result);
                return true;
            } 

            // No control content, must be ordinary content
            return false;
        }

        /// <summary>
        /// Change the current position of the story to the given path. From here you can 
        /// call Continue() to evaluate the next line.
        /// 
        /// The path string is a dot-separated path as used internally by the engine.
        /// These examples should work:
        /// 
        ///    myKnot
        ///    myKnot.myStitch
        /// 
        /// Note however that this won't necessarily work:
        /// 
        ///    myKnot.myStitch.myLabelledChoice
        /// 
        /// ...because of the way that content is nested within a weave structure.
        /// 
        /// By default this will reset the callstack beforehand, which means that any
        /// tunnels, threads or functions you were in at the time of calling will be
        /// discarded. This is different from the behaviour of ChooseChoiceIndex, which
        /// will always keep the callstack, since the choices are known to come from the
        /// correct state, and known their source thread.
        /// 
        /// You have the option of passing false to the resetCallstack parameter if you
        /// don't want this behaviour, and will leave any active threads, tunnels or
        /// function calls in-tact.
        /// 
        /// This is potentially dangerous! If you're in the middle of a tunnel,
        /// it'll redirect only the inner-most tunnel, meaning that when you tunnel-return
        /// using '->->', it'll return to where you were before. This may be what you
        /// want though. However, if you're in the middle of a function, ChoosePathString
        /// will throw an exception.
        /// 
        /// </summary>
        /// <param name="path">A dot-separted path string, as specified above.</param>
        /// <param name="resetCallstack">Whether to reset the callstack first (see summary description).</param>
        /// <param name="arguments">Optional set of arguments to pass, if path is to a knot that takes them.</param>
        public void ChoosePathString (string path, bool resetCallstack = true, params object [] arguments)
        {
            IfAsyncWeCant ("call ChoosePathString right now");

            if (resetCallstack) {
                ResetCallstack ();
            } else {
                // ChoosePathString is potentially dangerous since you can call it when the stack is
                // pretty much in any state. Let's catch one of the worst offenders.
                if (state.callStack.currentElement.type == PushPopType.Function) {
                    string funcDetail = "";
                    var container = state.callStack.currentElement.currentPointer.container;
                    if (container != null) {
                        funcDetail = "("+container.path.ToString ()+") ";
                    }
                    throw new System.Exception ("Story was running a function "+funcDetail+"when you called ChoosePathString("+path+") - this is almost certainly not not what you want! Full stack trace: \n"+state.callStack.callStackTrace);
                }
            }

            state.PassArgumentsToEvaluationStack (arguments);
            ChoosePath (new Path (path));
        }

        void IfAsyncWeCant (string activityStr)
        {
            if (_asyncContinueActive)
                throw new System.Exception ("Can't " + activityStr + ". Story is in the middle of a ContinueAsync(). Make more ContinueAsync() calls or a single Continue() call beforehand.");
        }

	    internal void ChoosePath(Path p, bool incrementingTurnIndex = true)
        {
            state.SetChosenPath (p, incrementingTurnIndex);

            // Take a note of newly visited containers for read counts etc
            VisitChangedContainersDueToDivert ();
        }

        /// <summary>
        /// Chooses the Choice from the currentChoices list with the given
        /// index. Internally, this sets the current content path to that
        /// pointed to by the Choice, ready to continue story evaluation.
        /// </summary>
        public void ChooseChoiceIndex(int choiceIdx)
        {
            var choices = currentChoices;
            Assert (choiceIdx >= 0 && choiceIdx < choices.Count, "choice out of range");

            // Replace callstack with the one from the thread at the choosing point, 
            // so that we can jump into the right place in the flow.
            // This is important in case the flow was forked by a new thread, which
            // can create multiple leading edges for the story, each of
            // which has its own context.
            var choiceToChoose = choices [choiceIdx];
            state.callStack.currentThread = choiceToChoose.threadAtGeneration;

            ChoosePath (choiceToChoose.targetPath);
        }

        /// <summary>
        /// Checks if a function exists.
        /// </summary>
        /// <returns>True if the function exists, else false.</returns>
        /// <param name="functionName">The name of the function as declared in ink.</param>
        public bool HasFunction (string functionName)
        {
            try {
                return KnotContainerWithName (functionName) != null;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Evaluates a function defined in ink.
        /// </summary>
        /// <returns>The return value as returned from the ink function with `~ return myValue`, or null if nothing is returned.</returns>
        /// <param name="functionName">The name of the function as declared in ink.</param>
        /// <param name="arguments">The arguments that the ink function takes, if any. Note that we don't (can't) do any validation on the number of arguments right now, so make sure you get it right!</param>
        public object EvaluateFunction (string functionName, params object [] arguments)
        {
            string _;
            return EvaluateFunction (functionName, out _, arguments);
        }

        /// <summary>
        /// Evaluates a function defined in ink, and gathers the possibly multi-line text as generated by the function.
        /// This text output is any text written as normal content within the function, as opposed to the return value, as returned with `~ return`.
        /// </summary>
        /// <returns>The return value as returned from the ink function with `~ return myValue`, or null if nothing is returned.</returns>
        /// <param name="functionName">The name of the function as declared in ink.</param>
        /// <param name="textOutput">The text content produced by the function via normal ink, if any.</param>
        /// <param name="arguments">The arguments that the ink function takes, if any. Note that we don't (can't) do any validation on the number of arguments right now, so make sure you get it right!</param>
        public object EvaluateFunction (string functionName, out string textOutput, params object [] arguments)
        {
            IfAsyncWeCant ("evaluate a function");

			if(functionName == null) {
				throw new System.Exception ("Function is null");
			} else if(functionName == string.Empty || functionName.Trim() == string.Empty) {
				throw new System.Exception ("Function is empty or white space.");
			}

            // Get the content that we need to run
            var funcContainer = KnotContainerWithName (functionName);
            if( funcContainer == null )
                throw new System.Exception ("Function doesn't exist: '" + functionName + "'");

            // Snapshot the output stream
            var outputStreamBefore = new List<Runtime.Object>(state.outputStream);
            _state.ResetOutput ();

            // State will temporarily replace the callstack in order to evaluate
            state.StartFunctionEvaluationFromGame (funcContainer, arguments);

            // Evaluate the function, and collect the string output
            var stringOutput = new StringBuilder ();
            while (canContinue) {
                stringOutput.Append (Continue ());
            }
            textOutput = stringOutput.ToString ();

            // Restore the output stream in case this was called
            // during main story evaluation.
            _state.ResetOutput (outputStreamBefore);

            // Finish evaluation, and see whether anything was produced
            var result = state.CompleteFunctionEvaluationFromGame ();
            return result;
        }

        // Evaluate a "hot compiled" piece of ink content, as used by the REPL-like
        // CommandLinePlayer.
	    internal Runtime.Object EvaluateExpression(Runtime.Container exprContainer)
        {
            int startCallStackHeight = state.callStack.elements.Count;

            state.callStack.Push (PushPopType.Tunnel);

            _temporaryEvaluationContainer = exprContainer;

            state.GoToStart ();

            int evalStackHeight = state.evaluationStack.Count;

            Continue ();

            _temporaryEvaluationContainer = null;

            // Should have fallen off the end of the Container, which should
            // have auto-popped, but just in case we didn't for some reason,
            // manually pop to restore the state (including currentPath).
            if (state.callStack.elements.Count > startCallStackHeight) {
                state.PopCallstack ();
            }

            int endStackHeight = state.evaluationStack.Count;
            if (endStackHeight > evalStackHeight) {
                return state.PopEvaluationStack ();
            } else {
                return null;
            }

        }

        /// <summary>
        /// An ink file can provide a fallback functions for when when an EXTERNAL has been left
        /// unbound by the client, and the fallback function will be called instead. Useful when
        /// testing a story in playmode, when it's not possible to write a client-side C# external
        /// function, but you don't want it to fail to run.
        /// </summary>
        public bool allowExternalFunctionFallbacks { get; set; }

	    internal void CallExternalFunction(string funcName, int numberOfArguments)
        {
            ExternalFunction func = null;
            Container fallbackFunctionContainer = null;

            var foundExternal = _externals.TryGetValue (funcName, out func);

            // Try to use fallback function?
            if (!foundExternal) {
                if (allowExternalFunctionFallbacks) {
                    fallbackFunctionContainer = KnotContainerWithName (funcName);
                    Assert (fallbackFunctionContainer != null, "Trying to call EXTERNAL function '" + funcName + "' which has not been bound, and fallback ink function could not be found.");

                    // Divert direct into fallback function and we're done
                    state.callStack.Push (
                        PushPopType.Function, 
                        outputStreamLengthWithPushed:state.outputStream.Count
                    );
                    state.divertedPointer = Pointer.StartOf(fallbackFunctionContainer);
                    return;

                } else {
                    Assert (false, "Trying to call EXTERNAL function '" + funcName + "' which has not been bound (and ink fallbacks disabled).");
                }
            }

            // Pop arguments
            var arguments = new List<object>();
            for (int i = 0; i < numberOfArguments; ++i) {
                var poppedObj = state.PopEvaluationStack () as Value;
                var valueObj = poppedObj.valueObject;
                arguments.Add (valueObj);
            }

            // Reverse arguments from the order they were popped,
            // so they're the right way round again.
            arguments.Reverse ();

            // Run the function!
            object funcResult = func (arguments.ToArray());

            // Convert return value (if any) to the a type that the ink engine can use
            Runtime.Object returnObj = null;
            if (funcResult != null) {
                returnObj = Value.Create (funcResult);
                Assert (returnObj != null, "Could not create ink value from returned object of type " + funcResult.GetType());
            } else {
                returnObj = new Runtime.Void ();
            }
                
            state.PushEvaluationStack (returnObj);
        }

        /// <summary>
        /// General purpose delegate definition for bound EXTERNAL function definitions
        /// from ink. Note that this version isn't necessary if you have a function
        /// with three arguments or less - see the overloads of BindExternalFunction.
        /// </summary>
        public delegate object ExternalFunction(object[] args);

        /// <summary>
        /// Most general form of function binding that returns an object
        /// and takes an array of object parameters.
        /// The only way to bind a function with more than 3 arguments.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="func">The C# function to bind.</param>
        public void BindExternalFunctionGeneral(string funcName, ExternalFunction func)
        {
            IfAsyncWeCant ("bind an external function");
            Assert (!_externals.ContainsKey (funcName), "Function '" + funcName + "' has already been bound.");
            _externals [funcName] = func;
        }

        object TryCoerce<T>(object value)
        {  
            if (value == null)
                return null;

            if (value is T)
                return (T) value;

            if (value is float && typeof(T) == typeof(int)) {
                int intVal = (int)Math.Round ((float)value);
                return intVal;
            }

            if (value is int && typeof(T) == typeof(float)) {
                float floatVal = (float)(int)value;
                return floatVal;
            }

            if (value is int && typeof(T) == typeof(bool)) {
                int intVal = (int)value;
                return intVal == 0 ? false : true;
            }

            if (typeof(T) == typeof(string)) {
                return value.ToString ();
            }

            Assert (false, "Failed to cast " + value.GetType ().Name + " to " + typeof(T).Name);

            return null;
        }

        // Convenience overloads for standard functions and actions of various arities
        // Is there a better way of doing this?!

        /// <summary>
        /// Bind a C# function to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="func">The C# function to bind.</param>
        public void BindExternalFunction(string funcName, Func<object> func)
        {
			Assert(func != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 0, "External function expected no arguments");
                return func();
            });
        }

        /// <summary>
        /// Bind a C# Action to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="act">The C# action to bind.</param>
        public void BindExternalFunction(string funcName, Action act)
        {
			Assert(act != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 0, "External function expected no arguments");
                act();
                return null;
            });
        }

        /// <summary>
        /// Bind a C# function to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="func">The C# function to bind.</param>
        public void BindExternalFunction<T>(string funcName, Func<T, object> func)
        {
			Assert(func != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 1, "External function expected one argument");
                return func( (T)TryCoerce<T>(args[0]) );
            });
        }

        /// <summary>
        /// Bind a C# action to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="act">The C# action to bind.</param>
        public void BindExternalFunction<T>(string funcName, Action<T> act)
        {
			Assert(act != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 1, "External function expected one argument");
                act( (T)TryCoerce<T>(args[0]) );
                return null;
            });
        }


        /// <summary>
        /// Bind a C# function to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="func">The C# function to bind.</param>
        public void BindExternalFunction<T1, T2>(string funcName, Func<T1, T2, object> func)
        {
			Assert(func != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 2, "External function expected two arguments");
                return func(
                    (T1)TryCoerce<T1>(args[0]), 
                    (T2)TryCoerce<T2>(args[1])
                );
            });
        }

        /// <summary>
        /// Bind a C# action to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="act">The C# action to bind.</param>
        public void BindExternalFunction<T1, T2>(string funcName, Action<T1, T2> act)
        {
			Assert(act != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 2, "External function expected two arguments");
                act(
                    (T1)TryCoerce<T1>(args[0]), 
                    (T2)TryCoerce<T2>(args[1])
                );
                return null;
            });
        }

        /// <summary>
        /// Bind a C# function to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="func">The C# function to bind.</param>
        public void BindExternalFunction<T1, T2, T3>(string funcName, Func<T1, T2, T3, object> func)
        {
			Assert(func != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 3, "External function expected two arguments");
                return func(
                    (T1)TryCoerce<T1>(args[0]), 
                    (T2)TryCoerce<T2>(args[1]),
                    (T3)TryCoerce<T3>(args[2])
                );
            });
        }

        /// <summary>
        /// Bind a C# action to an ink EXTERNAL function declaration.
        /// </summary>
        /// <param name="funcName">EXTERNAL ink function name to bind to.</param>
        /// <param name="act">The C# action to bind.</param>
        public void BindExternalFunction<T1, T2, T3>(string funcName, Action<T1, T2, T3> act)
        {
			Assert(act != null, "Can't bind a null function");

            BindExternalFunctionGeneral (funcName, (object[] args) => {
                Assert(args.Length == 3, "External function expected two arguments");
                act(
                    (T1)TryCoerce<T1>(args[0]), 
                    (T2)TryCoerce<T2>(args[1]),
                    (T3)TryCoerce<T3>(args[2])
                );
                return null;
            });
        }

        /// <summary>
        /// Remove a binding for a named EXTERNAL ink function.
        /// </summary>
        public void UnbindExternalFunction(string funcName)
        {
            IfAsyncWeCant ("unbind an external a function");
            Assert (_externals.ContainsKey (funcName), "Function '" + funcName + "' has not been bound.");
            _externals.Remove (funcName);
        }

        /// <summary>
        /// Check that all EXTERNAL ink functions have a valid bound C# function.
        /// Note that this is automatically called on the first call to Continue().
        /// </summary>
        public void ValidateExternalBindings()
        {
			var missingExternals = new HashSet<string>();

			ValidateExternalBindings (_mainContentContainer, missingExternals);
            _hasValidatedExternals = true;

			// No problem! Validation complete
			if( missingExternals.Count == 0 ) {
				_hasValidatedExternals = true;
			} 

			// Error for all missing externals
			else {
				var message = string.Format("ERROR: Missing function binding for external{0}: '{1}' {2}",
					missingExternals.Count > 1 ? "s" : string.Empty,
					string.Join("', '", missingExternals.ToArray()),
					allowExternalFunctionFallbacks ? ", and no fallback ink function found." : " (ink fallbacks disabled)"
				);
					
				Error(message);
			}
        }

		void ValidateExternalBindings(Container c, HashSet<string> missingExternals)
        {
            foreach (var innerContent in c.content) {
				var container = innerContent as Container;
				if( container == null || !container.hasValidName )
					ValidateExternalBindings (innerContent, missingExternals);
            }
            foreach (var innerKeyValue in c.namedContent) {
				ValidateExternalBindings (innerKeyValue.Value as Runtime.Object, missingExternals);
            }
        }

		void ValidateExternalBindings(Runtime.Object o, HashSet<string> missingExternals)
        {
            var container = o as Container;
            if (container) {
                ValidateExternalBindings (container, missingExternals);
                return;
            }

            var divert = o as Divert;
            if (divert && divert.isExternal) {
                var name = divert.targetPathString;

                if (!_externals.ContainsKey (name)) {
					if( allowExternalFunctionFallbacks ) {
						bool fallbackFound = mainContentContainer.namedContent.ContainsKey(name);
						if( !fallbackFound ) {
							missingExternals.Add(name);
						}
					} else {
						missingExternals.Add(name);
					}
                }
            }
        }
           
        /// <summary>
        /// Delegate definition for variable observation - see ObserveVariable.
        /// </summary>
        public delegate void VariableObserver(string variableName, object newValue);

        /// <summary>
        /// When the named global variable changes it's value, the observer will be
        /// called to notify it of the change. Note that if the value changes multiple
        /// times within the ink, the observer will only be called once, at the end
        /// of the ink's evaluation. If, during the evaluation, it changes and then
        /// changes back again to its original value, it will still be called.
        /// Note that the observer will also be fired if the value of the variable
        /// is changed externally to the ink, by directly setting a value in
        /// story.variablesState.
        /// </summary>
        /// <param name="variableName">The name of the global variable to observe.</param>
        /// <param name="observer">A delegate function to call when the variable changes.</param>
        public void ObserveVariable(string variableName, VariableObserver observer)
        {
            IfAsyncWeCant ("observe a new variable");

            if (_variableObservers == null)
                _variableObservers = new Dictionary<string, VariableObserver> ();

			if( !state.variablesState.GlobalVariableExistsWithName(variableName) ) 
				throw new StoryException("Cannot observe variable '"+variableName+"' because it wasn't declared in the ink story.");

            if (_variableObservers.ContainsKey (variableName)) {
                _variableObservers[variableName] += observer;
            } else {
                _variableObservers[variableName] = observer;
            }
        }

        /// <summary>
        /// Convenience function to allow multiple variables to be observed with the same
        /// observer delegate function. See the singular ObserveVariable for details.
        /// The observer will get one call for every variable that has changed.
        /// </summary>
        /// <param name="variableNames">The set of variables to observe.</param>
        /// <param name="observer">The delegate function to call when any of the named variables change.</param>
        public void ObserveVariables(IList<string> variableNames, VariableObserver observer)
        {
            foreach (var varName in variableNames) {
                ObserveVariable (varName, observer);
            }
        }

        /// <summary>
        /// Removes the variable observer, to stop getting variable change notifications.
        /// If you pass a specific variable name, it will stop observing that particular one. If you
        /// pass null (or leave it blank, since it's optional), then the observer will be removed
        /// from all variables that it's subscribed to.
        /// </summary>
        /// <param name="observer">The observer to stop observing.</param>
        /// <param name="specificVariableName">(Optional) Specific variable name to stop observing.</param>
        public void RemoveVariableObserver(VariableObserver observer, string specificVariableName = null)
        {
            IfAsyncWeCant ("remove a variable observer");

            if (_variableObservers == null)
                return;

            // Remove observer for this specific variable
            if (specificVariableName != null) {
                if (_variableObservers.ContainsKey (specificVariableName)) {
                    _variableObservers [specificVariableName] -= observer;
                }
            } 

            // Remove observer for all variables
            else {
                foreach (var keyValue in _variableObservers) {
                    var varName = keyValue.Key;
                    _variableObservers [varName] -= observer;
                }
            }
        }

        void VariableStateDidChangeEvent(string variableName, Runtime.Object newValueObj)
        {
            if (_variableObservers == null)
                return;
            
            VariableObserver observers = null;
            if (_variableObservers.TryGetValue (variableName, out observers)) {

                if (!(newValueObj is Value)) {
                    throw new System.Exception ("Tried to get the value of a variable that isn't a standard type");
                }
                var val = newValueObj as Value;

                observers (variableName, val.valueObject);
            }
        }

        /// <summary>
        /// Get any global tags associated with the story. These are defined as
        /// hash tags defined at the very top of the story.
        /// </summary>
        public List<string> globalTags {
            get {
                return TagsAtStartOfFlowContainerWithPathString ("");
            }
        }

        /// <summary>
        /// Gets any tags associated with a particular knot or knot.stitch.
        /// These are defined as hash tags defined at the very top of a 
        /// knot or stitch.
        /// </summary>
        /// <param name="path">The path of the knot or stitch, in the form "knot" or "knot.stitch".</param>
        public List<string> TagsForContentAtPath (string path)
        {
            return TagsAtStartOfFlowContainerWithPathString (path);
        }

        List<string> TagsAtStartOfFlowContainerWithPathString (string pathString)
        {
            var path = new Runtime.Path (pathString);

            // Expected to be global story, knot or stitch
            var flowContainer = ContentAtPath (path).container;
            while(true) {
                var firstContent = flowContainer.content [0];
                if (firstContent is Container)
                    flowContainer = (Container)firstContent;
                else break;
            }

            // Any initial tag objects count as the "main tags" associated with that story/knot/stitch
            List<string> tags = null;
            foreach (var c in flowContainer.content) {
                var tag = c as Runtime.Tag;
                if (tag) {
                    if (tags == null) tags = new List<string> ();
                    tags.Add (tag.text);
                } else break;
            }

            return tags;
        }

        /// <summary>
        /// Useful when debugging a (very short) story, to visualise the state of the
        /// story. Add this call as a watch and open the extended text. A left-arrow mark
        /// will denote the current point of the story.
        /// It's only recommended that this is used on very short debug stories, since
        /// it can end up generate a large quantity of text otherwise.
        /// </summary>
        public virtual string BuildStringOfHierarchy()
        {
            var sb = new StringBuilder ();

            mainContentContainer.BuildStringOfHierarchy (sb, 0, state.currentPointer.Resolve());

            return sb.ToString ();
        }

        string BuildStringOfContainer (Container container)
        {
        	var sb = new StringBuilder ();

            container.BuildStringOfHierarchy (sb, 0, state.currentPointer.Resolve());

        	return sb.ToString();
        }

		private void NextContent()
		{
            // Setting previousContentObject is critical for VisitChangedContainersDueToDivert
            state.previousPointer = state.currentPointer;

			// Divert step?
			if (!state.divertedPointer.isNull) {

                state.currentPointer = state.divertedPointer;
                state.divertedPointer = Pointer.Null;

                // Internally uses state.previousContentObject and state.currentContentObject
                VisitChangedContainersDueToDivert ();

                // Diverted location has valid content?
                if (!state.currentPointer.isNull) {
                    return;
                }
				
                // Otherwise, if diverted location doesn't have valid content,
                // drop down and attempt to increment.
                // This can happen if the diverted path is intentionally jumping
                // to the end of a container - e.g. a Conditional that's re-joining
			}

            bool successfulPointerIncrement = IncrementContentPointer ();

            // Ran out of content? Try to auto-exit from a function,
            // or finish evaluating the content of a thread
            if (!successfulPointerIncrement) {

                bool didPop = false;

                if (state.callStack.CanPop (PushPopType.Function)) {

                    // Pop from the call stack
                    state.PopCallstack (PushPopType.Function);

                    // This pop was due to dropping off the end of a function that didn't return anything,
                    // so in this case, we make sure that the evaluator has something to chomp on if it needs it
                    if (state.inExpressionEvaluation) {
                        state.PushEvaluationStack (new Runtime.Void ());
                    }

                    didPop = true;
                } else if (state.callStack.canPopThread) {
                    state.callStack.PopThread ();

                    didPop = true;
                } else {
                    state.TryExitFunctionEvaluationFromGame ();
                }

                // Step past the point where we last called out
                if (didPop && !state.currentPointer.isNull) {
                    NextContent ();
                }
			}
		}

        bool IncrementContentPointer()
        {
            bool successfulIncrement = true;

            var pointer = state.callStack.currentElement.currentPointer;
            pointer.index++;

            // Each time we step off the end, we fall out to the next container, all the
            // while we're in indexed rather than named content
            while (pointer.index >= pointer.container.content.Count) {

                successfulIncrement = false;

                Container nextAncestor = pointer.container.parent as Container;
                if (!nextAncestor) {
                    break;
                }

                var indexInAncestor = nextAncestor.content.IndexOf (pointer.container);
                if (indexInAncestor == -1) {
                    break;
                }

                pointer = new Pointer (nextAncestor, indexInAncestor);

                // Increment to next content in outer container
                pointer.index++;

                successfulIncrement = true;
            }

            if (!successfulIncrement) pointer = Pointer.Null;

            state.callStack.currentElement.currentPointer = pointer;

            return successfulIncrement;
        }
            
        bool TryFollowDefaultInvisibleChoice()
        {
            var allChoices = _state.currentChoices;

            // Is a default invisible choice the ONLY choice?
            var invisibleChoices = allChoices.Where (c => c.isInvisibleDefault).ToList();
            if (invisibleChoices.Count == 0 || allChoices.Count > invisibleChoices.Count)
                return false;

            var choice = invisibleChoices [0];

            ChoosePath (choice.targetPath, incrementingTurnIndex: false);

            return true;
        }
            
        int VisitCountForContainer(Container container)
        {
            if( !container.visitsShouldBeCounted ) {
                Error ("Read count for target ("+container.name+" - on "+container.debugMetadata+") unknown. The story may need to be compiled with countAllVisits flag (-c).");
                return 0;
            }

            int count = 0;
            var containerPathStr = container.path.ToString();
            state.visitCounts.TryGetValue (containerPathStr, out count);
            return count;
        }

        void IncrementVisitCountForContainer(Container container)
        {
            int count = 0;
            var containerPathStr = container.path.ToString();
            state.visitCounts.TryGetValue (containerPathStr, out count);
            count++;
            state.visitCounts [containerPathStr] = count;
        }

        void RecordTurnIndexVisitToContainer(Container container)
        {
            var containerPathStr = container.path.ToString();
            state.turnIndices [containerPathStr] = state.currentTurnIndex;
        }

        int TurnsSinceForContainer(Container container)
        {
            if( !container.turnIndexShouldBeCounted ) {
                Error ("TURNS_SINCE() for target ("+container.name+" - on "+container.debugMetadata+") unknown. The story may need to be compiled with countAllVisits flag (-c).");
            }

            int index = 0;
            var containerPathStr = container.path.ToString();
            if (state.turnIndices.TryGetValue (containerPathStr, out index)) {
                return state.currentTurnIndex - index;
            } else {
                return -1;
            }
        }

        // Note that this is O(n), since it re-evaluates the shuffle indices
        // from a consistent seed each time.
        // TODO: Is this the best algorithm it can be?
        int NextSequenceShuffleIndex()
        {
            var numElementsIntVal = state.PopEvaluationStack () as IntValue;
            if (numElementsIntVal == null) {
                Error ("expected number of elements in sequence for shuffle index");
                return 0;
            }

            var seqContainer = state.currentPointer.container;

            int numElements = numElementsIntVal.value;

            var seqCountVal = state.PopEvaluationStack () as IntValue;
            var seqCount = seqCountVal.value;
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
            var randomSeed = sequenceHash + loopIndex + state.storySeed;
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

        // Throw an exception that gets caught and causes AddError to be called,
        // then exits the flow.
	    internal void Error(string message, bool useEndLineNumber = false)
        {
            var e = new StoryException (message);
            e.useEndLineNumber = useEndLineNumber;
            throw e;
        }

	    internal void Warning (string message)
        {
            AddError (message, isWarning:true);
        }

        void AddError (string message, bool isWarning = false, bool useEndLineNumber = false)
        {
            var dm = currentDebugMetadata;

            var errorTypeStr = isWarning ? "WARNING" : "ERROR";

            if (dm != null) {
                int lineNum = useEndLineNumber ? dm.endLineNumber : dm.startLineNumber;
                message = string.Format ("RUNTIME {0}: '{1}' line {2}: {3}", errorTypeStr, dm.fileName, lineNum, message);
            } else if( !state.currentPointer.isNull  ) {
                message = string.Format ("RUNTIME {0}: ({1}): {2}", errorTypeStr, state.currentPointer.path, message);
			} else {
                message = "RUNTIME "+errorTypeStr+": " + message;
            }

            state.AddError (message, isWarning);

            // In a broken state don't need to know about any other errors.
            if( !isWarning )
                state.ForceEnd ();
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
                    
                throw new System.Exception (message + " " + currentDebugMetadata);
            }
        }

        DebugMetadata currentDebugMetadata
        {
            get {
                DebugMetadata dm;

                // Try to get from the current path first
                var pointer = state.currentPointer;
                if (!pointer.isNull) {
                    dm = pointer.Resolve().debugMetadata;
                    if (dm != null) {
                        return dm;
                    }
                }
                    
                // Move up callstack if possible
                for (int i = state.callStack.elements.Count - 1; i >= 0; --i) {
                    pointer = state.callStack.elements [i].currentPointer;
                    if (!pointer.isNull && pointer.Resolve() != null) {
                        dm = pointer.Resolve().debugMetadata;
                        if (dm != null) {
                            return dm;
                        }
                    }
                }

                // Current/previous path may not be valid if we've just had an error,
                // or if we've simply run out of content.
                // As a last resort, try to grab something from the output stream
                for (int i = state.outputStream.Count - 1; i >= 0; --i) {
                    var outputObj = state.outputStream [i];
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

	    internal Container mainContentContainer {
            get {
                if (_temporaryEvaluationContainer) {
                    return _temporaryEvaluationContainer;
                } else {
                    return _mainContentContainer;
                }
            }
        }

        Container _mainContentContainer;
        ListDefinitionsOrigin _listDefinitions;

        Dictionary<string, ExternalFunction> _externals;
        Dictionary<string, VariableObserver> _variableObservers;
        bool _hasValidatedExternals;

        Container _temporaryEvaluationContainer;

        StoryState _state;

        bool _asyncContinueActive;
        StoryState _stateAtLastNewline = null;

        int _recursiveContinueCount = 0;

		Profiler _profiler;
	}
}

