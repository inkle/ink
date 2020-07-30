
namespace Ink
{
    public interface IInkCompiler
    {
        Parsed.Fiction ParsedFiction { get; }

        Runtime.Story Compile(out Parsed.Fiction parsedFiction);
        Parsed.Fiction Parse();
    }
}