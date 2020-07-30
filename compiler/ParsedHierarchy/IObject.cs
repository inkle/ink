namespace Ink.Parsed
{
    public interface IObject
    {
        Object parent { get; set; }
        void ResolveReferences(IFiction context);
    }
}