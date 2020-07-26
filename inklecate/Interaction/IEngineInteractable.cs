using Ink.Inklecate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ink.Inklecate.Interaction
{
    /// <summary>The IEngineInteractable interface defines the interaction with the engine.</summary>
    public interface IEngineInteractable
    {
        Runtime.IStory CreateStoryFromJson(string fileTextContent);
    }
}
