using System.Collections.Generic;

namespace Ink.Runtime
{
    internal class Set
    {
        public string name { get { return _name; } }

        public Set (string name, Dictionary<string, int> namedItems)
        {
            _name = name;
            _namedItems = namedItems;
        }

        string _name;
        Dictionary<string, int> _namedItems;
    }
}
