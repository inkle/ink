
namespace Ink.Parsed
{
    internal class IncludedFile : Parsed.Object
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
    internal class CharacterRange : Parsed.Object
    {
        public string rangeName { get; private set; }

        public CharacterRange (string rangeName)
        {
            this.rangeName = rangeName;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            // Left to the main story to process
            return null;
        }
    }
}

