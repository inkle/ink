
namespace Ink.Parsed
{
    public class IncludedFile : Parsed.Object
    {
        public Parsed.Story includedStory { get; private set; }

        public IncludedFile (Parsed.Story includedStory)
        {
            this.includedStory = includedStory;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            // Left to the main story to process
            return null;
        }
    }
}

