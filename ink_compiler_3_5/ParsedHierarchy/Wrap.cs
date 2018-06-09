﻿
namespace Ink.Parsed
{
    internal class Wrap<T> : Parsed.Object where T : Runtime.Object
    {
        public Wrap (T objToWrap)
        {
            _objToWrap = objToWrap;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            return _objToWrap;
        }

        T _objToWrap;
    }

    // Shorthand for writing Parsed.Wrap<Runtime.Glue> and Parsed.Wrap<Runtime.Tag>
    internal class Glue : Wrap<Runtime.Glue> {
        public Glue (Runtime.Glue glue) : base(glue) {}
    }
    internal class Tag : Wrap<Runtime.Tag> {
        public Tag (Runtime.Tag tag) : base (tag) { }
    }
}

