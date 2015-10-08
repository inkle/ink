namespace Inklewriter.Parsed
{
    internal class TunnelOnwards : Parsed.Object
    {
        public override Runtime.Object GenerateRuntimeObject ()
        {
            return Runtime.ControlCommand.StackPop ();
        }
    }
}

