
namespace Ink.Parsed
{
    
	internal class CharacterRangeInlcude : Parsed.Object
	{
		public string rangeName { get; private set; }

		public CharacterRangeInlcude (string rangeName)
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
