using Newtonsoft.Json;
using System;

namespace Inklewriter.Runtime
{
    internal class ControlCommand : Runtime.Object
    {
        public enum CommandType
        {
            NotSet = -1,
            EvalStart,
            EvalOutput,
            EvalEnd,
            Duplicate,
            PopEvaluatedValue,
            NoOp,
            ChoiceCount,
            VisitIndex,
            SequenceShuffleIndex,
            SafeExit,
            Stop
        }
            
        public CommandType commandType { get; protected set; }

        // For serialisation
        [JsonProperty("cmd")]
        [UniqueJsonIdentifier]
        public string commandName {
            get {
                return this.commandType.ToString ();
            }
            set {
                string[] enumNames = Enum.GetNames (typeof(CommandType));
                int enumIndex = Array.IndexOf (enumNames, value);
                commandType = (CommandType) Enum.GetValues(typeof(CommandType)).GetValue(enumIndex);
            }
        }

        public ControlCommand (CommandType commandType)
        {
            this.commandType = commandType;
        }

        // Require default constructor for serialisation
        public ControlCommand() : this(CommandType.NotSet) {}

        public static ControlCommand EvalStart() {
            return new ControlCommand(CommandType.EvalStart);
        }

        public static ControlCommand EvalOutput() {
            return new ControlCommand(CommandType.EvalOutput);
        }

        public static ControlCommand EvalEnd() {
            return new ControlCommand(CommandType.EvalEnd);
        }

        public static ControlCommand Duplicate() {
            return new ControlCommand(CommandType.Duplicate);
        }

        public static ControlCommand PopEvaluatedValue() {
            return new ControlCommand (CommandType.PopEvaluatedValue);
        }

        public static ControlCommand NoOp() {
            return new ControlCommand(CommandType.NoOp);
        }

        public static ControlCommand ChoiceCount() {
            return new ControlCommand(CommandType.ChoiceCount);
        }

        public static ControlCommand VisitIndex() {
            return new ControlCommand(CommandType.VisitIndex);
        }
            
        public static ControlCommand SequenceShuffleIndex() {
            return new ControlCommand(CommandType.SequenceShuffleIndex);
        }

        public static ControlCommand SafeExit() {
            return new ControlCommand (CommandType.SafeExit);
        }

        public static ControlCommand Stop() {
            return new ControlCommand (CommandType.Stop);
        }

        public override string ToString ()
        {
            return commandType.ToString();
        }
    }
}

