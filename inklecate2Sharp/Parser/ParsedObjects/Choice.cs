using System.Text;

namespace Inklewriter.Parsed
{
    public class Choice : Parsed.Object, IWeavePoint, INamedContent
	{
        public string startText { get; protected set; }
        public string choiceOnlyText { get; protected set; }
        public ContentList innerContent { get; protected set; }

        public string name { get; set; }

        public Expression condition { 
            get { 
                return _condition; 
            } 
            set { 
                _condition = value; 
                if( _condition != null )
                    AddContent (_condition);
            }
        }

        public bool onceOnly { get; set; }

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

        public Choice (string startText, string choiceOnlyText, ContentList innerContent, Divert divert)
		{
            this.startText = startText;
            this.choiceOnlyText = choiceOnlyText;
            this.innerContent = innerContent;

            if( innerContent != null )
                AddContent (this.innerContent);

            this.onceOnly = true; // default

            if (divert != null) {
                if (divert.isToGather) {
                    this.explicitGather = true;
                } else {
                    this.explicitPath = divert.target;
                }
            }
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
            var onChoosingContent = new ContentList ();
            AddContent (onChoosingContent);
            if (hasWeaveStyleInlineBrackets) {
                if (startText != null) {
                    onChoosingContent.AddContent (new Parsed.Text(startText));
                }
                if (innerContent != null) {
                    onChoosingContent.AddContent (innerContent);
                }
            }

            // Build choice itself
            _runtimeChoice = new Runtime.Choice (choiceTextSB.ToString(), onceOnly);

            // Nested content like this:
            // [
            //     choice: -> "c"
            //     (c) = [
            //         weave content
            //     ]
            // ]
            if ( hasOwnContent || condition != null ) {

                _weaveOuterContainer = new Runtime.Container ();

                if (condition != null) {
                    var exprContainer = (Runtime.Container) condition.runtimeObject;
                    _weaveOuterContainer.AddContentsOfContainer (exprContainer);
                    _runtimeChoice.hasCondition = true;
                }

                _weaveOuterContainer.AddContent (_runtimeChoice);

                if( hasOwnContent ) {

                    if (onChoosingContent != null && onChoosingContent.content != null)
                        _weaveContentContainer = onChoosingContent.runtimeContainer;
                    else
                        _weaveContentContainer = new Runtime.Container ();
                    
                    
                    _weaveContentContainer.name = "c";
                    _weaveContentContainer.visitsShouldBeCounted = true;

                    if (this.explicitPath != null) {
                        _weaveContentEndDivert = new Runtime.Divert ();
                        _weaveContentContainer.AddContent (_weaveContentEndDivert);
                    }

                    _weaveOuterContainer.AddToNamedContentOnly (_weaveContentContainer);

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
                return;
            }

            _resolvedExplicitPath = obj.runtimePath;

            if (!hasOwnContent && onceOnly) {
                var targetObjContainer = obj.runtimeObject as Runtime.Container;
                targetObjContainer.visitsShouldBeCounted = true;
            }

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

            base.ResolveReferences (context);
		}
            
        Runtime.Choice _runtimeChoice;
        Runtime.Container _weaveContentContainer;
        Runtime.Container _weaveOuterContainer;
        Runtime.Divert _weaveContentEndDivert;
        Runtime.Path _resolvedExplicitPath;
        Expression _condition;
	}

}

