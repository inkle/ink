using System.Text;

namespace Ink.Parsed
{
    public class Choice : Parsed.Object, IWeavePoint, INamedContent
	{
        public ContentList startContent { get; protected set; }
        public ContentList choiceOnlyContent { get; protected set; }
        public ContentList innerContent { get; protected set; }

        public string name
        {
            get { return identifier?.name; }
        }
        public Identifier identifier { get; set; }

        public Expression condition {
            get {
                return _condition;
            }
            set {
                _condition = value;
                if( _condition )
                    AddContent (_condition);
            }
        }

        public bool onceOnly { get; set; }
        public bool isInvisibleDefault { get; set; }

        public int    indentationDepth { get; set; }// = 1;
        public bool   hasWeaveStyleInlineBrackets { get; set; }

        // Required for IWeavePoint interface
        // Choice's target container. Used by weave to append any extra
        // nested weave content into.
        public Runtime.Container runtimeContainer { get { return _innerContentContainer; } }


        public Runtime.Container innerContentContainer {
            get {
                return _innerContentContainer;
            }
        }

        public override Runtime.Container containerForCounting {
            get {
                return _innerContentContainer;
            }
        }

        // Override runtimePath to point to the Choice's target content (after it's chosen),
        // as opposed to the default implementation which would point to the choice itself
        // (or it's outer container), which is what runtimeObject is.
        public override Runtime.Path runtimePath
        {
            get {
                return _innerContentContainer.path;
            }
        }

        public Choice (ContentList startContent, ContentList choiceOnlyContent, ContentList innerContent)
		{
            this.startContent = startContent;
            this.choiceOnlyContent = choiceOnlyContent;
            this.innerContent = innerContent;
			this.indentationDepth = 1;

            if (startContent)
                AddContent (this.startContent);

            if (choiceOnlyContent)
                AddContent (this.choiceOnlyContent);

            if( innerContent )
                AddContent (this.innerContent);

            this.onceOnly = true; // default
		}


