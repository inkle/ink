using System.Collections.Generic;
using System.Text;

namespace Ink.Runtime
{
    internal struct RawListItem
    {
        public readonly string originName;
        public readonly string itemName;

        public RawListItem (string originName, string itemName)
        {
            this.originName = originName;
            this.itemName = itemName;
        }

        public RawListItem (string fullName)
        {
            var nameParts = fullName.Split ('.');
            this.originName = nameParts [0];
            this.itemName = nameParts [1];
        }

        public static RawListItem Null {
            get {
                return new RawListItem (null, null);
            }
        }

        public bool isNull {
            get {
                return originName == null && itemName == null;
            }
        }

        public string fullName {
            get {
                return (originName ?? "?") + "." + itemName;
            }
        }

        public override string ToString ()
        {
            return fullName;
        }

        public override bool Equals (object obj)
        {
            if (obj is RawListItem) {
                var otherItem = (RawListItem)obj;
                return otherItem.itemName   == itemName 
                    && otherItem.originName == originName;
            }

            return false;
        }

        public override int GetHashCode ()
        {
            int originCode = 0;
            int itemCode = itemName.GetHashCode ();
            if (originName != null)
                originCode = originName.GetHashCode ();
            
            return originCode + itemCode;
        }
    }

    // Confusingly from a C# point of view, a LIST in ink is actually
    // modelled using a C# Dictionary!
    internal class RawList : Dictionary<RawListItem, int>
    {
        public RawList () { }
        public RawList (RawList otherList) : base (otherList) { _originNames = otherList.originNames; }
        public RawList (KeyValuePair<RawListItem, int> singleElement)
        {
            Add (singleElement.Key, singleElement.Value);
        }

        // Story has to set this so that the value knows its origin,
        // necessary for certain operations (e.g. interacting with ints).
        // Only the story has access to the full set of lists, so that
        // the origin can be resolved from the originListName.
        public List<ListDefinition> origins;
        public ListDefinition originOfMaxItem {
            get {
                if (origins == null) return null;

                var maxOriginName = maxItem.Key.originName;
                foreach (var origin in origins) {
                    if (origin.name == maxOriginName)
                        return origin;
                }

                return null;
            }
        }

        // Origin name needs to be serialised when content is empty,
        // assuming a name is availble, for list definitions with variable
        // that is currently empty.
        public List<string> originNames {
            get {
                if (this.Count > 0) {
                    if (_originNames == null && this.Count > 0)
                        _originNames = new List<string> ();
                    else
                        _originNames.Clear ();

                    foreach (var itemAndValue in this)
                        _originNames.Add (itemAndValue.Key.originName);
                }

                return _originNames;
            }
        }
        List<string> _originNames;

        public void SetInitialOriginName (string initialOriginName)
        {
            _originNames = new List<string> { initialOriginName };
        }

        public void SetInitialOriginNames (List<string> initialOriginNames)
        {
            _originNames = new List<string>(initialOriginNames);
        }

        public KeyValuePair<RawListItem, int> maxItem {
            get {
                KeyValuePair<RawListItem, int> max = new KeyValuePair<RawListItem, int>();
                foreach (var kv in this) {
                    if (max.Key.isNull || kv.Value > max.Value)
                        max = kv;
                }
                return max;
            }
        }

        public KeyValuePair<RawListItem, int> minItem {
            get {
                var min = new KeyValuePair<RawListItem, int> ();
                foreach (var kv in this) {
                    if (min.Key.isNull || kv.Value < min.Value)
                        min = kv;
                }
                return min;
            }
        }

        public RawList inverse {
            get {
                var list = new RawList ();
                if (origins != null) {
                    foreach (var origin in origins) {
                        foreach (var itemAndValue in origin.items) {
                            if (!this.ContainsKey (itemAndValue.Key))
                                list.Add (itemAndValue.Key, itemAndValue.Value);
                        }
                    }

                }
                return list;
            }
        }

        public RawList all {
            get {
                var list = new RawList ();
                if (origins != null) {
                    foreach (var origin in origins) {
                        foreach (var itemAndValue in origin.items)
                            list[itemAndValue.Key] = itemAndValue.Value;
                    }
                }
                return list;
            }
        }

        public RawList Union (RawList otherList)
        {
            var union = new RawList (this);
            foreach (var kv in otherList) {
                union [kv.Key] = kv.Value;
            }
            return union;
        }

        public RawList Intersect (RawList otherList)
        {
            var intersection = new RawList ();
            foreach (var kv in this) {
                if (otherList.ContainsKey (kv.Key))
                    intersection.Add (kv.Key, kv.Value);
            }
            return intersection;
        }

        public RawList Without (RawList listToRemove)
        {
            var result = new RawList (this);
            foreach (var kv in listToRemove)
                result.Remove (kv.Key);
            return result;
        }

        public bool Contains (RawList otherList)
        {
            foreach (var kv in otherList) {
                if (!this.ContainsKey (kv.Key)) return false;
            }
            return true;
        }

        public bool GreaterThan (RawList otherList)
        {
            if (Count == 0) return false;
            if (otherList.Count == 0) return true;

            // All greater
            return minItem.Value > otherList.maxItem.Value;
        }

        public bool GreaterThanOrEquals (RawList otherList)
        {
            if (Count == 0) return false;
            if (otherList.Count == 0) return true;

            return minItem.Value >= otherList.minItem.Value
                && maxItem.Value >= otherList.maxItem.Value;
        }

        public bool LessThan (RawList otherList)
        {
            if (otherList.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value < otherList.minItem.Value;
        }

        public bool LessThanOrEquals (RawList otherList)
        {
            if (otherList.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value <= otherList.maxItem.Value
                && minItem.Value <= otherList.minItem.Value;
        }

        public RawList MaxAsList ()
        {
            if (Count > 0)
                return new RawList (maxItem);
            else
                return new RawList ();
        }

        public RawList MinAsList ()
        {
            if (Count > 0)
                return new RawList (minItem);
            else
                return new RawList ();
        }

        public override bool Equals (object other)
        {
            var otherRawList = other as RawList;
            if (otherRawList == null) return false;
            if (otherRawList.Count != Count) return false;

            foreach (var kv in this) {
                if (!otherRawList.ContainsKey (kv.Key))
                    return false;
            }

            return true;
        }

        public override int GetHashCode ()
        {
            int ownHash = 0;
            foreach (var kv in this)
                ownHash += kv.Key.GetHashCode ();
            return ownHash;
        }

        public override string ToString ()
        {
            var ordered = new List<KeyValuePair<RawListItem, int>> ();
            ordered.AddRange (this);
            ordered.Sort ((x, y) => x.Value.CompareTo (y.Value));

            var sb = new StringBuilder ();
            for (int i = 0; i < ordered.Count; i++) {
                if (i > 0)
                    sb.Append (", ");

                var item = ordered [i].Key;
                sb.Append (item.itemName);
            }

            return sb.ToString ();
        }
    }
}
