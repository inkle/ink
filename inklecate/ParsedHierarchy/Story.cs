using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("tests")]

namespace Inklewriter.Parsed
{
	internal class Story : FlowBase
    {
        public override FlowLevel flowLevel { get { return FlowLevel.Story; } }
        public bool hadError { get { return _errors != null && _errors.Count > 0; } }
        public bool hadWarning { get { return _warnings != null && _warnings.Count > 0; } }
        public List<string> errors { get { return _errors; } }
        public List<string> warnings { get { return _warnings; } }

        // Build setting for exporting:
        // When true, the visit count and beat index for *all* knots, stitches, choices,
        // and gathers are counted. When false, only those that are referenced by
        // a read count variable reference are stored.
        // Storing all counts is more robust and future proof (updates to the story file
        // that reference previously uncounted visits are possible, but generates a much 
        // larger safe file, with a lot of potentially redundant counts.
        public bool countAllVisits = false;

        public Story (List<Parsed.Object> toplevelObjects) : base(null, toplevelObjects)
		{
            // Don't do anything on construction, leave it lightweight until
            // the ExportRuntime method is called.
		}

        // Before this function is called, we have IncludedFile objects interspersed
        // in our content wherever an include statement was.
        // So that the include statement can be added in a sensible place (e.g. the
        // top of the file) without side-effects of jumping into a knot that was
        // defined in that include, we separate knots and stitches from anything
        // else defined at the top scope of the included file.
        // 
        // Algorithm: For each IncludedFile we find, split its contents into
        // knots/stiches and any other content. Insert the normal content wherever
        // the include statement was, and append the knots/stitches to the very
        // end of the main story.
        protected override void PreProcessTopLevelObjects(List<Parsed.Object> topLevelContent)
        {
            var flowsFromOtherFiles = new List<FlowBase> ();

            // Inject included files
            int i = 0;
            while (i < topLevelContent.Count) {
                var obj = topLevelContent [i];
                if (obj is IncludedFile) {

                    var file = (IncludedFile)obj;

                    // Remove the IncludedFile itself
                    topLevelContent.RemoveAt (i);

                    // When an included story fails to load, the include
                    // line itself is still valid, so we have to handle it here
                    if (file.includedStory) {
                        
                        var nonFlowContent = new List<Parsed.Object> ();

                        var subStory = file.includedStory;

                        // Allow empty file
                        if (subStory.content != null) {

                            foreach (var subStoryObj in subStory.content) {
                                if (subStoryObj is FlowBase) {
                                    flowsFromOtherFiles.Add ((FlowBase)subStoryObj);
                                } else {
                                    nonFlowContent.Add (subStoryObj);
                                }
                            }

                            // Add contents of the file in its place
                            topLevelContent.InsertRange (i, nonFlowContent);

                            // Skip past the content of this sub story
                            // (since it will already have recursively included
                            //  any lines from other files)
                            i += nonFlowContent.Count;
                        }

                    }

                    // Include object has been removed, with possible content inserted,
                    // and position of 'i' will have been determined already.
                    continue;
                } 

                // Non-include: skip over it
                else {
                    i++;
                }
            }

            // Add the flows we collected from the included files to the
            // end of our list of our content
            topLevelContent.AddRange (flowsFromOtherFiles.ToArray());

        }

		public Runtime.Story ExportRuntime()
		{
			// Get default implementation of runtimeObject, which calls ContainerBase's generation method
            var rootContainer = runtimeObject as Runtime.Container;

            // Signal that it's safe to exit without error, even if there are no choices generated
            // (this only happens at the end of top level content that isn't in any particular knot)
            rootContainer.AddContent (Runtime.ControlCommand.Stop ());

			// Replace runtimeObject with Story object instead of the Runtime.Container generated by Parsed.ContainerBase
			var runtimeStory = new Runtime.Story (rootContainer);
			runtimeObject = runtimeStory;

            if (CheckErrors ())
                return null;

            int earlyWarningCount = 0;
            if( _warnings != null )
                earlyWarningCount = _warnings.Count;

			// Now that the story has been fulled parsed into a hierarchy,
			// and the derived runtime hierarchy has been built, we can
			// resolve referenced symbols such as variables and paths.
			// e.g. for paths " -> knotName --> stitchName" into an INKPath (knotName.stitchName)
			// We don't make any assumptions that the INKPath follows the same
			// conventions as the script format, so we resolve to actual objects before
			// translating into an INKPath. (This also allows us to choose whether
			// we want the paths to be absolute)
			ResolveReferences (this);

            if (CheckErrors (earlyWarningCount))
                return null;

			return runtimeStory;
		}

        bool CheckErrors(int previouslyPrintedWarnings = 0)
        {
            // Print all warnings before all errors
            if (hadWarning) {
                for (int i = previouslyPrintedWarnings; i < _warnings.Count; ++i) {
                    Console.WriteLine (_warnings [i]);
                }
            }

            // Don't successfully return the object if there was an error
            if (hadError) {
                foreach (var e in _errors) Console.WriteLine (e);
                return true;
            }

            return false;
        }

        public override void Error(string message, Parsed.Object source, bool isWarning)
		{
            var sb = new StringBuilder ();
            if (isWarning) {
                sb.Append ("WARNING: ");
            } else {
                sb.Append ("ERROR: ");
            }

            sb.Append (message);
            if (source && source.debugMetadata != null && source.debugMetadata.startLineNumber >= 1 ) {
                sb.Append (" on "+source.debugMetadata.ToString());
            }

            message = sb.ToString ();

            if (isWarning) {
                if (_warnings == null)
                    _warnings = new List<string> ();

                _warnings.Add (message);
            } else {
                if (_errors == null)
                    _errors = new List<string> ();

                _errors.Add (message);
            }
		}

        public void ResetError()
        {
            _errors = null;
            _warnings = null;
        }

        List<string> _errors;
        List<string> _warnings;
	}
}

