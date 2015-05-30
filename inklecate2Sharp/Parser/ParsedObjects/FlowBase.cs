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
            if (this is Knot || this is Stitch) {
                CreateWeaveHierarchy ();
            }

			var container = new Runtime.Container ();
			container.name = name;

            FlowBase firstSubFlow = null;

			foreach (var parsedObj in content) {
				Runtime.Object runtimeObj = parsedObj.runtimeObject;


                // "sub-flow" means "stitch within knot" or "knot within story"
                bool isSubFlow = parsedObj is FlowBase;

                // First defined sub-flow within this flow?
                bool isFirstSubFlow = isSubFlow && firstSubFlow == null;

				
                if (isSubFlow) {
                    var knotOrStitch = (FlowBase) parsedObj;
					if ( container.namedContent.ContainsKey(knotOrStitch.name) ) {
						Error ("Duplicate content named " + knotOrStitch.name);
					}
				} 

                // In general, stitches aren't automatically stepped into, but have to be explicitly linked to.
                // However, the first stitch in a knot is automatically added to the sequential flow as an entry point,
                // even if there's some other content before it.
                // TODO: This is currently true of stories/knots as well - the first knot is automatically stepped into,
                // under the assumption that if the writer wants to divert elsewhere first, then they will do so. Could
                // re-evaluate that decision though...
                bool includeInSequentialFlow = !isSubFlow || isFirstSubFlow;
                if (includeInSequentialFlow) {
                    container.AddContent (runtimeObj);

                } else {
                    container.AddToNamedContentOnly ((Runtime.INamedContent) runtimeObj);
				}

                if (isFirstSubFlow) {
                    firstSubFlow = (FlowBase) parsedObj;
                }
					
			}

			return container;
		}

        public override void ResolveReferences (Story context)
		{
			foreach (Parsed.Object obj in content) {
				obj.ResolveReferences (context); 
			}
		}

        protected void CreateWeaveHierarchy ()
        {
            // Choices at each indentation level
            var allWeavePointsByIndentation = new List<List<IWeavePoint>> ();

            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];
                IWeavePoint parentObjForThisContent = null;

                // Weave points are Choices and Gathers (i.e. "weave bullets"
                if (obj is IWeavePoint) {

                    var weavePoint = (IWeavePoint) obj;

                    int indentIndex = weavePoint.indentationDepth - 1;
                    int removeIndentFrom = -1;

                    if (obj is Gather) {
                        var gather = (Gather)obj;

                        // Gather loose ends
                        indentIndex = gather.indentationDepth-1;

                        // Point loose ends at this gather point
                        for (var ind = indentIndex; ind < allWeavePointsByIndentation.Count; ++ind) {
                            var weavePointsAtLevel = allWeavePointsByIndentation [ind];
                            foreach(var previousWeavePoint in weavePointsAtLevel) {
                                if (previousWeavePoint.hasLooseEnd) {
                                    previousWeavePoint.AddNestedContent (new Parsed.Divert (gather, false));
                                }
                            }
                        }

                        // Loose ends dealt with, reduce indent level
                        removeIndentFrom = indentIndex;
                    }

                    if (indentIndex >= allWeavePointsByIndentation.Count + 1) {
                        Error ("weave nesting levels shouldn't jump (i.e. number bullets should only add or remove one at each level)", obj);
                        return;
                    }

                    // Going back outer scope? (smaller number of bullets)
                    if (removeIndentFrom < allWeavePointsByIndentation.Count) {
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

                // Move this content (choice or gather) to be owned by
                // the latest weave point if asked to
                if (parentObjForThisContent != null) {
                    parentObjForThisContent.AddNestedContent (obj);
                    content.RemoveAt (contentIdx);
                } else {
                    contentIdx++;
                }

            }
        }
	}
}

