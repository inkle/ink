using System.Collections.Generic;

namespace Ink.Parsed
{
	// Base class for Knots and Stitches
    public abstract class FlowBase : Parsed.Object, INamedContent
	{
        public class Argument
        {
            public Identifier identifier;
            public bool isByReference;
            public bool isDivertTarget;
        }

        public string name
        {
            get { return identifier?.name; }
        }
        public Identifier identifier { get; set; }
        public List<Argument> arguments { get; protected set; }
        public bool hasParameters { get { return arguments != null && arguments.Count > 0; } }
        public Dictionary<string, VariableAssignment> variableDeclarations;

        public abstract FlowLevel flowLevel { get; }
        public bool isFunction { get; protected set; }

        public FlowBase (Identifier name = null, List<Parsed.Object> topLevelObjects = null, List<Argument> arguments = null, bool isFunction = false, bool isIncludedStory = false)
		{
			this.identifier = name;

			if (topLevelObjects == null) {
				topLevelObjects = new List<Parsed.Object> ();
			}

            // Used by story to add includes
            PreProcessTopLevelObjects (topLevelObjects);

            topLevelObjects = SplitWeaveAndSubFlowContent (topLevelObjects, isRootStory:this is Story && !isIncludedStory);

            AddContent(topLevelObjects);

            this.arguments = arguments;
            this.isFunction = isFunction;
            this.variableDeclarations = new Dictionary<string, VariableAssignment> ();
		}

        List<Parsed.Object> SplitWeaveAndSubFlowContent(List<Parsed.Object> contentObjs, bool isRootStory)
        {
            var weaveObjs = new List<Parsed.Object> ();
            var subFlowObjs = new List<Parsed.Object> ();

            _subFlowsByName = new Dictionary<string, FlowBase> ();

            foreach (var obj in contentObjs) {

                var subFlow = obj as FlowBase;
                if (subFlow) {
                    if (_firstChildFlow == null)
                        _firstChildFlow = subFlow;

                    subFlowObjs.Add (obj);
                    _subFlowsByName [subFlow.identifier?.name] = subFlow;
                } else {
                    weaveObjs.Add (obj);
                }
            }

            // Implicit final gather in top level story for ending without warning that you run out of content
            if (isRootStory) {
            	weaveObjs.Add (new Gather (null, 1));
            	weaveObjs.Add (new Divert (new Path (Identifier.Done)));
            }

            var finalContent = new List<Parsed.Object> ();

            if (weaveObjs.Count > 0) {
                _rootWeave = new Weave (weaveObjs, 0);
                finalContent.Add (_rootWeave);
            }

            if (subFlowObjs.Count > 0) {
                finalContent.AddRange (subFlowObjs);
            }

            return finalContent;
        }

        protected virtual void PreProcessTopLevelObjects(List<Parsed.Object> topLevelObjects)
        {
            // empty by default, used by Story to process included file references
        }

        public struct VariableResolveResult
        {
            public bool found;
            public bool isGlobal;
            public bool isArgument;
            public bool isTemporary;
            public FlowBase ownerFlow;
        }

        public VariableResolveResult ResolveVariableWithName(string varName, Parsed.Object fromNode)
        {
            var result = new VariableResolveResult ();

            // Search in the stitch / knot that owns the node first
            var ownerFlow = fromNode == null ? this : fromNode.ClosestFlowBase ();

            // Argument
            if (ownerFlow.arguments != null ) {
                foreach (var arg in ownerFlow.arguments) {
                    if (arg.identifier.name.Equals (varName)) {
                        result.found = true;
                        result.isArgument = true;
                        result.ownerFlow = ownerFlow;
                        return result;
                    }
                }
            }

            // Temp
            var story = this.story; // optimisation
            if (ownerFlow != story && ownerFlow.variableDeclarations.ContainsKey (varName)) {
                result.found = true;
                result.ownerFlow = ownerFlow;
                result.isTemporary = true;
                return result;
            }

            // Global
            if (story.variableDeclarations.ContainsKey (varName)) {
                result.found = true;
                result.ownerFlow = story;
                result.isGlobal = true;
                return result;
            }

            result.found = false;
            return result;
        }

        public void TryAddNewVariableDeclaration(VariableAssignment varDecl)
        {
            var varName = varDecl.variableName;
            if (variableDeclarations.ContainsKey (varName)) {

                var prevDeclError = "";
                var debugMetadata = variableDeclarations [varName].debugMetadata;
                if (debugMetadata != null) {
                    prevDeclError = " ("+variableDeclarations [varName].debugMetadata+")";
                }
                Error("found declaration variable '"+varName+"' that was already declared"+prevDeclError, varDecl, false);

                return;
            }

            variableDeclarations [varDecl.variableName] = varDecl;
        }

