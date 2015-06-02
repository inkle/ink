using System;

namespace Inklewriter
{
    public partial class InkParser
    {
        // Valid returned objects:
        //  - "help"
        //  - int: for choice number
        //  - Parsed.Divert
        //  - Variable declaration/assignment
        //  - Epression
        public object CommandLineUserInput()
        {
            return OneOf (Spaced(String("help")), UserChoiceNumber, UserImmediateModeStatement);
        }

        object UserChoiceNumber()
        {
            BeginRule ();

            Whitespace ();

            int? number = ParseInt ();
            if (number == null) {
                return FailRule ();
            }

            Whitespace ();

            if (EndOfLine () == null) {
                return FailRule ();
            }

            return SucceedRule(number);
        }

        object UserImmediateModeStatement()
        {
            return OneOf (Divert, VariableDeclarationOrAssignment, Expression);
        }
    }
}

