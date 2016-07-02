
namespace Ink
{
    public class CommandLineInput
    {
        public bool isHelp;
        public bool isExit;
        public int? choiceInput;
        public int? debugSource;
        public object userImmediateModeStatement;
    }

    internal partial class InkParser
    {
        // Valid returned objects:
        //  - "help"
        //  - int: for choice number
        //  - Parsed.Divert
        //  - Variable declaration/assignment
        //  - Epression
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

            return (CommandLineInput) OneOf (DebugSource, UserChoiceNumber, UserImmediateModeStatement);
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
    }
}