        public void ResolveWeavePointNaming ()
        {
            // Find all weave points and organise them by name ready for
            // diverting. Also detect naming collisions.
            if( _rootWeave )
                _rootWeave.ResolveWeavePointNaming ();

            if (_subFlowsByName != null) {
                foreach (var namedSubFlow in _subFlowsByName) {
                    namedSubFlow.Value.ResolveWeavePointNaming ();
                }
            }
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            Return foundReturn = null;
            if (isFunction) {
                CheckForDisallowedFunctionFlowControl ();
            }

            // Non-functon: Make sure knots and stitches don't attempt to use Return statement
            else if( flowLevel == FlowLevel.Knot || flowLevel == FlowLevel.Stitch ) {
                foundReturn = Find<Return> ();
                if (foundReturn != null) {
                    Error ("Return statements can only be used in knots that are declared as functions: == function " + this.identifier + " ==", foundReturn);
                }
            }

            var container = new Runtime.Container ();
            container.name = identifier?.name;

            if( this.story.countAllVisits ) {
                container.visitsShouldBeCounted = true;
            }

            GenerateArgumentVariableAssignments (container);

            // Run through content defined for this knot/stitch:
            //  - First of all, any initial content before a sub-stitch
            //    or any weave content is added to the main content container
            //  - The first inner knot/stitch is automatically entered, while
            //    the others are only accessible by an explicit divert
            //       - The exception to this rule is if the knot/stitch takes
            //         parameters, in which case it can't be auto-entered.
            //  - Any Choices and Gathers (i.e. IWeavePoint) found are
            //    processsed by GenerateFlowContent.
            int contentIdx = 0;
            while (content != null && contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];

                // Inner knots and stitches
                if (obj is FlowBase) {

                    var childFlow = (FlowBase)obj;

                    var childFlowRuntime = childFlow.runtimeObject;

                    // First inner stitch - automatically step into it
                    // 20/09/2016 - let's not auto step into knots
                    if (contentIdx == 0 && !childFlow.hasParameters
                        && this.flowLevel == FlowLevel.Knot) {
                        _startingSubFlowDivert = new Runtime.Divert ();
                        container.AddContent(_startingSubFlowDivert);
                        _startingSubFlowRuntime = childFlowRuntime;
                    }

                    // Check for duplicate knots/stitches with same name
                    var namedChild = (Runtime.INamedContent)childFlowRuntime;
                    Runtime.INamedContent existingChild = null;
                    if (container.namedContent.TryGetValue(namedChild.name, out existingChild) ) {
                        var errorMsg = string.Format ("{0} already contains flow named '{1}' (at {2})",
                            this.GetType().Name,
                            namedChild.name,
                            (existingChild as Runtime.Object).debugMetadata);

                        Error (errorMsg, childFlow);
                    }

                    container.AddToNamedContentOnly (namedChild);
                }

                // Other content (including entire Weaves that were grouped in the constructor)
                // At the time of writing, all FlowBases have a maximum of one piece of "other content"
                // and it's always the root Weave
                else {
                    container.AddContent (obj.runtimeObject);
                }

                contentIdx++;
            }

            // CHECK FOR FINAL LOOSE ENDS!
            // Notes:
            //  - Functions don't need to terminate - they just implicitly return
            //  - If return statement was found, don't continue finding warnings for missing control flow,
            // since it's likely that a return statement has been used instead of a ->-> or something,
            // or the writer failed to mark the knot as a function.
            //  - _rootWeave may be null if it's a knot that only has stitches
            if (flowLevel != FlowLevel.Story && !this.isFunction && _rootWeave != null && foundReturn == null) {
                _rootWeave.ValidateTermination (WarningInTermination);
            }

            return container;
        }

        void GenerateArgumentVariableAssignments(Runtime.Container container)
        {
            if (this.arguments == null || this.arguments.Count == 0) {
                return;
            }

            // Assign parameters in reverse since they'll be popped off the evaluation stack
            // No need to generate EvalStart and EvalEnd since there's nothing being pushed
            // back onto the evaluation stack.
            for (int i = arguments.Count - 1; i >= 0; --i) {
                var paramName = arguments [i].identifier?.name;

                var assign = new Runtime.VariableAssignment (paramName, isNewDeclaration:true);
                container.AddContent (assign);
            }
        }

