using System.Collections.Generic;
using System.Text;

namespace Ink.Runtime
{
    /// <summary>
    /// The underlying type for a list item in ink. It stores the original list definition
    /// name as well as the item name, but without the value of the item. When the value is
    /// stored, it's stored in a KeyValuePair of InkListItem and int.
    /// </summary>
    public struct InkListItem
    {
        /// <summary>
        /// The name of the list where the item was originally defined.
        /// </summary>
        public readonly string originName;

        /// <summary>
        /// The main name of the item as defined in ink.
        /// </summary>
        public readonly string itemName;

        /// <summary>
        /// Create an item with the given original list definition name, and the name of this
        /// item.
        /// </summary>
        public InkListItem (string originName, string itemName)
        {
            this.originName = originName;
            this.itemName = itemName;
        }

        /// <summary>
        /// Create an item from a dot-separted string of the form "listDefinitionName.listItemName".
        /// </summary>
        public InkListItem (string fullName)
        {
            var nameParts = fullName.Split ('.');
            this.originName = nameParts [0];
            this.itemName = nameParts [1];
        }

        internal static InkListItem Null {
            get {
                return new InkListItem (null, null);
            }
        }

        internal bool isNull {
            get {
                return originName == null && itemName == null;
            }
        }

        /// <summary>
        /// Get the full dot-separated name of the item, in the form "listDefinitionName.itemName".
        /// </summary>
        public string fullName {
            get {
                return (originName ?? "?") + "." + itemName;
            }
        }

        /// <summary>
        /// Get the full dot-separated name of the item, in the form "listDefinitionName.itemName".
        /// Calls fullName internally.
        /// </summary>
        public override string ToString ()
        {
            return fullName;
        }

        /// <summary>
        /// Is this item the same as another item?
        /// </summary>
        public override bool Equals (object obj)
        {
            if (obj is InkListItem) {
                var otherItem = (InkListItem)obj;
                return otherItem.itemName   == itemName 
                    && otherItem.originName == originName;
            }

            return false;
        }

        /// <summary>
        /// Get the hashcode for an item.
        /// </summary>
        public override int GetHashCode ()
        {
            int originCode = 0;
            int itemCode = itemName.GetHashCode ();
            if (originName != null)
                originCode = originName.GetHashCode ();
            
            return originCode + itemCode;
        }
    }

    /// <summary>
    /// The InkList is the underlying type that's used to store an instance of a
    /// list in ink. It's not used for the *definition* of the list, but for a list
    /// value that's stored in a variable.
    /// Somewhat confusingly, it's backed by a C# Dictionary, and has nothing to
    /// do with a C# List!
    /// </summary>
    public class InkList : Dictionary<InkListItem, int>
    {
        /// <summary>
        /// Create a new empty ink list.
        /// </summary>
        public InkList () { }

        /// <summary>
        /// Create a new ink list that contains the same contents as another list.
        /// </summary>
        public InkList (InkList otherList) : base (otherList) { _originNames = otherList.originNames; }

        /// <summary>
        /// Create a new empty ink list that's intended to hold items from a particular origin
        /// list definition. The origin Story is needed in order to be able to look up that definition.
        /// </summary>
        public InkList (string singleOriginListName, Story originStory)
        {
            SetInitialOriginName (singleOriginListName);

            ListDefinition def;
            if (originStory.listDefinitions.TryListGetDefinition (singleOriginListName, out def))
                origins = new List<ListDefinition> { def };
            else
                throw new System.Exception ("InkList origin could not be found in story when constructing new list: " + singleOriginListName);
        }

        internal InkList (KeyValuePair<InkListItem, int> singleElement)
        {
            Add (singleElement.Key, singleElement.Value);
        }

        /// <summary>
        /// Adds the given item to the ink list. Note that the item must come from a list definition that
        /// is already "known" to this list, so that the item's value can be looked up. By "known", we mean
        /// that it already has items in it from that source, or it did at one point - it can't be a 
        /// completely fresh empty list, or a list that only contains items from a different list definition.
        /// </summary>
        public void AddItem (InkListItem item)
        {
            if (item.originName == null) {
                AddItem (item.itemName);
                return;
            }
            
            foreach (var origin in origins) {
                if (origin.name == item.originName) {
                    int intVal;
                    if (origin.TryGetValueForItem (item, out intVal)) {
                        this [item] = intVal;
                        return;
                    } else {
                        throw new System.Exception ("Could not add the item " + item + " to this list because it doesn't exist in the original list definition in ink.");
                    }
                }
            }

            throw new System.Exception ("Failed to add item to list because the item was from a new list definition that wasn't previously known to this list. Only items from previously known lists can be used, so that the int value can be found.");
        }

