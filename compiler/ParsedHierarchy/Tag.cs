
namespace Ink.Parsed
{
    public class Tag : Parsed.Object
    {

        public bool isStart;
        public bool inChoice;
        
        public override Runtime.Object GenerateRuntimeObject ()
        {
            if( isStart )
                return Runtime.ControlCommand.BeginTag();
            else
                return Runtime.ControlCommand.EndTag();
        }

        public override string ToString ()
        {
            if( isStart )
                return "#StartTag";
            else
                return "#EndTag";
        }
    }
}

