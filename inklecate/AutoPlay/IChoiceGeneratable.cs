using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Inklecate.AutoPlay
{
    public interface IChoiceGeneratable
    {
        int GetRandomChoice(int choiseCount);
    }
}
