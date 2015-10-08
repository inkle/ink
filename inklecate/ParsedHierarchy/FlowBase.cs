using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	// Base class for Knots and Stitches
    internal abstract class FlowBase : Parsed.Object, INamedContent
	{
        internal class Argument
        {
            public string name;
            public bool isByReference;
        }

		public string name { get; set; }
        public List<Argument> arguments { get; protected set; }
        public bool hasParameters { get { return arguments != null && arguments.Count > 0; } }
        public Dictionary<string, VariableAssignment> variableDeclarations;
        public abstract FlowLevel flowLevel { get; }
        public bool isFunction { get; protected set; }

        public FlowBase (string name = null, List<Parsed.Object> topLevelObjects = null, List<Argument> arguments = null, bool isFunction = false)
		{
			this.name = name;

			if (topLevelObjects == null) {
				topLevelObjects = new List<Parsed.Object> ();
			}

            // Used by story to add includes
            PreProcessTopLevelObjects (topLevelObjects);

            topLevelObjects = SplitWeaveAndSubFlowContent (topLevelObjects);

            AddContent(topLevelObjects);

            this.arguments = arguments;
            this.isFunction = isFunction;

            variableDeclarations = new Dictionary<string, VariableAssignment> ();

            TryAddNewVariableDeclarationsFrom (this);
		}

        void TryAddNewVariableDeclarationsFrom(Parsed.Object inObject)
        {
            if (inObject.content == null)
                return;

            foreach (var obj in inObject.content) {

                if (obj is VariableAssignment) {
                    var varDecl = (VariableAssignment)obj;
                    if (varDecl && varDecl.isNewDeclaration) {
                        TryAddNewVariableDeclaration (varDecl);
                    }
                } 

                // Other FlowBases handle their own declarations
                else if (obj is FlowBase) {
                    continue;
                } 

                // Recursive search into other objects (weaves, conditionals, etc)
                else {
                    TryAddNewVariableDeclarationsFrom (obj);
                }
            }
        }


        List<Parsed.Object> SplitWeaveAndSubFlowContent(List<Parsed.Object> contentObjs)
        {
            var weaveObjs = new List<Parsed.Object> ();
            var subFlowObjs = new List<Parsed.Object> ();

            _subFlowsByName = new Dictionary<string, FlowBase> ();

            foreach (var obj in contentObjs) {

                var subFlow = obj as FlowBase;
                if (subFlow) {
                    subFlowObjs.Add (obj);
                    _subFlowsByName [subFlow.name] = subFlow;
                } else {
                    weaveObjs.Add (obj);
                }
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

        public void TryAddNewVariableDeclaration(VariableAssignment varDecl)
        {
            if (variableDeclarations.ContainsKey (varDecl.variableName)) {
                Error("found declaration variable '"+varDecl.variableName+"' that was already declared", varDecl);

                var debugMetadata = variableDeclarations [varDecl.variableName].debugMetadata;
                if (debugMetadata != null) {
                    Error ("(previous declaration: " + debugMetadata + ")");
                }
            }

            variableDeclarations [varDecl.variableName] = varDecl;
        }

        protected virtual void PreProcessTopLevelObjects(List<Parsed.Object> topLevelObjects)
        {
            // empty by default, used by Story to process included file references
        }

        public bool ResolveVariableWithName(string varName, Parsed.Object fromNode)
        {
            if (fromNode == null) {
                fromNode = this;
            }

            var ancestor = fromNode;
            while (ancestor) {

                if (ancestor is FlowBase) {
                    var ancestorFlow = (FlowBase)ancestor;

                    if( ancestorFlow.HasVariableWithName(varName) ) {
                        return true;
                    }
                }

                ancestor = ancestor.parent;
            }
                
            return false;
        }
            
        public bool HasVariableWithName(string varName)
        {
            if (variableDeclarations.ContainsKey (varName))
                return true;

            if (arguments != null ) {
                foreach (var arg in arguments) {
                    if( arg.name.Equals(varName) ) return true;
                }
            }

            return false;
        }
            
        public override Runtime.Object GenerateRuntimeObject ()
        {
            if (isFunction) {
                CheckForDisallowedFunctionFlowControl ();
            }

            var container = new Runtime.Container ();
            container.name = name;

            if (this.story.countAllVisits) {
                container.visitsShouldBeCounted = true;
                container.beatIndexShouldBeCounted = true;
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

                    // First inner knot/stitch - automatically step into it
                    if (contentIdx == 0 && !childFlow.hasParameters) {
                        _startingSubFlowDivert = new Runtime.Divert ();
                        container.AddContent(_startingSubFlowDivert);
                        _startingSubFlowRuntime = childFlowRuntime;
                    }
                        
                    container.AddToNamedContentOnly ((Runtime.INamedContent) childFlowRuntime);
                }

                // Other content (including entire Weaves that were grouped in the constructor)
                // At the time of writing, all FlowBases have a maximum of one piece of "other content"
                // and it's always the root Weave
                else {
                    container.AddContent (obj.runtimeObject);
                }

                contentIdx++;
            }

            // Tie up final loose ends to the very end
            if (_rootWeave && _rootWeave.looseEnds != null && _rootWeave.looseEnds.Count > 0) {

                foreach (var looseEnd in _rootWeave.looseEnds) {
                    if (looseEnd is Divert) {
                        
                        if (_finalLooseEnds == null) {
                            _finalLooseEnds = new List<Inklewriter.Runtime.Divert> ();
                            _finalLooseEndTarget = Runtime.ControlCommand.NoOp ();
                            container.AddContent (_finalLooseEndTarget);
                        }

                        _finalLooseEnds.Add ((Runtime.Divert)looseEnd.runtimeObject);
                    }
                }
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
                var paramName = arguments [i].name;

                var assign = new Runtime.VariableAssignment (paramName, isNewDeclaration:true);
                container.AddContent (assign);
            }
        }
            
        public Parsed.Object ContentWithNameAtLevel(string name, FlowLevel? levelType = null, bool deepSearch = false)
        {
            if ( levelType == FlowLevel.WeavePoint || levelType == null ) {
                
                Parsed.Object weavePointResult = null;

                if (_rootWeave) {
                    weavePointResult = (Parsed.Object)_rootWeave.WeavePointNamed (name);
                    if (weavePointResult)
                        return weavePointResult;
                }

                // Stop now if we only wanted a result if it's a weave point?
                if (levelType == FlowLevel.WeavePoint)
                    return deepSearch ? DeepSearchForAnyLevelContent(name) : null;
            }

            // If this flow would be incapable of containing the requested level, early out
            // (e.g. asking for a Knot from a Stitch)
            if (levelType != null && levelType < this.flowLevel)
                return null;

            FlowBase subFlow = null;

            if (_subFlowsByName.TryGetValue (name, out subFlow)) {
                if (levelType == null || levelType == subFlow.flowLevel)
                    return subFlow;
            }

            return deepSearch ? DeepSearchForAnyLevelContent(name) : null;
        }

        Parsed.Object DeepSearchForAnyLevelContent(string name)
        {
            foreach (var subFlowNamePair in _subFlowsByName) {
                var subFlow = subFlowNamePair.Value;
                var deepResult = subFlow.ContentWithNameAtLevel (name, levelType:null, deepSearch: true);
                if (deepResult)
                    return deepResult;
            }

            return null;
        }

        public override void ResolveReferences (Story context)
        {
            if (_finalLooseEndTarget) {
                var flowEndPath = _finalLooseEndTarget.path;
                foreach (var finalLooseEndDivert in _finalLooseEnds) {
                    finalLooseEndDivert.targetPath = flowEndPath;
                }
            }

            if (_startingSubFlowDivert) {
                _startingSubFlowDivert.targetPath = _startingSubFlowRuntime.path;
            }

            base.ResolveReferences(context);
        }

        void CheckForDisallowedFunctionFlowControl()
        {
            if (!(this is Knot)) {
                Error ("Functions cannot be stitches - i.e. they should be defined as '== function myFunc ==' rather than internal to another knot.");
            }

            // Not allowed sub-flows
            foreach (var subFlowAndName in _subFlowsByName) {
                var name = subFlowAndName.Key;
                var subFlow = subFlowAndName.Value;
                Error ("Functions may not contain stitches, but saw '"+name+"' within the function '"+this.name+"'", subFlow);
            }

            var allDiverts = _rootWeave.FindAll<Divert> ();
            foreach (var divert in allDiverts) {
                if( !divert.isFunctionCall )
                    Error ("Functions may not contain diverts, but saw '"+divert.ToString()+"'", divert);
            }

            var allChoices = _rootWeave.FindAll<Choice> ();
            foreach (var choice in allChoices) {
                Error ("Functions may not contain choices, but saw '"+choice.ToString()+"'", choice);
            }
        }

        Weave _rootWeave;
        Dictionary<string, FlowBase> _subFlowsByName;
        List<Runtime.Divert> _finalLooseEnds;
        Runtime.Divert _startingSubFlowDivert;
        Runtime.Object _startingSubFlowRuntime;
        Runtime.Object _finalLooseEndTarget;
            
	}
}

