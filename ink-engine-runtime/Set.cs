using System.Collections.Generic;

namespace Ink.Runtime
{
    internal class Set
    {
        public string name { get { return _name; } }

        public int ValueForItem (string itemName)
        {
            int intVal;
            if (_namedItems.TryGetValue (itemName, out intVal))
                return intVal;
            else
                return 0;
        }

        public bool TryGetValueForItem (string itemName, out int val)
        {
            return _namedItems.TryGetValue (itemName, out val);
        }

        public bool TryGetItemWithValue (int val, out string itemName)
        {
            itemName = null;

            foreach (var namedItem in _namedItems) {
                if (namedItem.Value == val) {
                    itemName = namedItem.Key;
                    return true;
                }
            }

            return false;
        }

        public Set (string name, Dictionary<string, int> namedItems)
        {
            _name = name;
            _namedItems = namedItems;
        }

        string _name;
        Dictionary<string, int> _namedItems;
    }
}
