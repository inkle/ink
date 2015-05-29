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
            var allChoicesByIndentationLevel = new List<List<Choice>> ();

            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];

                Choice parentChoiceForThisContent = null;

                if (obj is Choice) {
                    var choice = (Choice)obj;

                    int indentIndex = choice.indentationDepth - 1;

                    if (choice.indentationDepth >= allChoicesByIndentationLevel.Count + 2) {
                        Error ("weave nesting levels shouldn't jump (i.e. number bullets should only add or remove one at each level)", choice);
                        return;
                    }

                    // Going back outer scope? (smaller number of bullets)
                    if (indentIndex < allChoicesByIndentationLevel.Count-1) {
                        int removalIndex = indentIndex+1;
                        allChoicesByIndentationLevel.RemoveRange (removalIndex, allChoicesByIndentationLevel.Count - removalIndex);
                    }

                    // Drilling into more indentated level (more bullets)
                    if (indentIndex >= allChoicesByIndentationLevel.Count) {
                        allChoicesByIndentationLevel.Add (new List<Choice> ());
                        Debug.Assert (indentIndex == allChoicesByIndentationLevel.Count - 1);
                    } 

                    var choicesThisLevel = allChoicesByIndentationLevel [indentIndex];
                    choicesThisLevel.Add (choice);

                    if (choice.indentationDepth > 1) {
                        //int indentIndex = choice.indentationDepth - 1;
                        var choicesAtParentLevel = allChoicesByIndentationLevel[indentIndex - 1];
                        parentChoiceForThisContent = choicesAtParentLevel.Last ();
                    }
                } 

                else if (obj is Gather) {
                    var gather = (Gather)obj;

                    // Gather loose ends
                    int indentIndex = gather.indentationDepth-1;

                    for (var ind = indentIndex; ind < allChoicesByIndentationLevel.Count; ++ind) {
                        var choicesAtLevel = allChoicesByIndentationLevel [ind];
                        foreach(var choice in choicesAtLevel) {
                            if (choice.hasLooseEnd) {
                                choice.AddNestedContent (new Parsed.Divert (gather, false));
                            }
                        }
                    }

                    allChoicesByIndentationLevel.RemoveRange (indentIndex, allChoicesByIndentationLevel.Count - indentIndex);

                }

                // Ordinary non-choice content
                else {

                    if (allChoicesByIndentationLevel.Count > 0) {
                        parentChoiceForThisContent = allChoicesByIndentationLevel.Last().Last();
                    }

                }

                // Move this content (choice or other) to be owned by a choice?
                if (parentChoiceForThisContent != null) {
                    parentChoiceForThisContent.AddNestedContent (obj);
                    content.RemoveAt (contentIdx);
                } else {
                    contentIdx++;
                }

            }
        }
	}
}