		public override Runtime.Object GenerateRuntimeObject ()
        {
            _outerContainer = new Runtime.Container ();

            // Content names for different types of choice:
            //  * start content [choice only content] inner content
            //  * start content   -> divert
            //  * start content
            //  * [choice only content]

            // Hmm, this structure has become slightly insane!
            //
            // [
            //     EvalStart
            //     assign $r = $r1   -- return target = return label 1
            //     BeginString
            //     -> s
            //     [(r1)]            -- return label 1 (after start content)
            //     EndString
            //     BeginString
            //     ... choice only content
            //     EndEval
            //     Condition expression
            //     choice: -> "c-0"
            //     (s) = [
            //         start content
            //         -> r          -- goto return label 1 or 2
            //     ]
            //  ]
            //
            //  in parent's container: (the inner content for the choice)
            //
            //  (c-0) = [
            //      EvalStart
            //      assign $r = $r2   -- return target = return label 2
            //      EndEval
            //      -> s
            //      [(r2)]            -- return label 1 (after start content)
            //      inner content
            //  ]
            //

            _runtimeChoice = new Runtime.ChoicePoint (onceOnly);
            _runtimeChoice.isInvisibleDefault = this.isInvisibleDefault;

            if (startContent || choiceOnlyContent || condition) {
                _outerContainer.AddContent (Runtime.ControlCommand.EvalStart ());
            }

            // Start content is put into a named container that's referenced both
            // when displaying the choice initially, and when generating the text
            // when the choice is chosen.
            if (startContent) {

                // Generate start content and return
                //  - We can't use a function since it uses a call stack element, which would
                //    put temporary values out of scope. Instead we manually divert around.
                //  - $r is a variable divert target contains the return point
                _returnToR1 = new Runtime.DivertTargetValue ();
                _outerContainer.AddContent (_returnToR1);
                var varAssign = new Runtime.VariableAssignment ("$r", true);
                _outerContainer.AddContent (varAssign);

                // Mark the start of the choice text generation, so that the runtime
                // knows where to rewind to to extract the content from the output stream.
                _outerContainer.AddContent (Runtime.ControlCommand.BeginString ());

                _divertToStartContentOuter = new Runtime.Divert ();
                _outerContainer.AddContent (_divertToStartContentOuter);

                // Start content itself in a named container
                _startContentRuntimeContainer = startContent.GenerateRuntimeObject () as Runtime.Container;
                _startContentRuntimeContainer.name = "s";

                // Effectively, the "return" statement - return to the point specified by $r
                var varDivert = new Runtime.Divert ();
                varDivert.variableDivertName = "$r";
                _startContentRuntimeContainer.AddContent (varDivert);

                // Add the container
                _outerContainer.AddToNamedContentOnly (_startContentRuntimeContainer);

                // This is the label to return to
                _r1Label = new Runtime.Container ();
                _r1Label.name = "$r1";
                _outerContainer.AddContent (_r1Label);

                _outerContainer.AddContent (Runtime.ControlCommand.EndString ());

                _runtimeChoice.hasStartContent = true;
            }

            // Choice only content - mark the start, then generate it directly into the outer container
            if (choiceOnlyContent) {
                _outerContainer.AddContent (Runtime.ControlCommand.BeginString ());

                var choiceOnlyRuntimeContent = choiceOnlyContent.GenerateRuntimeObject () as Runtime.Container;
                _outerContainer.AddContentsOfContainer (choiceOnlyRuntimeContent);

                _outerContainer.AddContent (Runtime.ControlCommand.EndString ());

                _runtimeChoice.hasChoiceOnlyContent = true;
            }

            // Generate any condition for this choice
            if (condition) {
                condition.GenerateIntoContainer (_outerContainer);
                _runtimeChoice.hasCondition = true;
            }

            if (startContent || choiceOnlyContent || condition) {
                _outerContainer.AddContent (Runtime.ControlCommand.EvalEnd ());
            }

            // Add choice itself
            _outerContainer.AddContent (_runtimeChoice);

            // Container that choice points to for when it's chosen
            _innerContentContainer = new Runtime.Container ();

            // Repeat start content by diverting to its container
            if (startContent) {

                // Set the return point when jumping back into the start content
                //  - In this case, it's the $r2 point, within the choice content "c".
                _returnToR2 = new Runtime.DivertTargetValue ();
                _innerContentContainer.AddContent (Runtime.ControlCommand.EvalStart ());
                _innerContentContainer.AddContent (_returnToR2);
                _innerContentContainer.AddContent (Runtime.ControlCommand.EvalEnd ());
                var varAssign = new Runtime.VariableAssignment ("$r", true);
                _innerContentContainer.AddContent (varAssign);

                // Main divert into start content
                _divertToStartContentInner = new Runtime.Divert ();
                _innerContentContainer.AddContent (_divertToStartContentInner);

                // Define label to return to
                _r2Label = new Runtime.Container ();
                _r2Label.name = "$r2";
                _innerContentContainer.AddContent (_r2Label);
            }

            // Choice's own inner content
            if (innerContent) {
				var innerChoiceOnlyContent = innerContent.GenerateRuntimeObject () as Runtime.Container;
                _innerContentContainer.AddContentsOfContainer (innerChoiceOnlyContent);
            }

            if (this.story.countAllVisits) {
                _innerContentContainer.visitsShouldBeCounted = true;
            }

            _innerContentContainer.countingAtStartOnly = true;

            return _outerContainer;
		}

        public override void ResolveReferences(Story context)
		{
			// Weave style choice - target own content container
            if (_innerContentContainer) {
                _runtimeChoice.pathOnChoice = _innerContentContainer.path;

                if (onceOnly)
                    _innerContentContainer.visitsShouldBeCounted = true;
            }

            if (_returnToR1)
                _returnToR1.targetPath = _r1Label.path;

            if (_returnToR2)
                _returnToR2.targetPath = _r2Label.path;

            if( _divertToStartContentOuter )
                _divertToStartContentOuter.targetPath = _startContentRuntimeContainer.path;

            if( _divertToStartContentInner )
                _divertToStartContentInner.targetPath = _startContentRuntimeContainer.path;

            base.ResolveReferences (context);

            if( identifier != null && identifier.name.Length > 0 )
                context.CheckForNamingCollisions (this, identifier, Story.SymbolType.SubFlowAndWeave);
		}

        public override string ToString ()
        {
            if (choiceOnlyContent != null) {
                return string.Format ("* {0}[{1}]...", startContent, choiceOnlyContent);
            } else {
                return string.Format ("* {0}...", startContent);
            }
        }

        Runtime.ChoicePoint _runtimeChoice;
        Runtime.Container _innerContentContainer;
        Runtime.Container _outerContainer;
        Runtime.Container _startContentRuntimeContainer;
        Runtime.Divert _divertToStartContentOuter;
        Runtime.Divert _divertToStartContentInner;
        Runtime.Container _r1Label;
        Runtime.Container _r2Label;
        Runtime.DivertTargetValue _returnToR1;
        Runtime.DivertTargetValue _returnToR2;
        Expression _condition;
	}

}

