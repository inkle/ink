using System;

namespace Ink.Runtime
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
            PopFunction,
            PopTunnel,
            BeginString,
            EndString,
            NoOp,
            ChoiceCount,
            TurnsSince,
            ReadCount,
            Random,
            SeedRandom,
            VisitIndex,
            SequenceShuffleIndex,
            StartThread,
            Done,
            End,
            ListFromInt,
            ListRange,
            //----
            TOTAL_VALUES
        }
            
        public CommandType commandType { get; protected set; }

        public ControlCommand (CommandType commandType)
        {
            this.commandType = commandType;
        }

        // Require default constructor for serialisation
        public ControlCommand() : this(CommandType.NotSet) {}

        internal override Object Copy()
        {
            return new ControlCommand (commandType);
        }

        // The following static factory methods are to make generating these objects
        // slightly more succinct. Without these, the code gets pretty massive! e.g.
        //
        //     var c = new Runtime.ControlCommand(Runtime.ControlCommand.CommandType.EvalStart)
        // 
        // as opposed to
        //
        //     var c = Runtime.ControlCommand.EvalStart()

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

        public static ControlCommand PopFunction() {
            return new ControlCommand (CommandType.PopFunction);
        }

        public static ControlCommand PopTunnel() {
            return new ControlCommand (CommandType.PopTunnel);
        }
            
        public static ControlCommand BeginString() {
            return new ControlCommand (CommandType.BeginString);
        }

        public static ControlCommand EndString() {
            return new ControlCommand (CommandType.EndString);
        }

        public static ControlCommand NoOp() {
            return new ControlCommand(CommandType.NoOp);
        }

        public static ControlCommand ChoiceCount() {
            return new ControlCommand(CommandType.ChoiceCount);
        }

        public static ControlCommand TurnsSince() {
            return new ControlCommand(CommandType.TurnsSince);
        }

        public static ControlCommand ReadCount ()
        {
            return new ControlCommand (CommandType.ReadCount);
        }

        public static ControlCommand Random ()
        {
            return new ControlCommand (CommandType.Random);
        }

        public static ControlCommand SeedRandom ()
        {
            return new ControlCommand (CommandType.SeedRandom);
        }

        public static ControlCommand VisitIndex() {
            return new ControlCommand(CommandType.VisitIndex);
        }
            
        public static ControlCommand SequenceShuffleIndex() {
            return new ControlCommand(CommandType.SequenceShuffleIndex);
        }

        public static ControlCommand StartThread() {
            return new ControlCommand (CommandType.StartThread);
        }

        public static ControlCommand Done() {
            return new ControlCommand (CommandType.Done);
        }

        public static ControlCommand End() {
            return new ControlCommand (CommandType.End);
        }

        public static ControlCommand ListFromInt () {
            return new ControlCommand (CommandType.ListFromInt);
        }

        public static ControlCommand ListRange ()
        {
            return new ControlCommand (CommandType.ListRange);
        }

        public override string ToString ()
        {
            return commandType.ToString();
        }
    }
}

