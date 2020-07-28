
namespace Ink.Parsed
{
    public class IncludedFile : Parsed.Object
    {
        public Parsed.Fiction includedStory { get; private set; }

        public IncludedFile (Parsed.Fiction includedFiction)
        {
            this.includedStory = includedFiction;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            // Left to the main story to process
            return null;
        }
    }
}

