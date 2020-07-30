using Ink.Runtime;
using System.Collections.Generic;

namespace Ink.Parsed
{
    public interface IFiction : IObject
    {
        Runtime.Story ExportRuntime(ErrorHandler errorHandler = null);
        void TryAddNewVariableDeclaration(VariableAssignment varDecl);
        FlowBase.VariableResolveResult ResolveVariableWithName(string varName, Parsed.Object fromNode);
        void CheckForNamingCollisions(Parsed.Object obj, string name, Fiction.SymbolType symbolType, string typeNameOverride = null);
        bool IsExternal(string namedFuncTarget);
        Dictionary<string, ExternalDeclaration> externals { get; set; }
        bool hadError { get; }
        void ResetError();
    }
}