using System.Collections.Generic;

namespace Ink.Runtime
{
    internal class SetDictionary : Dictionary<string, int>
    {
        public SetDictionary () { }
        public SetDictionary (Dictionary<string, int> otherDict) : base (otherDict) { }
        public SetDictionary (KeyValuePair<string, int> singleElement)
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

        public SetDictionary Union (SetDictionary otherDict)
        {
            var union = new SetDictionary (this);
            foreach (var kv in otherDict) {
                union [kv.Key] = kv.Value;
            }
            return union;
        }

        public SetDictionary Intersect (SetDictionary otherDict)
        {
            var intersection = new SetDictionary ();
            foreach (var kv in this) {
                if (otherDict.ContainsKey (kv.Key))
                    intersection.Add (kv.Key, kv.Value);
            }
            return intersection;
        }

        public SetDictionary Without (SetDictionary setToRemove)
        {
            var result = new SetDictionary (this);
            foreach (var kv in setToRemove)
                result.Remove (kv.Key);
            return result;
        }

        public bool Contains (SetDictionary otherSet)
        {
            foreach (var kv in otherSet) {
                if (!this.ContainsKey (kv.Key)) return false;
            }
            return true;
        }

        public bool GreaterThan (SetDictionary otherSet)
        {
            if (Count == 0) return false;
            if (otherSet.Count == 0) return true;

            // All greater
            return minItem.Value > otherSet.maxItem.Value;
        }

        public bool GreaterThanOrEquals (SetDictionary otherSet)
        {
            if (Count == 0) return false;
            if (otherSet.Count == 0) return true;

            return minItem.Value >= otherSet.minItem.Value
                && maxItem.Value >= otherSet.maxItem.Value;
        }

        public bool LessThan (SetDictionary otherSet)
        {
            if (otherSet.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value < otherSet.minItem.Value;
        }

        public bool LessThanOrEquals (SetDictionary otherSet)
        {
            if (otherSet.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value <= otherSet.maxItem.Value
                && minItem.Value <= otherSet.minItem.Value;
        }

        public SetDictionary MaxAsSet ()
        {
            if (Count > 0)
                return new SetDictionary (maxItem);
            else
                return new SetDictionary ();
        }

        public SetDictionary MinAsSet ()
        {
            if (Count > 0)
                return new SetDictionary (minItem);
            else
                return new SetDictionary ();
        }

        public override bool Equals (object other)
        {
            var otherSetValue = other as SetDictionary;
            if (otherSetValue == null) return false;
            if (otherSetValue.Count != Count) return false;

            foreach (var kv in this) {
                if (!otherSetValue.ContainsKey (kv.Key))
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
