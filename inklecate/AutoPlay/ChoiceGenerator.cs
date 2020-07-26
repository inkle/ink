using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Inklecate.AutoPlay
{
    public class ChoiceGenerator : IChoiceGeneratable
    {
        Random _randomizer = new Random();

        public int GetRandomChoice(int choiseCount)
        {
            return _randomizer.Next() % choiseCount;
        }
    }
}