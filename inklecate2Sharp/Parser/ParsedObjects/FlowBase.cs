using System;
using System.Collections.Generic;
using System.Linq;

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
            CreateWeaveHierarchy ();

			var container = new Runtime.Container ();
			container.name = name;

			foreach (var parsedObj in content) {
				Runtime.Object runtimeObj = parsedObj.runtimeObject;

				bool isKnotOrStitch = parsedObj is Knot || parsedObj is Stitch;
				bool hasInitialContent = container.content.Count > 0;

				// Add named content (knots and stitches)
				if (isKnotOrStitch && hasInitialContent) {

					var knotOrStitch = parsedObj as FlowBase;
					if ( container.namedContent.ContainsKey(knotOrStitch.name) ) {
						Error ("Duplicate content named " + knotOrStitch.name);
					}

					container.AddToNamedContentOnly ((Runtime.INamedContent) runtimeObj);
				} else {
					container.AddContent (runtimeObj);
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
            // Active parent choices at each indentation level
            var choiceLevels = new List<Choice> ();

            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];

                Choice parentChoiceForThisContent = null;

                if (obj is Choice) {
                    var choice = (Choice)obj;

                    if (choice.indentationDepth > choiceLevels.Count + 1) {
                        Error ("weave nesting levels shouldn't jump (i.e. number bullets should only add or remove one at each level)", choice);
                        return;
                    }

                    // Higher (or equal) level choice
                    if (choice.indentationDepth <= choiceLevels.Count) {
                        int removalIndex = choice.indentationDepth - 1;
                        choiceLevels.RemoveRange (removalIndex, choiceLevels.Count - removalIndex);
                    }

                    choiceLevels.Add (choice);

                    if (choice.indentationDepth > 1) {
                        int indentIndex = choice.indentationDepth - 1;
                        parentChoiceForThisContent = choiceLevels [indentIndex - 1];
                    }
                } 

                // Ordinary non-choice content
                else {

                    if (choiceLevels.Count > 0) {
                        parentChoiceForThisContent = choiceLevels.Last ();
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

