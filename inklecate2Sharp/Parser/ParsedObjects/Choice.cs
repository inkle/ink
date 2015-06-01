using System;
using System.Text;
using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    public class Choice : Parsed.Object, IWeavePoint, INamedContent
	{
        public string startText { get; protected set; }
        public string choiceOnlyText { get; protected set; }
        public string contentOnlyText { get; protected set; }

        public string name { get; set; }

		public Path   explicitPath { get; }
        public bool   explicitGather { get; }
        public int    indentationDepth { get; set; } = 1;
        public bool   hasWeaveStyleInlineBrackets { get; set; }

        public Runtime.Container runtimeContainer { get { return _weaveContentContainer; } }

        public bool   hasOwnContent {
            get {
                return hasWeaveStyleInlineBrackets || this.explicitPath == null;
            }
        }

        public List<Parsed.Object> content { get { return _nestedContent; } }

        // Override runtimePath to point to the Choice's target content (after it's chosen),
        // as opposed to the default implementation which would point to the choice itself
        // (or it's outer container), which is what runtimeObject is.
        public override Runtime.Path runtimePath
        {
            get {
                if (_weaveContentContainer != null) {
                    return _weaveContentContainer.path;
                } else {
                    // This Choice may or may not have been resolved already
                    ResolveExplicitPathIfNecessary ();
                    return _resolvedExplicitPath;
                }
            }
        }

        public Choice (string startText, string choiceOnlyText, string contentOnlyText, Divert divert)
		{
            this.startText = startText;
            this.choiceOnlyText = choiceOnlyText;
            this.contentOnlyText = contentOnlyText;

            if (divert != null) {
                if (divert.isToGather) {
                    this.explicitGather = true;
                } else {
                    this.explicitPath = divert.target;
                }
            }
		}

        public void AddNestedContent(Parsed.Object obj)
        {
            if (_nestedContent == null) {
                _nestedContent = new List<Parsed.Object> ();
            }

            _nestedContent.Add (obj);
            obj.parent = this;
        }

		public override Runtime.Object GenerateRuntimeObject ()
        {
            // Choice Text
            var choiceTextSB = new StringBuilder ();
            if (startText != null) {
                choiceTextSB.Append (startText);
            }
            if (choiceOnlyText != null) {
                choiceTextSB.Append (choiceOnlyText);
            }

            // Content (Weave style choices)
            var contentTextSB = new StringBuilder ();
            if (hasWeaveStyleInlineBrackets) {
                if (startText != null) {
                    contentTextSB.Append (startText);
                }
                if (contentOnlyText != null) {
                    contentTextSB.Append (contentOnlyText);
                }
            }

            // Build choice itself
            _runtimeChoice = new Runtime.Choice (choiceTextSB.ToString());

            // Nested content like this:
            // [
            //     choice: -> "c"
            //     (c) = [
            //         weave content
            //     ]
            // ]
            if ( hasOwnContent ) {

                _weaveContentContainer = new Runtime.Container ();
                _weaveContentContainer.AddContent (new Runtime.Text (contentTextSB.ToString () + "\n"));
                _weaveContentContainer.name = "c";

                if (this.explicitPath != null) {
                    _weaveContentEndDivert = new Runtime.Divert ();
                    _weaveContentContainer.AddContent (_weaveContentEndDivert);
                }

                _weaveOuterContainer = new Runtime.Container ();
                _weaveOuterContainer.AddContent (_runtimeChoice);
                _weaveOuterContainer.AddToNamedContentOnly (_weaveContentContainer);

                if (_nestedContent != null) {
                    foreach(var nestedObj in _nestedContent) {

                        // Explicit gather diverts aren't included since the
                        // weave generating algorithm adds another normal divert
                        var divert = nestedObj as Divert;
                        if (divert != null && divert.isToGather) {
                            continue;
                        }

                        _weaveContentContainer.AddContent(nestedObj.runtimeObject);
                    }
                }

                return _weaveOuterContainer;
            } 

            // Simple/normal choice
            else {
                return _runtimeChoice;
            }
		}

        void ResolveExplicitPathIfNecessary()
        {
            if ( _resolvedExplicitPath != null) {
                return;
            }

            Parsed.Object obj = explicitPath.ResolveFromContext (this);
            if (obj == null) {
                Error ("Choice: target not found: '" + explicitPath.ToString () + "'");
            }

            _resolvedExplicitPath = obj.runtimePath;

            if (_weaveContentEndDivert != null) {
                _weaveContentEndDivert.targetPath = _resolvedExplicitPath;
            } else {
                _runtimeChoice.pathOnChoice = _resolvedExplicitPath;
            }
        }

        public override void ResolveReferences(Story context)
		{
			// Weave style choice - target own content container
            if (_weaveContentContainer != null) {
                _runtimeChoice.pathOnChoice = _weaveContentContainer.path;
            }

            // Resolve path that was explicitly specified (either at the end of the weave choice, or just as the normal choice path)
            if (explicitPath != null) {
                ResolveExplicitPathIfNecessary ();
            }

            if (_nestedContent != null) {
                foreach (var obj in _nestedContent) {
                    obj.ResolveReferences (context);
                }
            }
		}

        List<Parsed.Object> _nestedContent;

        Runtime.Choice _runtimeChoice;
        Runtime.Container _weaveContentContainer;
        Runtime.Container _weaveOuterContainer;
        Runtime.Divert _weaveContentEndDivert;
        Runtime.Path _resolvedExplicitPath;
	}

}

