using System.Collections.Generic;

namespace Ink.Runtime
{
    // Confusingly from a C# point of view, a LIST in ink is actually
    // modelled using a C# Dictionary!
    internal class RawList : Dictionary<string, int>
    {
        public RawList () { }
        public RawList (Dictionary<string, int> otherDict) : base (otherDict) { }
        public RawList (KeyValuePair<string, int> singleElement)
        {
            Add (singleElement.Key, singleElement.Value);
        }

        public KeyValuePair<string, int> maxItem {
            get {
                var max = new KeyValuePair<string, int> (null, 0);
                foreach (var kv in this) {
                    if (max.Key == null || kv.Value > max.Value)
                        max = kv;
                }
                return max;
            }
        }

        public KeyValuePair<string, int> minItem {
            get {
                var min = new KeyValuePair<string, int> (null, 0);
                foreach (var kv in this) {
                    if (min.Key == null || kv.Value < min.Value)
                        min = kv;
                }
                return min;
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
    }
}
