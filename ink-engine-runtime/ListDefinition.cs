using System.Collections.Generic;

namespace Ink.Runtime
{
    internal class ListDefinition
    {
        public string name { get { return _name; } }

        public Dictionary<string, int> items {
            get {
                return _items;
            }
        }

        public int ValueForItem (string itemName)
        {
            int intVal;
            if (_items.TryGetValue (itemName, out intVal))
                return intVal;
            else
                return 0;
        }

        public bool TryGetValueForItem (string itemName, out int val)
        {
            return _items.TryGetValue (itemName, out val);
        }

        public bool TryGetItemWithValue (int val, out string itemName)
        {
            itemName = null;

            foreach (var namedItem in _items) {
                if (namedItem.Value == val) {
                    itemName = namedItem.Key;
                    return true;
                }
            }

            return false;
        }

        public ListValue ListRange (int min, int max)
        {
            var rawList = new RawList ();
            foreach (var namedItem in _items) {
                if (namedItem.Value >= min && namedItem.Value <= max) {
                    rawList [name + "." + namedItem.Key] = namedItem.Value;
                }
            }
            return new ListValue(rawList);
        }

        public ListDefinition (string name, Dictionary<string, int> items)
        {
            _name = name;
            _items = items;
        }

        string _name;
        Dictionary<string, int> _items;
    }
}
