using System;

namespace Ink
{
    public interface IPlugin
    {  
        // Hooks: if in doubt use PostExport, since the parsedStory is in a more finalised state.

        void PreParse(ref string storyContent);
        
        // Hook for immediately after the story has been parsed into its basic Parsed hierarchy.
        // Could be useful for modifying the story before it's exported.
        void PostParse(ref Parsed.Story parsedStory);

        // Hook for after parsed story has been converted into its runtime equivalent. Note that
        // during this process the parsed story will have changed structure too, to take into 
        // account analysis of the structure of Weave, for example.
        void PostExport(Parsed.Story parsedStory, ref Runtime.Story runtimeStory);
    }
}

