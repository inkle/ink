using System;

namespace Inklewriter.Runtime
{
    public class ControlCommand : Runtime.Object
    {
        public enum CommandType
        {
            EvalStart,
            EvalOutput,
            EvalEnd,
            StackPush,
            StackPop,
            Duplicate,
            NoOp,
            ChoiceCount,
            SequenceCount,
            SequenceIncrement
        }

        public CommandType commandType { get; protected set; }

        public ControlCommand (CommandType commandType)
        {
            this.commandType = commandType;
        }

        public static ControlCommand EvalStart() {
            return new ControlCommand(CommandType.EvalStart);
        }

        public static ControlCommand EvalOutput() {
            return new ControlCommand(CommandType.EvalOutput);
        }

        public static ControlCommand EvalEnd() {
            return new ControlCommand(CommandType.EvalEnd);
        }

        public static ControlCommand StackPush() {
            return new ControlCommand(CommandType.StackPush);
        }

        public static ControlCommand StackPop() {
            return new ControlCommand(CommandType.StackPop);
        }

        public static ControlCommand Duplicate() {
            return new ControlCommand(CommandType.Duplicate);
        }

        public static ControlCommand NoOp() {
            return new ControlCommand(CommandType.NoOp);
        }

        public static ControlCommand ChoiceCount() {
            return new ControlCommand(CommandType.ChoiceCount);
        }

        public static ControlCommand SequenceCount() {
            return new ControlCommand(CommandType.SequenceCount);
        }

        public static ControlCommand SequenceIncrement() {
            return new ControlCommand(CommandType.SequenceIncrement);
        }

        public override string ToString ()
        {
            return commandType.ToString();
        }
    }
}