        public Parsed.Object ContentWithNameAtLevel(string name, FlowLevel? level = null, bool deepSearch = false)
        {
            // Referencing self?
            if (level == this.flowLevel || level == null) {
                if (name == this.identifier?.name) {
                    return this;
                }
            }

            if ( level == FlowLevel.WeavePoint || level == null ) {

                Parsed.Object weavePointResult = null;

                if (_rootWeave) {
                    weavePointResult = (Parsed.Object)_rootWeave.WeavePointNamed (name);
                    if (weavePointResult)
                        return weavePointResult;
                }

                // Stop now if we only wanted a result if it's a weave point?
                if (level == FlowLevel.WeavePoint)
                    return deepSearch ? DeepSearchForAnyLevelContent(name) : null;
            }

            // If this flow would be incapable of containing the requested level, early out
            // (e.g. asking for a Knot from a Stitch)
            if (level != null && level < this.flowLevel)
                return null;

            FlowBase subFlow = null;

            if (_subFlowsByName.TryGetValue (name, out subFlow)) {
                if (level == null || level == subFlow.flowLevel)
                    return subFlow;
            }

            return deepSearch ? DeepSearchForAnyLevelContent(name) : null;
        }

        Parsed.Object DeepSearchForAnyLevelContent(string name)
        {
            var weaveResultSelf = ContentWithNameAtLevel (name, level:FlowLevel.WeavePoint, deepSearch: false);
            if (weaveResultSelf) {
                return weaveResultSelf;
            }

            foreach (var subFlowNamePair in _subFlowsByName) {
                var subFlow = subFlowNamePair.Value;
                var deepResult = subFlow.ContentWithNameAtLevel (name, level:null, deepSearch: true);
                if (deepResult)
                    return deepResult;
            }

            return null;
        }

        public override void ResolveReferences (Story context)
        {
            if (_startingSubFlowDivert) {
                _startingSubFlowDivert.targetPath = _startingSubFlowRuntime.path;
            }

            base.ResolveReferences(context);

            // Check validity of parameter names
            if (arguments != null) {

                foreach (var arg in arguments)
                    context.CheckForNamingCollisions (this, arg.identifier, Story.SymbolType.Arg, "argument");

                // Separately, check for duplicate arugment names, since they aren't Parsed.Objects,
                // so have to be checked independently.
                for (int i = 0; i < arguments.Count; i++) {
                    for (int j = i + 1; j < arguments.Count; j++) {
                        if (arguments [i].identifier?.name == arguments [j].identifier?.name) {
                            Error ("Multiple arguments with the same name: '" + arguments [i].identifier + "'");
                        }
                    }
                }
            }

            // Check naming collisions for knots and stitches
            if (flowLevel != FlowLevel.Story) {
                // Weave points aren't FlowBases, so this will only be knot or stitch
                var symbolType = flowLevel == FlowLevel.Knot ? Story.SymbolType.Knot : Story.SymbolType.SubFlowAndWeave;
                context.CheckForNamingCollisions (this, identifier, symbolType);
            }
        }

        void CheckForDisallowedFunctionFlowControl()
        {
            if (!(this is Knot)) {
                Error ("Functions cannot be stitches - i.e. they should be defined as '== function myFunc ==' rather than public to another knot.");
            }

            // Not allowed sub-flows
            foreach (var subFlowAndName in _subFlowsByName) {
                var name = subFlowAndName.Key;
                var subFlow = subFlowAndName.Value;
                Error ("Functions may not contain stitches, but saw '"+name+"' within the function '"+this.identifier+"'", subFlow);
            }

            var allDiverts = _rootWeave.FindAll<Divert> ();
            foreach (var divert in allDiverts) {
                if( !divert.isFunctionCall && !(divert.parent is DivertTarget) )
                    Error ("Functions may not contain diverts, but saw '"+divert.ToString()+"'", divert);
            }

            var allChoices = _rootWeave.FindAll<Choice> ();
            foreach (var choice in allChoices) {
                Error ("Functions may not contain choices, but saw '"+choice.ToString()+"'", choice);
            }
        }

        void WarningInTermination(Parsed.Object terminatingObject)
        {
            string message = "Apparent loose end exists where the flow runs out. Do you need a '-> DONE' statement, choice or divert?";
            if (terminatingObject.parent == _rootWeave && _firstChildFlow) {
                message = message + " Note that if you intend to enter '"+_firstChildFlow.identifier+"' next, you need to divert to it explicitly.";
            }

            var terminatingDivert = terminatingObject as Divert;
            if (terminatingDivert && terminatingDivert.isTunnel) {
                message = message + " When final tunnel to '"+terminatingDivert.target+" ->' returns it won't have anywhere to go.";
            }

            Warning (message, terminatingObject);
        }

        protected Dictionary<string, FlowBase> subFlowsByName {
            get {
                return _subFlowsByName;
            }
        }

        public override string typeName {
            get {
                if (isFunction) return "Function";
                else return flowLevel.ToString ();
            }
        }

        public override string ToString ()
        {
            return typeName+" '" + identifier + "'";
        }

        Weave _rootWeave;
        Dictionary<string, FlowBase> _subFlowsByName;
        Runtime.Divert _startingSubFlowDivert;
        Runtime.Object _startingSubFlowRuntime;
        FlowBase _firstChildFlow;

	}
}

