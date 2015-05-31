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
        public Dictionary<string, VariableAssignment> variableDeclarations;
        public abstract FlowLevel flowLevel { get; }

		public FlowBase (string name = null, List<Parsed.Object> topLevelObjects = null)
		{
			this.name = name;

			if (topLevelObjects == null) {
				topLevelObjects = new List<Parsed.Object> ();
			}
			this.content = topLevelObjects;

            variableDeclarations = new Dictionary<string, VariableAssignment> ();

			foreach (var child in this.content) {
				child.parent = this;

                var varDecl = child as VariableAssignment;
                if (varDecl != null && varDecl.isNewDeclaration) {
                    variableDeclarations [varDecl.variableName] = varDecl;
                }
			}
		}
            
        public override Runtime.Object GenerateRuntimeObject ()
        {
            var container = new Runtime.Container ();
            container.name = name;

            // Maintain a list of gathered loose ends so that we can resolve
            // their divert paths in ResolveReferences
            this._allGatheredLooseEnds = new List<GatheredLooseEnd> ();

            bool initialKnotOrStitchEntered = false;

            // Run through content defined for this knot/stitch:
            //  - First of all, any initial content before a sub-stitch
            //    or any weave content is added to the main content container
            //  - The first inner knot/stitch is automatically entered, while
            //    the others are only accessible by an explicit divert
            //  - Any Choices and Gathers (i.e. IWeavePoint) found are 
            //    processsed by GenerateFlowContent.
            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];

                // Inner knots and stitches
                if (obj is FlowBase) {

                    var childFlow = (FlowBase)obj;

                    // First inner knot/stitch - automatically step into it
                    if (!initialKnotOrStitchEntered) {
                        container.AddContent (childFlow.runtimeObject);
                        initialKnotOrStitchEntered = true;
                    } 

                    // All other knots/stitches are only accessible by name:
                    // i.e. by explicit divert
                    else {
                        container.AddToNamedContentOnly ((Runtime.INamedContent) childFlow.runtimeObject);
                    }
                }

                // Choices and Gathers: Process as blocks of weave-like content
                else if (obj is IWeavePoint) {
                    var result = GenerateWeaveBlockRuntime (ref contentIdx, indentIndex: 0);
                    container.AddContent (result.rootContainer);
                } 

                // Normal content (defined at start)
                else {
                    container.AddContent (obj.runtimeObject);
                }

                contentIdx++;
            }
                
            return container;
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
                    else if (levelType == FlowLevel.WeavePoint && obj is IWeavePoint) {
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