        /// <summary>
        /// Adds the given item to the ink list, attempting to find the origin list definition that it belongs to.
        /// The item must therefore come from a list definition that is already "known" to this list, so that the
        /// item's value can be looked up. By "known", we mean that it already has items in it from that source, or
        /// it did at one point - it can't be a completely fresh empty list, or a list that only contains items from
        /// a different list definition.
        /// </summary>
        public void AddItem (string itemName)
        {
            ListDefinition foundListDef = null;

            foreach (var origin in origins) {
                if (origin.ContainsItemWithName (itemName)) {
                    if (foundListDef != null) {
                        throw new System.Exception ("Could not add the item " + itemName + " to this list because it could come from either " + origin.name + " or " + foundListDef.name);
                    } else {
                        foundListDef = origin;
                    }
                }
            }

            if (foundListDef == null)
                throw new System.Exception ("Could not add the item " + itemName + " to this list because it isn't known to any list definitions previously associated with this list.");

            var item = new InkListItem (foundListDef.name, itemName);
            var itemVal = foundListDef.ValueForItem(item);
            this [item] = itemVal;
        }

        /// <summary>
        /// Returns true if this ink list contains an item with the given short name
        /// (ignoring the original list where it was defined).
        /// </summary>
        public bool ContainsItemNamed (string itemName)
        {
            foreach (var itemWithValue in this) {
                if (itemWithValue.Key.itemName == itemName) return true;
            }
            return false;
        }

