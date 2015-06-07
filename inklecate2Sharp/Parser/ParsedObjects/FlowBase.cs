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

			foreach (var child in this.content) {
                var varDecl = child as VariableAssignment;
                if (varDecl != null && varDecl.isNewDeclaration) {
                    TryAddNewVariableDeclaration (varDecl);
                }
			}
		}

        List<Parsed.Object> SplitWeaveAndSubFlowContent(List<Parsed.Object> contentObjs)
        {
            var weaveObjs = new List<Parsed.Object> ();
            var subFlowObjs = new List<Parsed.Object> ();

            foreach (var obj in contentObjs) {
                if (obj is FlowBase) {
                    subFlowObjs.Add (obj);
                } else {
                    weaveObjs.Add (obj);
                }
            }

            var finalContent = new List<Parsed.Object> ();
            if (weaveObjs.Count > 0) {
                var weave = new Weave (weaveObjs, 0);
                finalContent.Add (weave);
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

        public bool ResolveVariableWithName(string varName, out Parsed.FlowBase foundFlow, Parsed.Object fromNode, bool allowReadCounts, bool reportErrors)
        {
            foundFlow = null;

            if (fromNode == null) {
                fromNode = this;
            }
                
            List<string> searchedLocationsForErrorReport = null;
            if (reportErrors) {
                searchedLocationsForErrorReport = new List<string> ();
            }

            var ancestor = fromNode;
            while (ancestor != null) {

                if (ancestor is FlowBase) {
                    var ancestorFlow = (FlowBase)ancestor;

                    if (reportErrors && ancestorFlow.name != null) {
                        searchedLocationsForErrorReport.Add ("'"+ancestorFlow.name+"'");
                    }

                    if( ancestorFlow.HasOwnVariableWithName(varName, allowReadCounts) ) {
                        return true;
                    }

                    if (allowReadCounts) {
                        var content = ancestorFlow.ContentWithNameAtLevel (varName);
                        if (content != null) {
                            foundFlow = (FlowBase) content;
                            return true;
                        }
                    }

                }

                ancestor = ancestor.parent;
            }

            if (reportErrors) {
                var locationsStr = "";
                if (searchedLocationsForErrorReport.Count > 0) {
                    var locationsListStr = string.Join (", ", searchedLocationsForErrorReport);
                    locationsStr = " in " + locationsListStr + " or globally";
                }
                string.Join (", ", searchedLocationsForErrorReport);
                Error ("variable '" + varName + "' not found"+locationsStr, fromNode);
            }

            return false;
        }

        public bool HasVariableWithName(string varName, bool allowReadCounts = true)
        {
            // Search full tree
            Parsed.FlowBase unusedFoundFlow = null;
            return ResolveVariableWithName (varName, out unusedFoundFlow, this, allowReadCounts, reportErrors:false);
        }

        public virtual bool HasOwnVariableWithName(string varName, bool allowReadCounts = true)
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
            
       // List<Weave> _weaves;
	}
}

