
namespace Ink
{
    public partial class InkParser
    {
        // Valid returned objects:
        //  - "help"
        //  - int: for choice number
        //  - Parsed.Divert
        //  - Variable declaration/assignment
        //  - Epression
        //  - Lookup debug source for character offset
        //  - Lookup debug source for runtime path
        public CommandLineInput CommandLineUserInput()
        {
            CommandLineInput result = new CommandLineInput ();

            Whitespace ();

            if (ParseString ("help") != null) {
                result.isHelp = true;
                return result;
            }

            if (ParseString ("exit") != null || ParseString ("quit") != null) {
                result.isExit = true;
                return result;
            }

            return (CommandLineInput) OneOf (
                DebugSource,
                DebugPathLookup,
                UserChoiceNumber, 
                UserImmediateModeStatement
            );
        }

        CommandLineInput DebugSource ()
        {
            Whitespace ();

            if (ParseString ("DebugSource") == null)
                return null;

            Whitespace ();

            var expectMsg = "character offset in parentheses, e.g. DebugSource(5)";
            if (Expect (String ("("), expectMsg) == null)
                return null;

            Whitespace ();

            int? characterOffset = ParseInt ();
            if (characterOffset == null) {
                Error (expectMsg);
                return null;
            }

            Whitespace ();

            Expect (String (")"), "closing parenthesis");

            var inputStruct = new CommandLineInput ();
            inputStruct.debugSource = characterOffset;
            return inputStruct;
        }

        CommandLineInput DebugPathLookup ()
        {
            Whitespace ();

            if (ParseString ("DebugPath") == null)
                return null;

            if (Whitespace () == null)
                return null;

            var pathStr = Expect (RuntimePath, "path") as string;

            var inputStruct = new CommandLineInput ();
            inputStruct.debugPathLookup = pathStr;
            return inputStruct;
        }

        string RuntimePath ()
        {
            if (_runtimePathCharacterSet == null) {
                _runtimePathCharacterSet = new CharacterSet (identifierCharSet);
                _runtimePathCharacterSet.Add ('-'); // for c-0, g-0 etc
                _runtimePathCharacterSet.Add ('.');

            }
            
            return ParseCharactersFromCharSet (_runtimePathCharacterSet);
        }

        CommandLineInput UserChoiceNumber()
        {
            Whitespace ();

            int? number = ParseInt ();
            if (number == null) {
                return null;
            }

            Whitespace ();

            if (Parse(EndOfLine) == null) {
                return null;
            }

            var inputStruct = new CommandLineInput ();
            inputStruct.choiceInput = number;
            return inputStruct;
        }

        CommandLineInput UserImmediateModeStatement()
        {
            var statement = OneOf (SingleDivert, TempDeclarationOrAssignment, Expression);

            var inputStruct = new CommandLineInput ();
            inputStruct.userImmediateModeStatement = statement;
            return inputStruct;
        }

        CharacterSet _runtimePathCharacterSet;
    }
}

