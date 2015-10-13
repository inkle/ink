namespace Inklewriter.Parsed
{
    internal class TunnelOnwards : Parsed.Object
    {
        public override Runtime.Object GenerateRuntimeObject ()
        {
            return new Runtime.PushPop (Runtime.PushPop.Type.Tunnel, Runtime.PushPop.Direction.Pop);
        }
    }
}

