using Ink.Inklecate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ink.Inklecate.Interaction
{
    /// <summary>The ICompilerInteractable interface defines the interaction with the compiler.</summary>
    public interface ICompilerInteractable
    {
        IInkCompiler CreateCompiler(string fileTextContent, CompilerOptions compilerOptions);
    }
}
