
namespace Inklewriter
{
    internal partial class InkParser
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
            Whitespace ();

            int? number = ParseInt ();
            if (number == null) {
                return null;
            }

            Whitespace ();

            if (Parse(EndOfLine) == null) {
                return null;
            }

            return number;
        }

        object UserImmediateModeStatement()
        {
            return OneOf (SingleDivert, ProceduralVarDeclarationOrAssignment, Expression);
        }
    }
}

