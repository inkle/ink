using System;
using System.Collections.Generic;

namespace Ink.Runtime
{
    internal class StoryLookups
    {
        public List<string> visitCountNames;
        public List<string> turnIndexNames;

        public StoryLookups()
        {
            visitCountNames = new List<string>();
            turnIndexNames = new List<string>();
        }

        public StoryLookups(List<string> visitCountNames, List<string> turnIndexNames)
        {
            this.visitCountNames = visitCountNames;
            this.turnIndexNames = turnIndexNames;
        }
    }
}
