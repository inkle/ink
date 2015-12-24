using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Ink.Runtime
{
	public class Story : Runtime.Object
	{
        const int inkVersionCurrent = 9;

        // Version numbers are for engine itself and story file, rather
        // than the save format.
        //  -- old engine, new format: always fail
        //  -- new engine, old format: possibly cope, based on this number
        // When incrementing the version number above, the question you
        // should ask yourself is:
        //  -- Will the engine be able to load an old story file from 
        //     before I made these changes to the engine?
        //     If possible, you should support it, though it's not as
        //     critical as loading old save games, since it's an
        //     in-development problem only.
        const int inkVersionMinimumCompatible = 9;

        internal Path currentPath { 
            get { 
                return _callStack.currentElement.path; 
            } 
            private set {
                _callStack.currentElement.path = value;
            }
        }

        public List<Runtime.Object> outputStream;


        public List<ChoiceInstance> currentChoices
		{
			get 
			{
                return CurrentOutput<ChoiceInstance> (c => !c.choice.isInvisibleDefault);
			}
		}

		public string currentText
		{
			get 
			{
                return StringExt.Join("", CurrentOutput<Runtime.Text> ());
			}
		}

        public List<string> currentErrors
        {
            get {
                return _currentErrors;
            }
        }

        public bool hasError
        {
            get {
                return _currentErrors != null && _currentErrors.Count > 0;
            }
        }

        public VariablesState variablesState
        {
            get {
                return _variablesState;
            }
        }
            
        internal Story (Container contentContainer)
		{
			_mainContentContainer = contentContainer;
            _externals = new Dictionary<string, ExternalFunction> ();

            Reset ();
		}

        public static Story CreateWithJson(string jsonString)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
            while (reader.Read ()) {

                if (reader.TokenType == JsonToken.PropertyName) {

                    var propName = reader.Value as string;
                    if (propName == "inkVersion") {
                        reader.Read ();
                        int formatFromFile = System.Convert.ToInt32( reader.Value );

                        if (formatFromFile > inkVersionCurrent) {
                            throw new System.Exception ("Version of ink used to build story was newer than the current verison of the engine");
                        } else if (formatFromFile < inkVersionMinimumCompatible) {
                            throw new System.Exception ("Version of ink used to build story is too old to be loaded by this verison of the engine");
                        } else if (formatFromFile != inkVersionCurrent) {
                            Console.WriteLine ("WARNING: Version of ink used to build story doesn't match current version of engine. Non-critical, but recommend synchronising.");
                        }
                    } 

                    else if (propName == "root") {

                        var settings = new JsonSerializerSettings { 
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        };

                        settings.Converters.Add(new ObjectJsonConverter());

                        var serialiser = JsonSerializer.Create (settings);

                        reader.Read ();

                        var rootContainer = serialiser.Deserialize<Container> (reader);
                        return new Story (rootContainer);
                    }
                }

            }

            throw new System.Exception ("Root node for ink not found. Are you sure it's a valid .ink.json file?");
        }

        public string ToJsonString(bool indented = false)
        {
            var formatting = indented ? Formatting.Indented : Formatting.None;
            var settings = new JsonSerializerSettings { 
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add (new JsonSimplificationConverter ());

            var rootJsonString = JsonConvert.SerializeObject(_mainContentContainer, formatting, settings);

            // Wrap root in an object 
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter (sw)) {
                writer.WriteStartObject();
                writer.WritePropertyName("inkVersion");
                writer.WriteValue(inkVersionCurrent);
                writer.WritePropertyName("root");
                writer.WriteRawValue (rootJsonString);
                writer.WriteEndObject();
            }
                
            return sb.ToString ();
        }

        internal Runtime.Object ContentAtPath(Path path)
		{
			return mainContentContainer.ContentAtPath (path);
		}

		public void Begin()
		{
            Reset ();
			Continue ();
		}

        public void Reset()
        {
            outputStream = new List<Runtime.Object> ();
            _evaluationStack = new List<Runtime.Object> ();
            _callStack = new CallStack ();
            _variablesState = new VariablesState (_callStack);
            _visitCounts = new Dictionary<string, int> ();
            _turnIndices = new Dictionary<string, int> ();
            _currentTurnIndex = -1;

            // Seed the shuffle random numbers
            int timeSeed = DateTime.Now.Millisecond;
            _storySeed = (new Random (timeSeed)).Next () % 100;

            // Go to start
            currentPath = Path.ToFirstElement ();
        }

        public void ResetErrors()
        {
            _currentErrors = null;
        }

		public void Continue()
		{
            _didSafeExit = false;
            _currentTurnIndex++;

            try {

                while( Step () || TryFollowDefaultInvisibleChoice() ) {}

                if( _callStack.canPopThread ) {
                    Error("Thread available to pop, threads should always be flat by the end of evaluation?");
                }

                if( currentChoices.Count == 0 && !_didSafeExit ) {
                    if( _callStack.CanPop(PushPopType.Tunnel) ) {
                        Error("unexpectedly reached end of content. Do you need a '->->' to return from a tunnel?");
                    } else if( _callStack.CanPop(PushPopType.Function) ) {
                        Error("unexpectedly reached end of content. Do you need a '~ return'?");
                    } else if( !_callStack.canPop ) {
                        Error("ran out of content. Do you need a '-> DONE' or '-> END'?");
                    } else {
                        Error("unexpectedly reached end of content for unknown reason. Please debug compiler!");
                    }
                }

            } catch(StoryException e) {
                AddError (e.Message, e.useEndLineNumber);
            } finally {
                _callStack.currentThread.ResetOpenContainers ();
                _didSafeExit = false;
            }
		}

        // Return false if story ran out of content
        bool Step ()
        {
            var currentContentObj = ContentAtPath (currentPath);
            if (currentContentObj == null)
                return false;
            
            // Convert path to get first leaf content
            Container currentContainer = currentContentObj as Container;
            if (currentContainer && currentContainer.content.Count > 0) {
                currentPath = currentContainer.pathToFirstLeafContent;
                currentContentObj = ContentAtPath (currentPath);
            }

            IncrementVisitCountForActiveContainers (currentContentObj);

            // Is the current content object:
            //  - Normal content
            //  - Or a logic/flow statement - if so, do it
            // Stop flow if we hit a stack pop when we're unable to pop (e.g. return/done statement in knot
            // that was diverted to rather than called as a function)
            bool endFlow;

            bool isLogicOrFlowControl = PerformLogicAndFlowControl (currentContentObj, out endFlow);
            if (endFlow) {
                currentPath = null;

                while (_callStack.canPopThread)
                    _callStack.PopThread ();
                
                while (_callStack.canPop)
                    _callStack.Pop ();

                return false;
            }

            // Choice with condition?
            bool shouldAddObject = true;
            var choice = currentContentObj as Choice;
            if (choice) {
                var choiceInstance = ProcessChoice (choice);
                currentContentObj = choiceInstance;
                shouldAddObject = currentContentObj != null;
            }

            // If the container has no content, then it will be
            // the "content" itself, but we skip over it.
            if (currentContentObj is Container) {
                shouldAddObject = false;
            }

            // Content to add to evaluation stack or the output stream
            if (!isLogicOrFlowControl && shouldAddObject) {

                // If we're pushing a variable pointer onto the evaluation stack, ensure that it's specific
                // to our current (possibly temporary) context index. And make a copy of the pointer
                // so that we're not editing the original runtime object.
                var varPointer = currentContentObj as LiteralVariablePointer;
                if (varPointer && varPointer.contextIndex == -1) {
                    currentContentObj = new LiteralVariablePointer (varPointer.variableName, _callStack.currentElementIndex);
                }

                // Expression evaluation content
                if (inExpressionEvaluation) {
                    PushEvaluationStack (currentContentObj);
                }
                // Output stream content (i.e. not expression evaluation)
                else {
                    PushToOutputStream (currentContentObj);
                }
            }

            // Increment the content pointer, following diverts if necessary
            NextContent ();

            // Starting a thread should be done after the increment to the content pointer,
            // so that when returning from the thread, it returns to the content after this instruction.
            var controlCmd = currentContentObj as ControlCommand;
            if (controlCmd && controlCmd.commandType == ControlCommand.CommandType.StartThread) {
                _callStack.PushThread ();
            }

            // Do we have somewhere valid to go?
            return currentPath != null;
        }

        ChoiceInstance ProcessChoice(Choice choice)
        {
            bool showChoice = true;

            // Don't create choice instance if choice doesn't pass conditional
            if (choice.hasCondition) {
                var conditionValue = PopEvaluationStack ();
                if (!IsTruthy (conditionValue)) {
                    showChoice = false;
                }
            }

            string startText = "";
            string choiceOnlyText = "";

            if (choice.hasChoiceOnlyContent) {
                var choiceOnlyLitStr = PopEvaluationStack () as LiteralString;
                choiceOnlyText = choiceOnlyLitStr.value;
            }

            if (choice.hasStartContent) {
                var startLitStr = PopEvaluationStack () as LiteralString;
                startText = startLitStr.value;
            }

            // Don't create choice instance if player has already read this content
            if (choice.onceOnly) {
                var choiceTargetContainer = ClosestContainerToObject (choice.choiceTarget);
                var visitCount = VisitCountForContainer (choiceTargetContainer);
                if (visitCount > 0) {
                    showChoice = false;
                }
            }
                
            var choiceInstance = new ChoiceInstance (choice);
            choiceInstance.hasBeenChosen = false;
            choiceInstance.threadAtGeneration = _callStack.currentThread.Copy ();

            // We go through the full process of creating the choice above so
            // that we consume the content for it, since otherwise it'll
            // be shown on the output stream.
            if (!showChoice) {
                return null;
            }

            // Set final text for the choice instance
            choiceInstance.choiceText = startText + choiceOnlyText;

            return choiceInstance;
        }

        // Does the expression result represented by this object evaluate to true?
        // e.g. is it a Number that's not equal to 1?
        bool IsTruthy(Runtime.Object obj)
        {
            bool truthy = false;
            if (obj is Literal) {
                var literal = (Literal)obj;

                if (literal is LiteralDivertTarget) {
                    var divTarget = (LiteralDivertTarget)literal;
                    Error ("Shouldn't use a divert target (to " + divTarget.targetPath + ") as a conditional value. Did you intend a function call 'likeThis()' or a read count check 'likeThis'? (no arrows)");
                    return false;
                }

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
        private bool PerformLogicAndFlowControl(Runtime.Object contentObj, out bool endFlow)
        {
            endFlow = false;

            if( contentObj == null ) {
                return false;
            }

            // Divert
            if (contentObj is Divert) {
                
                Divert currentDivert = (Divert)contentObj;
                if (currentDivert.hasVariableTarget) {
                    var varName = currentDivert.variableDivertName;

                    var varContents = _variablesState.GetVariableWithName (varName);

                    if (!(varContents is LiteralDivertTarget)) {

                        var intContent = varContents as LiteralInt;

                        string errorMessage = "Tried to divert to a target from a variable, but the variable (" + varName + ") didn't contain a divert target, it ";
                        if (intContent && intContent.value == 0) {
                            errorMessage += "was empty/null (the value 0).";
                        } else {
                            errorMessage += "contained '" + varContents + "'.";
                        }

                        Error (errorMessage);
                    }

                    var target = (LiteralDivertTarget)varContents;
                    _divertedPath = target.targetPath;

                } else if (currentDivert.isExternal) {
                    CallExternalFunction (currentDivert.targetPathString, currentDivert.externalArgs);
                    return true;
                } else {
                    _divertedPath = currentDivert.targetPath;
                }

                if (currentDivert.pushesToStack) {
                    _callStack.Push (currentDivert.stackPushType);
                }

                if (_divertedPath == null && !currentDivert.isExternal) {

                    // Human readable name available - runtime divert is part of a hard-written divert that to missing content
                    if (currentDivert && currentDivert.debugMetadata.sourceName != null) {
                        Error ("Divert target doesn't exist: " + currentDivert.debugMetadata.sourceName);
                    } else {
                        Error ("Divert resolution failed: " + currentDivert);
                    }
                }

                return true;
            } 

            // Branch (conditional divert)
            else if (contentObj is Branch) {
                var branch = (Branch)contentObj;
                var conditionValue = PopEvaluationStack ();

                if (IsTruthy (conditionValue))
                    _divertedPath = branch.trueDivert.targetPath;
                else if (branch.falseDivert)
                    _divertedPath = branch.falseDivert.targetPath;
                
                return true;
            } 

            else if (contentObj is Pop) {
                
                var pop = (Pop) contentObj;
                if (_callStack.currentElement.type != pop.type || !_callStack.canPop) {

                    var names = new Dictionary<PushPopType, string> ();
                    names [PushPopType.Function] = "function return statement (~ ~ ~)";
                    names [PushPopType.Tunnel] = "tunnel onwards statement (->->)";

                    string expected = names [_callStack.currentElement.type];
                    if (!_callStack.canPop) {
                        expected = "end of flow (-> END or choice)";
                    }

                    var errorMsg = string.Format ("Found {0}, when expected {1}", names [pop.type], expected);

                    Error (errorMsg);
                } 

                else {
                    _callStack.Pop ();
                }

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

                            PushToOutputStream (text);
                        }

                    }
                    break;

                case ControlCommand.CommandType.NoOp:
                    break;

                case ControlCommand.CommandType.Duplicate:
                    PushEvaluationStack (PeekEvaluationStack ());
                    break;

                case ControlCommand.CommandType.PopEvaluatedValue:
                    PopEvaluationStack ();
                    break;

                case ControlCommand.CommandType.BeginString:
                    outputStream.Add (evalCommand);

                    Assert (inExpressionEvaluation == true, "Expected to be in an expression when evaluating a string");
                    inExpressionEvaluation = false;
                    break;

                case ControlCommand.CommandType.EndString:

                    var currentOutput = CurrentOutput ();

                    // Since we're iterating backward through the content,
                    // build a stack so that when we build the string,
                    // it's in the right order
                    var contentStackForString = new Stack<Runtime.Object> ();

                    int outputCountConsumed = 0;
                    for (int i = currentOutput.Count - 1; i >= 0; --i) {
                        var obj = currentOutput [i];

                        outputCountConsumed++;

                        var command = obj as ControlCommand;
                        if (command != null && command.commandType == ControlCommand.CommandType.BeginString) {
                            break;
                        }

                        if( obj is Text )
                            contentStackForString.Push (obj);
                    }

                    // Consume the content that was produced for this string
                    outputStream.RemoveRange (outputStream.Count - outputCountConsumed, outputCountConsumed);

                    // Build string out of the content we collected
                    var sb = new StringBuilder ();
                    foreach (var c in contentStackForString) {
                        sb.Append (c.ToString ());
                    }

                    // Return to expression evaluation (from content mode)
                    inExpressionEvaluation = true;
                    PushEvaluationStack (new LiteralString (sb.ToString ()));
                    break;

                case ControlCommand.CommandType.ChoiceCount:
                    var choiceCount = currentChoices.Count;
                    PushEvaluationStack (new Runtime.LiteralInt (choiceCount));
                    break;

                case ControlCommand.CommandType.TurnsSince:
                    var target = PopEvaluationStack();
                    if( !(target is LiteralDivertTarget) ) {
                        string extraNote = "";
                        if( target is LiteralInt )
                            extraNote = ". Did you accidentally pass a read count ('knot_name') instead of a target ('-> knot_name')?";
                        Error("TURNS_SINCE expected a divert target (knot, stitch, label name), but saw "+target+extraNote);
                        break;
                    }
                        
                    var divertTarget = target as LiteralDivertTarget;
                    var container = ContentAtPath (divertTarget.targetPath) as Container;
                    int turnCount = TurnsSinceForContainer (container);
                    PushEvaluationStack (new LiteralInt (turnCount));
                    break;

                case ControlCommand.CommandType.VisitIndex:
                    var currentContainer = ClosestContainerAtPath (currentPath);
                    var count = VisitCountForContainer(currentContainer) - 1; // index not count
                    PushEvaluationStack (new LiteralInt (count));
                    break;

                case ControlCommand.CommandType.SequenceShuffleIndex:
                    var shuffleIndex = NextSequenceShuffleIndex ();
                    PushEvaluationStack (new LiteralInt (shuffleIndex));
                    break;

                case ControlCommand.CommandType.StartThread:
                    // Handled in main step function
                    break;

                case ControlCommand.CommandType.Done:
                    
                    // We may exist in the context of the initial
                    // act of creating the thread, or in the context of
                    // evaluating the content.
                    if (_callStack.canPopThread) {
                        _callStack.PopThread ();
                    } 

                    // In normal flow - allow safe exit without warning
                    else {
                        _didSafeExit = true;
                    }

                    break;

                case ControlCommand.CommandType.End:
                    endFlow = true;
                    _didSafeExit = true;
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
                //var prioritiseHigherInCallStack = _temporaryEvaluationContainer != null;

                _variablesState.Assign (varAss, assignedVal);

                return true;
            }

            // Variable reference
            else if( contentObj is VariableReference ) {
                var varRef = (VariableReference)contentObj;
                Runtime.Object foundValue = null;


                // Explicit literal read count
                if (varRef.pathForCount != null) {

                    var container = varRef.containerForCount;
                    int count = VisitCountForContainer (container);
                    foundValue = new LiteralInt (count);
                }

                // Normal variable reference
                else {

                    foundValue = _variablesState.GetVariableWithName (varRef.name);

                    if (foundValue == null) {
                        Error("Uninitialised variable: " + varRef.name);
                        foundValue = new LiteralInt (0);
                    }
                }

                _evaluationStack.Add( foundValue );

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
        // and if it is then it wouldn't have a ChoiceInstance to mark where
        // the last chunk of content ended
        internal void ContinueFromPath(Path path, bool addChoiceMarker = true)
		{
            if (addChoiceMarker) {
                var choiceMarker = new ChoiceInstance (null);
                choiceMarker.hasBeenChosen = true;
                outputStream.Add (choiceMarker);
            }

            _previousPath = currentPath;

			currentPath = path;
			Continue ();
		}

		public void ContinueWithChoiceIndex(int choiceIdx)
		{
            var choiceInstances = CurrentOutput<ChoiceInstance> (c => !c.choice.isInvisibleDefault);
            Assert (choiceIdx >= 0 && choiceIdx < choiceInstances.Count, "choice out of range");

            // Replace callstack with the one from the thread at the choosing point, 
            // so that we can jump into the right place in the flow.
            // This is important in case the flow was forked by a new thread, which
            // can create multiple leading edges for the story, each of
            // which has its own context.
            var instanceToChoose = choiceInstances [choiceIdx];
            _callStack.currentThread = instanceToChoose.threadAtGeneration;

            // Create new instance as marker
            var chosenMarker = new ChoiceInstance (instanceToChoose.choice);
            chosenMarker.hasBeenChosen = true;
            outputStream.Add (chosenMarker);

            ContinueFromPath (chosenMarker.choice.choiceTarget.path, addChoiceMarker:false);
		}

        internal Runtime.Object EvaluateExpression(Runtime.Container exprContainer)
        {
            int startCallStackHeight = _callStack.elements.Count;

            _callStack.Push (PushPopType.Tunnel);

            _temporaryEvaluationContainer = exprContainer;

            currentPath = Path.ToFirstElement ();

            int evalStackHeight = _evaluationStack.Count;

            Continue ();

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

        internal void CallExternalFunction(string funcName, int numberOfArguments)
        {
            ExternalFunction func = null;
            var foundExternal = _externals.TryGetValue (funcName, out func);
            Assert (foundExternal, "Trying to call EXTERNAL function '" + funcName + "' which has not been bound.");

            // Pop arguments
            var arguments = new List<object>();
            for (int i = 0; i < numberOfArguments; ++i) {
                var poppedObj = PopEvaluationStack () as Literal;
                arguments.Add (poppedObj.valueObject);
            }

            // Convert return value (if any) to the a type that the ink engine can use
            Runtime.Object returnObj = null;
            object funcResult = func (arguments.ToArray());
            if (funcResult != null) {
                returnObj = Literal.Create (funcResult);
                Assert (returnObj != null, "Could not create ink value from returned object of type " + funcResult.GetType());
            } else {
                returnObj = new Runtime.Void ();
            }
                
            PushEvaluationStack (returnObj);
        }

        public delegate object ExternalFunction(object[] args);

        public void BindExternalFunction(string funcName, ExternalFunction func)
        {
            _externals [funcName] = func;
        }

        void PushToOutputStream(Runtime.Object obj)
        {
            // Glue: absorbs newlines both before and after it,
            // causing two piece of inline text to stay on the same line.
            bool outputStreamEndsInGlue = false;
            int glueIdx = -1;
            if (outputStream.Count > 0) {
                outputStreamEndsInGlue = outputStream.Last () is Glue;
                glueIdx = outputStream.Count - 1;
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
                TrimNewlinesFromOutputStreamEnd ();
            }

            // Only remove an existing glue if we're definitely now
            // adding something new on top, since it's served its purpose.
            if( outputStreamEndsInGlue ) 
                outputStream.RemoveAt (glueIdx);
            
            outputStream.Add(obj);
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
            for (int i = outputStream.Count - 1; i >= 0; --i) {

                var outputObj = outputStream [i];
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

                int entireObjCountToRemove = outputStream.Count - firstEntireObjToRemove;
                if (entireObjCountToRemove > 0) {
                    outputStream.RemoveRange (firstEntireObjToRemove, entireObjCountToRemove);
                }

                if (lastNewlineCharIdx > 0) {
                    Text textToTrim = (Text)outputStream [lastNewlineObjIdx];
                    textToTrim.text = textToTrim.text.Substring (0, lastNewlineCharIdx);
                }
            }


        }

        bool outputStreamEndsInNewline {
            get {
                if (outputStream.Count > 0) {
                    var text = outputStream.Last () as Text;
                    if (text) {
                        return text.text == "\n";
                    }
                }

                return false;
            }
        }

        void PushEvaluationStack(Runtime.Object obj)
        {
            _evaluationStack.Add(obj);
        }

        Runtime.Object PopEvaluationStack()
        {
            var obj = _evaluationStack.Last ();
            _evaluationStack.RemoveAt (_evaluationStack.Count - 1);
            return obj;
        }

        Runtime.Object PeekEvaluationStack()
        {
            return _evaluationStack.Last ();
        }

        List<Runtime.Object> PopEvaluationStack(int numberOfObjects)
        {
            Assert (numberOfObjects <= _evaluationStack.Count, "trying to pop too many objects");
            var popped = _evaluationStack.GetRange (_evaluationStack.Count - numberOfObjects, numberOfObjects);
            _evaluationStack.RemoveRange (_evaluationStack.Count - numberOfObjects, numberOfObjects);
            return popped;
        }
			
        public List<T> CurrentOutput<T>(Func<T, bool> optionalQuery = null) where T : class
		{
			List<T> result = new List<T> ();

			for (int i = outputStream.Count - 1; i >= 0; --i) {
				object outputObj = outputStream [i];

				// "Current" is defined as "since last chosen choice"
                var chosenInstance = outputObj as ChoiceInstance;
                if (chosenInstance && chosenInstance.hasBeenChosen) {
					break;
				}

				T outputOfType = outputObj as T;
				if (outputOfType != null) {

                    if (optionalQuery == null || optionalQuery(outputOfType) == true) {
                        
                        // Insert rather than Add since we're iterating in reverse
                        result.Insert (0, outputOfType);
                    }
				}
			}

			return result;
		}

        public List<Runtime.Object> CurrentOutput(Func<bool> optionalQuery = null)
        {
            return CurrentOutput<Runtime.Object> ();
        }

        public virtual string BuildStringOfHierarchy()
        {
            var sb = new StringBuilder ();

            Runtime.Object currentObj = null;
            if (currentPath != null) {
                currentObj = ContentAtPath (currentPath);
            }
            mainContentContainer.BuildStringOfHierarchy (sb, 0, currentObj);

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
                if (ContentAtPath (currentPath)) {
                    return;
                }
				
                // Otherwise, if diverted location doesn't have valid content,
                // drop down and attempt to increment.
                // This can happen if the diverted path is intentionally jumping
                // to the end of a container - e.g. a Conditional that's re-joining
			}

			// Can we increment successfully?
			currentPath = mainContentContainer.IncrementPath (currentPath);

            // Ran out of content? Try to auto-exit from a function,
            // or finish evaluating the content of a thread
			if (currentPath == null) {

                bool didPop = false;

                if (_callStack.CanPop (PushPopType.Function)) {
                    
                    // Pop from the call stack
                    _callStack.Pop (PushPopType.Function);

                    // This pop was due to dropping off the end of a function that didn't return anything,
                    // so in this case, we make sure that the evaluator has something to chomp on if it needs it
                    if (inExpressionEvaluation) {
                        PushEvaluationStack (new Runtime.Void ());
                    }

                    didPop = true;

                } 

                else if (_callStack.canPopThread) {
                    _callStack.PopThread ();

                    didPop = true;
                }

                // Step past the point where we last called out
                if (didPop && currentPath != null) {
                    NextContent ();
                }
			}
		}
            
        bool TryFollowDefaultInvisibleChoice()
        {
            var allChoices = CurrentOutput<ChoiceInstance> ();
            var invisibleChoiceInstances = allChoices.Where (c => c.choice.isInvisibleDefault).ToList();
            if (invisibleChoiceInstances.Count == 0 || allChoices.Count > invisibleChoiceInstances.Count)
                return false;

            // Silently consume the invisible choice so that it doesn't
            // get used twice in the same call to Continue
            var choiceInstance = invisibleChoiceInstances [0];
            outputStream.Remove (choiceInstance);

            currentPath = choiceInstance.choice.choiceTarget.path;

            return true;
        }

        void IncrementVisitCountForActiveContainers (Object currentContentObj)
        {
            // Find all open containers (runtime version of knots and stitches) across
            // all stack elements within the current thread.
            // We will then see which ones are new compared to last step.
            var openContainersThisStep = new HashSet<Container> ();
            foreach (CallStack.Element el in _callStack.elements) {
                var callstackElementCurrentObject = ContentAtPath (el.path);

                var ancestor = callstackElementCurrentObject;
                while (ancestor) {
                    var c = ancestor as Container;
                    if (c != null && (c.visitsShouldBeCounted || c.turnIndexShouldBeCounted)) {

                        bool shouldCount = false;

                        // Knots and stitches are "full" containers - any entry to them, even
                        // half way through via a labelled choice or gather count as a visit.
                        // By contrast, gathers and choices only count as being visited
                        // if you enter them at the start. This is mainly for directing
                        // to the nested content - the choice or gather point isn't counted
                        // as having been visited if you've seen a nested choice for example.
                        if (c.countingAtStartOnly) {
                            shouldCount = el.path.Equals( c.pathToFirstLeafContent );
                        } else {
                            shouldCount = true;
                        }

                        if (shouldCount) {
                            openContainersThisStep.Add ((Container)ancestor);
                        }

                    }
                    
                    ancestor = ancestor.parent;
                }
            }

            // Ask thread which containers are new, and increment read / turn counts for those.
            var newlyOpenContainers = _callStack.currentThread.UpdateOpenContainers (openContainersThisStep);

            foreach (var c in newlyOpenContainers) {
                if( c.visitsShouldBeCounted )
                    IncrementVisitCountForContainer (c);
                if (c.turnIndexShouldBeCounted)
                    RecordTurnIndexVisitToContainer (c);
            }
        }

        int VisitCountForContainer(Container container)
        {
            if( !container.visitsShouldBeCounted ) {
                Error ("Read count for target ("+container.name+" - on "+container.debugMetadata+") unknown. The story may need to be compiled with countAllVisits flag (-c).");
                return 0;
            }

            int count = 0;
            var containerPathStr = container.path.ToString();
            _visitCounts.TryGetValue (containerPathStr, out count);
            return count;
        }

        void IncrementVisitCountForContainer(Container container)
        {
            int count = 0;
            var containerPathStr = container.path.ToString();
            _visitCounts.TryGetValue (containerPathStr, out count);
            count++;
            _visitCounts [containerPathStr] = count;
        }

        void RecordTurnIndexVisitToContainer(Container container)
        {
            var containerPathStr = container.path.ToString();
            _turnIndices [containerPathStr] = _currentTurnIndex;
        }

        int TurnsSinceForContainer(Container container)
        {
            if( !container.turnIndexShouldBeCounted ) {
                Error ("TURNS_SINCE() for target ("+container.name+" - on "+container.debugMetadata+") unknown. The story may need to be compiled with countAllVisits flag (-c).");
            }

            int index = 0;
            var containerPathStr = container.path.ToString();
            if (_turnIndices.TryGetValue (containerPathStr, out index)) {
                return _currentTurnIndex - index;
            } else {
                return -1;
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

            var seqCount = VisitCountForContainer (seqContainer);
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
            return ClosestContainerToObject (content);
        }

        Runtime.Container ClosestContainerToObject(Runtime.Object content)
        {
            while (content && !(content is Container)) {
                content = content.parent;
            }
            return (Runtime.Container) content;
        }

        // Throw an exception that gets caught and causes AddError to be called,
        // then exits the flow.
        void Error(string message, bool useEndLineNumber = false)
        {
            var e = new StoryException (message);
            e.useEndLineNumber = useEndLineNumber;
            throw e;
        }

        void AddError (string message, bool useEndLineNumber)
        {
            var dm = currentDebugMetadata;

            if (dm != null) {
                int lineNum = useEndLineNumber ? dm.endLineNumber : dm.startLineNumber;
                message = string.Format ("Runtime error in {0} line {1}: {2}", dm.fileName, lineNum, message);
            }
            else {
                message = "Runtime error" + ": " + message;
            }

            // TODO: Could just add to output?
            if (_currentErrors == null) {
                _currentErrors = new List<string> ();
            }

            _currentErrors.Add (message);
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
                if (currentObj) {
                    var dm = currentObj.debugMetadata;
                    if (dm != null) {
                        return dm;
                    }
                }
            }

            return null;
        }

        Container mainContentContainer {
            get {
                if (_temporaryEvaluationContainer) {
                    return _temporaryEvaluationContainer;
                } else {
                    return _mainContentContainer;
                }
            }
        }

        [JsonProperty]
        private Container _mainContentContainer;

        Dictionary<string, ExternalFunction> _externals;

        private Container _temporaryEvaluationContainer;
        private Path _divertedPath;
        private bool _didSafeExit;
            
        private CallStack _callStack;
        private VariablesState _variablesState;

        private Dictionary<string, int> _visitCounts;
        private Dictionary<string, int> _turnIndices;
        private int _currentTurnIndex;
        private int _storySeed;

        private List<Runtime.Object> _evaluationStack;

        private List<string> _currentErrors;

        private Path _previousPath;

        // Keep track of the current set of containers up the nested chain,
        // so that as we move between the containers, we know which ones are
        // being newly visited, and therefore increment their visit counts.
        //private HashSet<Container> _openContainers;

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

