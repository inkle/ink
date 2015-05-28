using System;

namespace Inklewriter.Runtime
{
    public class EvaluationCommand : Runtime.Object
    {
        public enum CommandType
        {
            Start,
            Output,
            End
        }

        public CommandType commandType { get; protected set; }

        public EvaluationCommand (CommandType commandType)
        {
            this.commandType = commandType;
        }

        public static EvaluationCommand Start() {
            return new EvaluationCommand(CommandType.Start);
        }

        public static EvaluationCommand Output() {
            return new EvaluationCommand(CommandType.Output);
        }

        public static EvaluationCommand End() {
            return new EvaluationCommand(CommandType.End);
        }

        public override string ToString ()
        {
            return "Evaluation"+commandType.ToString();
        }
    }
}

