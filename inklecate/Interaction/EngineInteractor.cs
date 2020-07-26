using Ink.Inklecate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ink.Inklecate.Interaction
{
    public class EngineInteractor : IEngineInteractable
    {
        public Runtime.IStory CreateStoryFromJson(string fileTextContent)
        {
            return new Runtime.Story(fileTextContent);
        }
    }
}
