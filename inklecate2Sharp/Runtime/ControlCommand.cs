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
            NoOp
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

        public static ControlCommand NoOp() {
            return new ControlCommand(CommandType.NoOp);
        }

        public override string ToString ()
        {
            return commandType.ToString();
        }
    }
}

