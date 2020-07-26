using Ink.Inklecate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ink.Inklecate.Interaction
{
    public class CompilerInteractor : ICompilerInteractable
    {
        public IInkCompiler CreateCompiler(string fileTextContent, CompilerOptions compilerOptions)
        {
            return new Compiler(fileTextContent, compilerOptions);
        }
    }
}