        // Story has to set this so that the value knows its origin,
        // necessary for certain operations (e.g. interacting with ints).
        // Only the story has access to the full set of lists, so that
        // the origin can be resolved from the originListName.
        internal List<ListDefinition> origins;
        internal ListDefinition originOfMaxItem {
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
        internal List<string> originNames {
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

        internal void SetInitialOriginName (string initialOriginName)
        {
            _originNames = new List<string> { initialOriginName };
        }

        internal void SetInitialOriginNames (List<string> initialOriginNames)
        {
            if (initialOriginNames == null)
                _originNames = null;
            else
                _originNames = new List<string>(initialOriginNames);
        }

        /// <summary>
        /// Get the maximum item in the list, equivalent to calling LIST_MAX(list) in ink.
        /// </summary>
        public KeyValuePair<InkListItem, int> maxItem {
            get {
                KeyValuePair<InkListItem, int> max = new KeyValuePair<InkListItem, int>();
                foreach (var kv in this) {
                    if (max.Key.isNull || kv.Value > max.Value)
                        max = kv;
                }
                return max;
            }
        }

        /// <summary>
        /// Get the minimum item in the list, equivalent to calling LIST_MIN(list) in ink.
        /// </summary>
        public KeyValuePair<InkListItem, int> minItem {
            get {
                var min = new KeyValuePair<InkListItem, int> ();
                foreach (var kv in this) {
                    if (min.Key.isNull || kv.Value < min.Value)
                        min = kv;
                }
                return min;
            }
        }

        /// <summary>
        /// The inverse of the list, equivalent to calling LIST_INVERSE(list) in ink
        /// </summary>
        public InkList inverse {
            get {
                var list = new InkList ();
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

        /// <summary>
        /// The list of all items from the original list definition, equivalent to calling
        /// LIST_ALL(list) in ink.
        /// </summary>
        public InkList all {
            get {
                var list = new InkList ();
                if (origins != null) {
                    foreach (var origin in origins) {
                        foreach (var itemAndValue in origin.items)
                            list[itemAndValue.Key] = itemAndValue.Value;
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// Returns a new list that is the combination of the current list and one that's
        /// passed in. Equivalent to calling (list1 + list2) in ink.
        /// </summary>
        public InkList Union (InkList otherList)
        {
            var union = new InkList (this);
            foreach (var kv in otherList) {
                union [kv.Key] = kv.Value;
            }
            return union;
        }

        /// <summary>
        /// Returns a new list that is the intersection of the current list with another
        /// list that's passed in - i.e. a list of the items that are shared between the
        /// two other lists. Equivalent to calling (list1 ^ list2) in ink.
        /// </summary>
        public InkList Intersect (InkList otherList)
        {
            var intersection = new InkList ();
            foreach (var kv in this) {
                if (otherList.ContainsKey (kv.Key))
                    intersection.Add (kv.Key, kv.Value);
            }
            return intersection;
        }

        /// <summary>
        /// Returns a new list that's the same as the current one, except with the given items
        /// removed that are in the passed in list. Equivalent to calling (list1 - list2) in ink.
        /// </summary>
        /// <param name="listToRemove">List to remove.</param>
        public InkList Without (InkList listToRemove)
        {
            var result = new InkList (this);
            foreach (var kv in listToRemove)
                result.Remove (kv.Key);
            return result;
        }

        /// <summary>
        /// Returns true if the current list contains all the items that are in the list that
        /// is passed in. Equivalent to calling (list1 ? list2) in ink.
        /// </summary>
        /// <param name="otherList">Other list.</param>
        public bool Contains (InkList otherList)
        {
            foreach (var kv in otherList) {
                if (!this.ContainsKey (kv.Key)) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if all the item values in the current list are greater than all the
        /// item values in the passed in list. Equivalent to calling (list1 > list2) in ink.
        /// </summary>
        public bool GreaterThan (InkList otherList)
        {
            if (Count == 0) return false;
            if (otherList.Count == 0) return true;

            // All greater
            return minItem.Value > otherList.maxItem.Value;
        }

        /// <summary>
        /// Returns true if the item values in the current list overlap or are all greater than
        /// the item values in the passed in list. None of the item values in the current list must
        /// fall below the item values in the passed in list. Equivalent to (list1 >= list2) in ink,
        /// or LIST_MIN(list1) >= LIST_MIN(list2) &amp;&amp; LIST_MAX(list1) >= LIST_MAX(list2).
        /// </summary>
        public bool GreaterThanOrEquals (InkList otherList)
        {
            if (Count == 0) return false;
            if (otherList.Count == 0) return true;

            return minItem.Value >= otherList.minItem.Value
                && maxItem.Value >= otherList.maxItem.Value;
        }

        /// <summary>
        /// Returns true if all the item values in the current list are less than all the
        /// item values in the passed in list. Equivalent to calling (list1 &lt; list2) in ink.
        /// </summary>
        public bool LessThan (InkList otherList)
        {
            if (otherList.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value < otherList.minItem.Value;
        }

        /// <summary>
        /// Returns true if the item values in the current list overlap or are all less than
        /// the item values in the passed in list. None of the item values in the current list must
        /// go above the item values in the passed in list. Equivalent to (list1 &lt;= list2) in ink,
        /// or LIST_MAX(list1) &lt;= LIST_MAX(list2) &amp;&amp; LIST_MIN(list1) &lt;= LIST_MIN(list2).
        /// </summary>
        public bool LessThanOrEquals (InkList otherList)
        {
            if (otherList.Count == 0) return false;
            if (Count == 0) return true;

            return maxItem.Value <= otherList.maxItem.Value
                && minItem.Value <= otherList.minItem.Value;
        }

        internal InkList MaxAsList ()
        {
            if (Count > 0)
                return new InkList (maxItem);
            else
                return new InkList ();
        }

        internal InkList MinAsList ()
        {
            if (Count > 0)
                return new InkList (minItem);
            else
                return new InkList ();
        }

        /// <summary>
        /// Returns true if the passed object is also an ink list that contains
        /// the same items as the current list, false otherwise.
        /// </summary>
        public override bool Equals (object other)
        {
            var otherRawList = other as InkList;
            if (otherRawList == null) return false;
            if (otherRawList.Count != Count) return false;

            foreach (var kv in this) {
                if (!otherRawList.ContainsKey (kv.Key))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Return the hashcode for this object, used for comparisons and inserting into dictionaries.
        /// </summary>
        public override int GetHashCode ()
        {
            int ownHash = 0;
            foreach (var kv in this)
                ownHash += kv.Key.GetHashCode ();
            return ownHash;
        }

        /// <summary>
        /// Returns a string in the form "a, b, c" with the names of the items in the list, without
        /// the origin list definition names. Equivalent to writing {list} in ink.
        /// </summary>
        public override string ToString ()
        {
            var ordered = new List<KeyValuePair<InkListItem, int>> ();
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
