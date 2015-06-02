using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Parsed
{
	// Base class for Knots and Stitches
    public abstract class FlowBase : Parsed.Object, INamedContent
	{
		public string name { get; set; }
		public List<Parsed.Object> content { get; protected set; }
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
			this.content = topLevelObjects;

            this.parameterNames = parameterNames;

            variableDeclarations = new Dictionary<string, VariableAssignment> ();

			foreach (var child in this.content) {
				child.parent = this;

                var varDecl = child as VariableAssignment;
                if (varDecl != null && varDecl.isNewDeclaration) {
                    variableDeclarations [varDecl.variableName] = varDecl;
                }
			}
		}

        public string dotSeparatedFullName {
            get {
                if (this.parent != null) {
                    var parentFlow = (FlowBase)this.parent;
                    var parentName = parentFlow.dotSeparatedFullName;
                    if (parentName != null) {
                        return parentFlow.dotSeparatedFullName + "." + this.name;
                    }
                }

                return this.name;
            }
        }

        public virtual bool HasVariableWithName(string varName)
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

            OnRuntimeGenerationDidStart (container);

            // Maintain a list of gathered loose ends so that we can resolve
            // their divert paths in ResolveReferences
            this._allGatheredLooseEnds = new List<GatheredLooseEnd> ();

            bool foundFirstKnotStitchOrWeave = false;

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
                    if (!foundFirstKnotStitchOrWeave && !childFlow.hasParameters) {
                        container.AddContent (childFlow.runtimeObject);
                    } 

                    // All other knots/stitches are only accessible by name:
                    // i.e. by explicit divert
                    else {
                        container.AddToNamedContentOnly ((Runtime.INamedContent) childFlow.runtimeObject);
                    }

                    foundFirstKnotStitchOrWeave = true;
                }

                // Choices and Gathers: Process as blocks of weave-like content
                else if (obj is IWeavePoint) {
                    var result = GenerateWeaveBlockRuntime (ref contentIdx, indentIndex: 0);
                    container.AddContent (result.rootContainer);

                    foundFirstKnotStitchOrWeave = true;
                } 

                // Normal content (defined at start)
                else {
                    container.AddContent (obj.runtimeObject);
                }

                contentIdx++;
            }
                
            return container;
        }

        protected virtual void OnRuntimeGenerationDidStart(Runtime.Container container)
        {
            GenerateArgumentVariableAssignments (container);

            GenerateReadCountUpdate (container);
        }

        // Initially called from main GenerateRuntimeObject
        // Generate a container of content for a particular indent level.
        // Recursive for further indentation levels.
        WeaveBlockRuntimeResult GenerateWeaveBlockRuntime(ref int contentIdx, int indentIndex)
        {
            var result = new WeaveBlockRuntimeResult ();
            result.gatheredLooseEndDelegate = OnLooseEndGathered;

            // Iterate through content for the block at this level of indentation
            //  - Normal content is nested under Choices and Gathers
            //  - Blocks that are further indented cause recursion
            //  - Keep track of loose ends so that they can be diverted to Gathers
            while (contentIdx < content.Count) {
                
                Parsed.Object obj = content [contentIdx];

                // If we've now found a knot/stitch, we've overstepped,
                // since it certainly doesn't belong inside a weave block,
                // since it's a higher level construct
                if (obj is FlowBase) {
                    contentIdx--;
                    return result;
                }

                // Choice or Gather
                if (obj is IWeavePoint) {
                    var weavePoint = (IWeavePoint)obj;
                    var weaveIndentIdx = weavePoint.indentationDepth - 1;

                    // Moving to outer level indent - this block is complete
                    if (weaveIndentIdx < indentIndex) {
                        return result;
                    }

                    // Inner level indentation - recurse
                    else if (weaveIndentIdx > indentIndex) {
                        var nestedResult = GenerateWeaveBlockRuntime (ref contentIdx, weaveIndentIdx);
                        result.AddNestedBlock (nestedResult);
                        continue;
                    } 

                    result.AddWeavePoint (weavePoint);
                } 

                // Normal content
                else {
                    result.AddContent (obj.runtimeObject);
                }

                contentIdx++;
            }

            return result;
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

        protected void GenerateReadCountUpdate(Runtime.Container container)
        {
            if (name == null) {
                return;
            }

            container.AddContent (Runtime.ControlCommand.EvalStart());

            string varName = dotSeparatedFullName;
            container.AddContent (new Runtime.VariableReference (varName));
            container.AddContent (new Runtime.LiteralInt(1));
            container.AddContent (Runtime.NativeFunctionCall.CallWithName("+"));
            container.AddContent (new Runtime.VariableAssignment (varName, false));

            container.AddContent (Runtime.ControlCommand.EvalEnd());
        }

        public override void ResolveReferences (Story context)
		{
			foreach (Parsed.Object obj in content) {
				obj.ResolveReferences (context); 
			}

            if (_allGatheredLooseEnds != null) {
                foreach(GatheredLooseEnd looseEnd in _allGatheredLooseEnds) {
                    looseEnd.divert.targetPath = looseEnd.targetGather.runtimePath;
                }
            }

        }
            
        public Parsed.Object ContentWithNameAtLevel(string name, FlowLevel? levelType = null)
        {
            foreach(var obj in content) {

                var namedContent = obj as INamedContent;
                if (namedContent != null && namedContent.name == name) {

                    // No FlowLevel specified
                    if (levelType == null) {
                        return obj;
                    } 

                    // Searching for Knot/Stitch
                    else if (obj is FlowBase) {
                        var flowContent = (FlowBase)obj;
                        if (flowContent.flowLevel == levelType) {
                            return obj;
                        }
                    } 

                    // Searching for Choice/Gather
                    // WEIRD: FOR SOME REASON THIS DOESN'T WORK, BUT THE ELSE BELOW DOES
//                    else if ( (levelType == FlowLevel.WeavePoint) && (obj is IWeavePoint) ) {
//                        Console.WriteLine ("woo");
//                        return obj;
//                    }
              
                    else {
                        bool weaveLevelRequested = levelType == FlowLevel.WeavePoint;
                        bool isWeavePoint = obj is IWeavePoint;
                        if( weaveLevelRequested && isWeavePoint ) 
                            return obj;
                    }

                }
            }

            return null;
        }
            
            
        void OnLooseEndGathered (Runtime.Divert divert, Gather gather)
        {
            _allGatheredLooseEnds.Add (new GatheredLooseEnd{ divert = divert, targetGather = gather });
        }

        class GatheredLooseEnd
        {
            public Runtime.Divert divert;
            public Gather targetGather;
        }

        List<GatheredLooseEnd> _allGatheredLooseEnds;
	}
}

