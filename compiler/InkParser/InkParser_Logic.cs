using System.Collections.Generic;
using System.Linq;
using Ink.Parsed;

namespace Ink
{
    public partial class InkParser
    {

        protected Parsed.Object LogicLine()
        {
            Whitespace ();

            if (ParseString ("~") == null) {
                return null;
            }

            Whitespace ();

            // Some example lines we need to be able to distinguish between:
            // ~ temp x = 5  -- var decl + assign
            // ~ temp x      -- var decl
            // ~ x = 5       -- var assign
            // ~ x           -- expr (not var decl or assign)
            // ~ f()         -- expr
            // We don't treat variable decl/assign as an expression since we don't want an assignment
            // to have a return value, or to be used in compound expressions.
            ParseRule afterTilda = () => OneOf (ReturnStatement, TempDeclarationOrAssignment, Expression);

            var result = Expect(afterTilda, "expression after '~'", recoveryRule: SkipToNextLine) as Parsed.Object;

            // Prevent further errors, already reported expected expression and have skipped to next line.
            if (result == null) return new ContentList();

            // Parse all expressions, but tell the writer off if they did something useless like:
            //  ~ 5 + 4
            // And even:
            //  ~ false && myFunction()
            // ...since it's bad practice, and won't do what they expect if
            // they're expecting C's lazy evaluation.
            if (result is Expression && !(result is FunctionCall || result is IncDecExpression) ) {

                // TODO: Remove this specific error message when it has expired in usefulness
                var varRef = result as VariableReference;
                if (varRef && varRef.name == "include") {
                    Error ("'~ include' is no longer the correct syntax - please use 'INCLUDE your_filename.ink', without the tilda, and in block capitals.");
                }

                else {
                    Error ("Logic following a '~' can't be that type of expression. It can only be something like:\n\t~ return\n\t~ var x = blah\n\t~ x++\n\t~ myFunction()");
                }
            }

            // Line is pure function call? e.g.
            //  ~ f()
            // Add extra pop to make sure we tidy up after ourselves.
            // We no longer need anything on the evaluation stack.
            var funCall = result as FunctionCall;
            if (funCall) funCall.shouldPopReturnedValue = true;

            // If the expression contains a function call, then it could produce a text side effect,
            // in which case it needs a newline on the end. e.g.
            //  ~ printMyName()
            //  ~ x = 1 + returnAValueAndAlsoPrintStuff()
            // If no text gets printed, then the extra newline will have to be culled later.
            // Multiple newlines on the output will be removed, so there will be no "leak" for
            // long running calculations. It's disappointingly messy though :-/
            if (result.Find<FunctionCall>() != null ) {
                result = new ContentList (result, new Parsed.Text ("\n"));
            }

            Expect(EndOfLine, "end of line", recoveryRule: SkipToNextLine);

            return result as Parsed.Object;
        }

        protected Parsed.Object VariableDeclaration()
        {
            Whitespace ();

            var id = Parse (Identifier);
            if (id != "VAR")
                return null;

            Whitespace ();

            var varName = Expect (IdentifierWithMetadata, "variable name") as Identifier;

            Whitespace ();

            Expect (String ("="), "the '=' for an assignment of a value, e.g. '= 5' (initial values are mandatory)");

            Whitespace ();

            var definition = Expect (Expression, "initial value for ");

            var expr = definition as Parsed.Expression;

            if (expr) {
                if (!(expr is Number || expr is StringExpression || expr is DivertTarget || expr is VariableReference || expr is List)) {
                    Error ("initial value for a variable must be a number, constant, list or divert target");
                }

                if (Parse (ListElementDefinitionSeparator) != null)
                    Error ("Unexpected ','. If you're trying to declare a new list, use the LIST keyword, not VAR");

                // Ensure string expressions are simple
                else if (expr is StringExpression) {
                    var strExpr = expr as StringExpression;
                    if (!strExpr.isSingleString)
                        Error ("Constant strings cannot contain any logic.");
                }

                var result = new VariableAssignment (varName, expr);
                result.isGlobalDeclaration = true;
                return result;
            }

            return null;
        }

        protected Parsed.VariableAssignment ListDeclaration ()
        {
            Whitespace ();

            var id = Parse (Identifier);
            if (id != "LIST")
                return null;

            Whitespace ();

            var varName = Expect (IdentifierWithMetadata, "list name") as Identifier;

            Whitespace ();

            Expect (String ("="), "the '=' for an assignment of the list definition");

            Whitespace ();

            var definition = Expect (ListDefinition, "list item names") as ListDefinition;

            if (definition) {

                definition.identifier = varName;

                return new VariableAssignment (varName, definition);
            }

            return null;
        }

        protected Parsed.ListDefinition ListDefinition ()
        {
            AnyWhitespace ();

            var allElements = SeparatedList (ListElementDefinition, ListElementDefinitionSeparator);
            if (allElements == null)
                return null;

            return new ListDefinition (allElements);
        }

        protected string ListElementDefinitionSeparator ()
        {
            AnyWhitespace ();

            if (ParseString (",") == null) return null;

            AnyWhitespace ();

            return ",";
        }

        protected Parsed.ListElementDefinition ListElementDefinition ()
        {
            var inInitialList = ParseString ("(") != null;
            var needsToCloseParen = inInitialList;

            Whitespace ();

            var name = Parse (IdentifierWithMetadata);
            if (name == null)
                return null;

            Whitespace ();

            if (inInitialList) {
                if (ParseString (")") != null) {
                    needsToCloseParen = false;
                    Whitespace ();
                }
            }

            int? elementValue = null;
            if (ParseString ("=") != null) {

                Whitespace ();

                var elementValueNum = Expect (ExpressionInt, "value to be assigned to list item") as Number;
                if (elementValueNum != null) {
                    elementValue = (int) elementValueNum.value;
                }

                if (needsToCloseParen) {
                    Whitespace ();

                    if (ParseString (")") != null)
                        needsToCloseParen = false;
                }
            }

            if (needsToCloseParen)
                Error("Expected closing ')'");

            return new ListElementDefinition (name, inInitialList, elementValue);
        }

