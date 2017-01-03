using System.Collections.Generic;

namespace Ink.Runtime
{
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

        public RawList Union (RawList otherDict)
        {
            var union = new RawList (this);
            foreach (var kv in otherDict) {
                union [kv.Key] = kv.Value;
            }
            return union;
        }

        public RawList Intersect (RawList otherDict)
        {
            var intersection = new RawList ();
            foreach (var kv in this) {
                if (otherDict.ContainsKey (kv.Key))
                    intersection.Add (kv.Key, kv.Value);
            }
            return intersection;
        }

        public RawList Without (RawList setToRemove)
        {
            var result = new RawList (this);
            foreach (var kv in setToRemove)
                result.Remove (kv.Key);
            return result;
        }

        public bool Contains (RawList otherSet)
        {
            foreach (var kv in otherSet) {
                if (!this.ContainsKey (kv.Key)) return false;
            }
            return true;
        }

        public bool GreaterThan (RawList otherSet)
        {
            if (Count == 0) return false;
            if (otherSet.Count == 0) return true;

            // All greater
            return minItem.Value > otherSet.maxItem.Value;
        }

        public bool GreaterThanOrEquals (RawList otherSet)
        {
            if (Count == 0) return false;
            if (otherSet.Count == 0) return true;

            return minItem.Value >= otherSet.minItem.Value
                && maxItem.Value >= otherSet.maxItem.Value;
        }

        public bool LessThan (RawList otherSet)
        {
            if (otherSet.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value < otherSet.minItem.Value;
        }

        public bool LessThanOrEquals (RawList otherSet)
        {
            if (otherSet.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value <= otherSet.maxItem.Value
                && minItem.Value <= otherSet.minItem.Value;
        }

        public RawList MaxAsSet ()
        {
            if (Count > 0)
                return new RawList (maxItem);
            else
                return new RawList ();
        }

        public RawList MinAsSet ()
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
