
namespace Ink.Parsed
{
    public class Tag : Parsed.Object
    {

        public Tag (bool isStart)
        {
            _isStart = isStart;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            return _isStart ? Runtime.ControlCommand.BeginTag() : Runtime.ControlCommand.EndTag();
        }

        bool _isStart;
    }
}

