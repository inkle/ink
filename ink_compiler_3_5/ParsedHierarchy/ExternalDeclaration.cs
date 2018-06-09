using System;
using System.Collections.Generic;

namespace Ink.Parsed
{
    public class ExternalDeclaration : Parsed.Object, INamedContent
    {
        public string name { get; set; }
        public List<string> argumentNames { get; set; }

        public ExternalDeclaration (string name, List<string> argumentNames)
        {
            this.name = name;
            this.argumentNames = argumentNames;
        }

        public override Ink.Runtime.Object GenerateRuntimeObject ()
        {
            story.AddExternal (this);

            // No runtime code exists for an external, only metadata
            return null;
        }
    }
}

