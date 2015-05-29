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
            var allWeavePointsByIndentation = new List<List<Parsed.Object>> ();

            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];

                int indentIndex = -1;
                int removeIndentFrom = -1;

                Parsed.Object parentObjForThisContent = null;

                if (obj is Choice) {
                    var choice = (Choice)obj;

                    indentIndex = choice.indentationDepth - 1;
                    removeIndentFrom = indentIndex+1;
                } 

                else if (obj is Gather) {
                    var gather = (Gather)obj;

                    // Gather loose ends
                    indentIndex = gather.indentationDepth-1;

                    for (var ind = indentIndex; ind < allWeavePointsByIndentation.Count; ++ind) {
                        var weavePointsAtLevel = allWeavePointsByIndentation [ind];
                        foreach(var weavePoint in weavePointsAtLevel) {

                            if (weavePoint is Choice) {
                                var choice = (Choice)weavePoint;
                                if (choice.hasLooseEnd) {
                                    choice.AddNestedContent (new Parsed.Divert (gather, false));
                                }
                            } else if (weavePoint is Gather) {
                                var parentGather = (Gather) weavePoint;
                                parentGather.AddNestedContent (new Parsed.Divert (gather, false));
                            }


                        }
                    }

                    removeIndentFrom = indentIndex;
                }

                // Ordinary non-choice content
                else {

                    if (allWeavePointsByIndentation.Count > 0) {
                        parentObjForThisContent = allWeavePointsByIndentation.Last().Last();
                    }

                }

                if( indentIndex >= 0 ) {

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
                        allWeavePointsByIndentation.Add (new List<Parsed.Object> ());
                        Debug.Assert (indentIndex == allWeavePointsByIndentation.Count - 1);
                    } 

                    var weavePointsThisLevel = allWeavePointsByIndentation [indentIndex];
                    weavePointsThisLevel.Add (obj);

                    if (indentIndex > 0) {
                        var weavePointsForLevel = allWeavePointsByIndentation[indentIndex - 1];
                        parentObjForThisContent = weavePointsForLevel.Last ();
                    }

                }

                // Move this content (choice or other) to be owned by a choice/gather?
                if (parentObjForThisContent != null) {

                    // TODO: Create IWeavePoint
                    if (parentObjForThisContent is Choice) {
                        var parentChoice = (Choice)parentObjForThisContent;
                        parentChoice.AddNestedContent (obj);
                    } else {
                        var parentGather = (Gather)parentObjForThisContent;
                        parentGather.AddNestedContent (obj);
                    }

                    content.RemoveAt (contentIdx);
                } else {
                    contentIdx++;
                }

            }
        }
	}
}

