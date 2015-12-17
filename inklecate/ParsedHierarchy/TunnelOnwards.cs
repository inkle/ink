namespace Ink.Parsed
{
    internal class TunnelOnwards : Parsed.Object
    {
        public override Runtime.Object GenerateRuntimeObject ()
        {
            return new Runtime.Pop (Runtime.PushPopType.Tunnel);
        }
    }
}

