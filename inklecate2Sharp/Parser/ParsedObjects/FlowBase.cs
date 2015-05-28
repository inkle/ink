using System;
using System.Collections.Generic;

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
            LinkWeaveStructure ();

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

        protected void LinkWeaveStructure ()
        {
            Choice activeChoice = null;

            int contentIdx = 0;
            while (contentIdx < content.Count) {

                Parsed.Object obj = content [contentIdx];
                bool objMoved = false;

                if (obj is Choice) {
                    var choice = (Choice)obj;
                    if (choice.hasMultiLineContent) {
                        activeChoice = choice;
                    }
                } 

                // Normal content - move it into the active Choice
                else if( activeChoice != null ) {
                    activeChoice.AddNestedContent(obj);
                    content.RemoveAt (contentIdx);
                }

                if (!objMoved) {
                    contentIdx++;
                }

            }
        }
	}
}