        protected Parsed.Object ConstDeclaration()
        {
            Whitespace ();

            var id = Parse (Identifier);
            if (id != "CONST")
                return null;

            Whitespace ();

            var varName = Expect (IdentifierWithMetadata, "constant name") as Identifier;

            Whitespace ();

            Expect (String ("="), "the '=' for an assignment of a value, e.g. '= 5' (initial values are mandatory)");

            Whitespace ();

            var expr = Expect (Expression, "initial value for ") as Parsed.Expression;
            if (!(expr is Number || expr is DivertTarget || expr is StringExpression)) {
                Error ("initial value for a constant must be a number or divert target");
            }

            // Ensure string expressions are simple
            else if (expr is StringExpression) {
                var strExpr = expr as StringExpression;
                if (!strExpr.isSingleString)
                    Error ("Constant strings cannot contain any logic.");
            }


            var result = new ConstantDeclaration (varName, expr);
            return result;
        }

        protected Parsed.Object InlineLogicOrGlue()
        {
            return (Parsed.Object) OneOf (InlineLogic, Glue);
        }

        protected Parsed.Glue Glue()
        {
            // Don't want to parse whitespace, since it might be important
            // surrounding the glue.
            var glueStr = ParseString("<>");
            if (glueStr != null) {
                return new Parsed.Glue (new Runtime.Glue ());
            } else {
                return null;
            }
        }

        protected Parsed.Object InlineLogic()
        {
            if ( ParseString ("{") == null) {
                return null;
            }

            Whitespace ();

            var logic = (Parsed.Object) Expect(InnerLogic, "some kind of logic, conditional or sequence within braces: { ... }");
            if (logic == null)
                return null;

            DisallowIncrement (logic);

            ContentList contentList = logic as ContentList;
            if (!contentList) {
                contentList = new ContentList (logic);
            }

            Whitespace ();

            Expect (String("}"), "closing brace '}' for inline logic");

            return contentList;
        }

        protected Parsed.Object InnerLogic()
        {
            Whitespace ();

            // Explicitly try the combinations of inner logic
            // that could potentially have conflicts first.

            // Explicit sequence annotation?
            SequenceType? explicitSeqType = (SequenceType?) ParseObject(SequenceTypeAnnotation);
            if (explicitSeqType != null) {
                var contentLists = (List<ContentList>) Expect(InnerSequenceObjects, "sequence elements (for cycle/stoping etc)");
                if (contentLists == null)
                    return null;
                return new Sequence (contentLists, (SequenceType) explicitSeqType);
            }

            // Conditional with expression?
            var initialQueryExpression = Parse(ConditionExpression);
            if (initialQueryExpression) {
                var conditional = (Conditional) Expect(() => InnerConditionalContent (initialQueryExpression), "conditional content following query");
                return conditional;
            }

            // Now try to evaluate each of the "full" rules in turn
            ParseRule[] rules = {

                // Conditional still necessary, since you can have a multi-line conditional
                // without an initial query expression:
                // {
                //   - true:  this is true
                //   - false: this is false
                // }
                InnerConditionalContent,
                InnerSequence,
                InnerExpression,
            };

            // Adapted from "OneOf" structuring rule except that in
            // order for the rule to succeed, it has to maximally
            // cover the entire string within the { }. Used to
            // differentiate between:
            //  {myVar}                 -- Expression (try first)
            //  {my content is jolly}   -- sequence with single element
            foreach (ParseRule rule in rules) {
                int ruleId = BeginRule ();

                Parsed.Object result = ParseObject(rule) as Parsed.Object;
                if (result) {

                    // Not yet at end?
                    if (Peek (Spaced (String ("}"))) == null)
                        FailRule (ruleId);

                    // Full parse of content within braces
                    else
                        return (Parsed.Object) SucceedRule (ruleId, result);

                } else {
                    FailRule (ruleId);
                }
            }

            return null;
        }

        protected Parsed.Object InnerExpression()
        {
            var expr = Parse(Expression);
            if (expr) {
                expr.outputWhenComplete = true;
            }
            return expr;
        }

        protected Identifier IdentifierWithMetadata()
        {
            var id = Identifier();
            if( id == null ) return null;

            // InkParser.RuleDidSucceed will add DebugMetadata
            return new Identifier { name = id, debugMetadata = null };
        }

        // Note: we allow identifiers that start with a number,
        // but not if they *only* comprise numbers
        protected string Identifier()
        {
            // Parse remaining characters (if any)
            var name = ParseCharactersFromCharSet (identifierCharSet);
            if (name == null)
                return null;

            // Reject if it's just a number
            bool isNumberCharsOnly = true;
            foreach (var c in name) {
                if ( !(c >= '0' && c <= '9') ) {
                    isNumberCharsOnly = false;
                    break;
                }
            }
            if (isNumberCharsOnly) {
                return null;
            }

            return name;
        }

        CharacterSet identifierCharSet {
            get {
                if (_identifierCharSet == null) {
                    (_identifierCharSet = new CharacterSet ())
                        .AddRange ('A', 'Z')
                        .AddRange ('a', 'z')
                        .AddRange ('0', '9')
                        .Add ('_');
                    // Enable non-ASCII characters for story identifiers.
                    ExtendIdentifierCharacterRanges (_identifierCharSet);
                }
                return _identifierCharSet;
            }
        }

        private CharacterSet _identifierCharSet;
    }
}

