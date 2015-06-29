using System.Collections.Generic;

namespace Inklewriter.Parsed
{
	// Base class for Knots and Stitches
    public abstract class FlowBase : Parsed.Object, INamedContent
	{
		public string name { get; set; }
        public List<string> parameterNames { get; protected set; }
        public bool hasParameters { get { return parameterNames != null && parameterNames.Count > 0; } }
        public Dictionary<string, VariableAssignment> variableDeclarations;
        public abstract FlowLevel flowLevel { get; }

        public FlowBase (string name = null, List<Parsed.Object> topLevelObjects = null, List<string> parameterNames = null)
		{
			this.name = name;

			if (topLevelObjects == null) {
				topLevelObjects = new List<Parsed.Object> ();
			}

            // Used by story to add includes
            PreProcessTopLevelObjects (topLevelObjects);

            topLevelObjects = SplitWeaveAndSubFlowContent (topLevelObjects);

            AddContent(topLevelObjects);

            this.parameterNames = parameterNames;

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
                    if (varDecl != null && varDecl.isNewDeclaration) {
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
                if (subFlow != null) {
                    subFlowObjs.Add (obj);
                    _subFlowsByName [subFlow.name] = subFlow;
                } else {
                    weaveObjs.Add (obj);
                }
            }

            // Will step into the own content of
            bool willStepStraightIntoSubFlow = subFlowObjs.Count > 0 && weaveObjs.Count == 0;

            // Add error if runtime gets to the end of content without a divert/return etc
            if (!(this is Story) && !willStepStraightIntoSubFlow) {
                

                var lastWeaveObj = weaveObjs [weaveObjs.Count - 1];
                if (!(lastWeaveObj is Parsed.Return)) {

                    var runtimeError = new Runtime.Error ("unexpectedly reached end of content. Do you need a '~ done' or '~ return'?");
                    var lineNumber = lastWeaveObj.debugMetadata.endLineNumber;

                    // Steal debug metadata from the last content line 
                    // of this FlowBase since the *lack* of content doesn't
                    // have a line number!
                    var dm = new Runtime.DebugMetadata ();
                    dm.startLineNumber = lineNumber;
                    dm.endLineNumber = lineNumber;
                    dm.fileName = lastWeaveObj.debugMetadata.fileName;

                    var wrappedError = new Parsed.Wrap<Runtime.Error> (runtimeError);
                    wrappedError.debugMetadata = dm;
                    weaveObjs.Add (wrappedError);
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
            while (ancestor != null) {

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

        public Parsed.Object ResolveTargetForReadCountWithName(string name, Parsed.Object fromNode)
        {
            if (fromNode == null) {
                fromNode = this;
            }

            var ancestor = fromNode;
            while (ancestor != null) {

                if (ancestor is FlowBase) {
                    var ancestorFlow = (FlowBase)ancestor;

                    var content = ancestorFlow.ContentWithNameAtLevel (name);
                    if (content != null) {
                        return content;
                    }
                }

                ancestor = ancestor.parent;
            }

            return null;
        }

        public bool HasVariableWithName(string varName)
        {
            if (variableDeclarations.ContainsKey (varName))
                return true;

            if (this.parameterNames != null && this.parameterNames.Contains (varName))
                return true;

            return false;
        }
            
        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();
            container.name = name;
            container.visitsShouldBeCounted = true;

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
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];

                // Inner knots and stitches
                if (obj is FlowBase) {

                    var childFlow = (FlowBase)obj;

                    // First inner knot/stitch - automatically step into it
                    if (contentIdx == 0 && !childFlow.hasParameters) {
                        container.AddContent (childFlow.runtimeObject);
                    } 

                    // All other knots/stitches are only accessible by name:
                    // i.e. by explicit divert
                    else {
                        container.AddToNamedContentOnly ((Runtime.INamedContent) childFlow.runtimeObject);
                    }
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
            if (_rootWeave != null && _rootWeave.looseEnds != null && _rootWeave.looseEnds.Count > 0) {

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
            if (this.parameterNames == null || this.parameterNames.Count == 0) {
                return;
            }

            // Assign parameters in reverse since they'll be popped off the evaluation stack
            // No need to generate EvalStart and EvalEnd since there's nothing being pushed
            // back onto the evaluation stack.
            for (int i = parameterNames.Count - 1; i >= 0; --i) {
                var paramName = parameterNames [i];

                var assign = new Runtime.VariableAssignment (paramName, isNewDeclaration:true);
                container.AddContent (assign);
            }
        }
            
        public Parsed.Object ContentWithNameAtLevel(string name, FlowLevel? levelType = null)
        {
            if ( levelType == FlowLevel.WeavePoint || levelType == null ) {
                
                Parsed.Object weavePointResult = null;

                if (_rootWeave != null) {
                    weavePointResult = (Parsed.Object)_rootWeave.WeavePointNamed (name);
                    if (weavePointResult != null)
                        return weavePointResult;
                }

                // Stop now if we only wanted a result if it's a weave point?
                if (levelType == FlowLevel.WeavePoint)
                    return null;
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

            return null;
        }

        public override void ResolveReferences (Story context)
        {
            if (_finalLooseEndTarget != null) {
                var flowEndPath = _finalLooseEndTarget.path;
                foreach (var finalLooseEndDivert in _finalLooseEnds) {
                    finalLooseEndDivert.targetPath = flowEndPath;
                }
            }

            base.ResolveReferences(context);
        }

        Weave _rootWeave;
        Dictionary<string, FlowBase> _subFlowsByName;
        List<Runtime.Divert> _finalLooseEnds;
        Runtime.Object _finalLooseEndTarget;
            
	}
}

