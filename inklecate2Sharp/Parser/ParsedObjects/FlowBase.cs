using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Inklewriter.Parsed
{
	// Base class for Knots and Stitches
	public abstract class FlowBase : Parsed.Object
	{
		public string name { get; protected set; }
		public List<Parsed.Object> content { get; protected set; }
        public Dictionary<string, VariableAssignment> variableDeclarations;

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

		public override Runtime.Object GenerateRuntimeObject()
		{
            var newContent = new List<Parsed.Object> ();

            var container = new Runtime.Container ();
            container.name = name;

            // Choices at each indentation level
            var allWeavePointsByIndentation = new List<List<IWeavePoint>> ();

            FlowBase firstSubFlow = null;

            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];
                IWeavePoint parentObjForThisContent = null;



                // "sub-flow" means "stitch within knot" or "knot within story"
                bool isSubFlow = obj is FlowBase;

                // First defined sub-flow within this flow?
                bool isFirstSubFlow = isSubFlow && firstSubFlow == null;


                if (isSubFlow) {
                    var knotOrStitch = (FlowBase) obj;
                    if ( container.namedContent.ContainsKey(knotOrStitch.name) ) {
                        Error ("Duplicate content named " + knotOrStitch.name);
                    }

                    if (isFirstSubFlow) {
                        firstSubFlow = knotOrStitch;
                    }
                } 




                // Weave points are Choices and Gathers (i.e. "weave bullets"
                if (obj is IWeavePoint) {

                    var weavePoint = (IWeavePoint) obj;

                    int indentIndex = weavePoint.indentationDepth - 1;
                    int removeIndentFrom = -1;

                    if (obj is Gather) {
                        var gather = (Gather)obj;

                        gather.name = "g" + contentIdx;

                        // Gather loose ends
                        indentIndex = gather.indentationDepth-1;

                        // Point loose ends at this gather point
                        for (var ind = indentIndex; ind < allWeavePointsByIndentation.Count; ++ind) {
                            var weavePointsAtLevel = allWeavePointsByIndentation [ind];
                            foreach(var previousWeavePoint in weavePointsAtLevel) {
                                if (previousWeavePoint.hasLooseEnd) {
                                    var gatherDivert = new Parsed.Divert (gather, false);
                                    newContent.Add (gatherDivert);
                                    previousWeavePoint.runtimeContainer.AddContent(gatherDivert.runtimeObject);
                                }
                            }
                        }

                        // Loose ends dealt with, reduce indent level
                        removeIndentFrom = indentIndex;
                    }

                    if (indentIndex >= allWeavePointsByIndentation.Count + 1) {
                        Error ("weave nesting levels shouldn't jump (i.e. number bullets should only add or remove one at each level)", obj);
                        return container;
                    }

                    // Going back outer scope? (smaller number of bullets)
                    if (removeIndentFrom >= 0 && removeIndentFrom < allWeavePointsByIndentation.Count) {
                        allWeavePointsByIndentation.RemoveRange (removeIndentFrom, allWeavePointsByIndentation.Count - removeIndentFrom);
                    }

                    // Drilling into more indentated level (more bullets)
                    if (indentIndex >= allWeavePointsByIndentation.Count) {
                        allWeavePointsByIndentation.Add (new List<IWeavePoint> ());
                        Debug.Assert (indentIndex == allWeavePointsByIndentation.Count - 1);
                    } 

                    var weavePointsThisLevel = allWeavePointsByIndentation [indentIndex];
                    weavePointsThisLevel.Add (weavePoint);

                    if (indentIndex > 0) {
                        var weavePointsForLevel = allWeavePointsByIndentation[indentIndex - 1];
                        parentObjForThisContent = weavePointsForLevel.Last ();
                    }

                }

                // Ordinary non-choice content
                else {
                    if (allWeavePointsByIndentation.Count > 0) {
                        parentObjForThisContent = allWeavePointsByIndentation.Last().Last();
                    }
                }

                Runtime.Container containerToAddTo;



                // Move this content (choice or gather) to be owned by
                // the latest weave point if asked to
                if (parentObjForThisContent != null) {


                    containerToAddTo = parentObjForThisContent.runtimeContainer;

                    //parentObjForThisContent.AddNestedContent (obj);
                    //content.RemoveAt (contentIdx);
                } else {
                    containerToAddTo = container;
                    //contentIdx++;
                }



                bool includeInSequentialFlow = !isSubFlow || isFirstSubFlow;
                if (obj is Gather)
                    includeInSequentialFlow = false;

                if (includeInSequentialFlow) {
                    containerToAddTo.AddContent (obj.runtimeObject);

                } else {
                    containerToAddTo.AddToNamedContentOnly ((Runtime.INamedContent) obj.runtimeObject);
                }


                contentIdx++;

            }

            this.content.AddRange (newContent);

            return container;
		}

        public override void ResolveReferences (Story context)
		{
			foreach (Parsed.Object obj in content) {
				obj.ResolveReferences (context); 
			}
        }
	}
}

