using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Ink.Runtime
{
    /// <summary>
    /// The underlying type for a stack value in ink.
    /// </summary>
    public class InkStack: IEnumerable<Value>
    {
        /// <summary>
        /// Create a new empty stack
        /// </summary>
        public InkStack() {
            _values = new List<Value>();
        }

        /// <summary>
        /// Create a new ink stack that contains copies of the values in
        /// an other stack
        /// </summary>
        public InkStack(InkStack otherStack)
        {
            _values = new List<Value>(otherStack);
        }

        /// <summary>
        /// Create a new empty stack
        /// </summary>
        public InkStack(IEnumerable<Value> values) {
            _values = new List<Value>(values);
        }

        /// <summary>
        /// Create a new ink stack that contains a single value
        /// </summary>
        public InkStack(Value v)
        {
            _values = new List<Value>();
            _values.Add(v);
        }

        /// <summary>
        /// Adds the given item to the ink stack.
        /// </summary>
        public InkStack AddItem(Value value)
        {
            return Addition(new InkStack(value));
        }

        /// <summary>
        /// Returns a new stack that is the combination of the current stack and one that's
        /// passed in. Equivalent to calling (stack1 + stack2) in ink.
        /// </summary>
        public InkStack Addition(InkStack otherStack)
        {
            var result = new InkStack(this);
            result._values.AddRange(otherStack);
            return result;
        }

        /// <summary>
        /// Returns a new stack that has the items in the second stack removed;
        /// if there are duplicate values in the original stack, only the oldest
        /// instance will be removed.
        /// </summary>
        public InkStack Subtract(InkStack otherStack)
        {
            var result = new InkStack(this);
            foreach(var v in otherStack._values) {
                var found = result._values.FindIndex((target) => target.valueObject.Equals(v.valueObject));
                if(found != -1) {
                    result._values.RemoveAt(found);
                }
            }
            return result;
        }

        public int Count {
            get { return _values.Count; }
        }

        /// <summary>
        /// Returns true if the passed object is also an ink stack that contains
        /// the same items as the current stack, false otherwise.
        /// </summary>
        public override bool Equals(object other)
        {
            var otherRawStack = other as InkStack;
            if (otherRawStack == null) return false;
            if (otherRawStack.Count != Count) return false;

            for (int i = 0; i < Count; i++)
            {
                if (!otherRawStack._values[i].valueObject.Equals(_values[i].valueObject))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Take the most recently added item from the stack (if one exists)
        /// </summary>
        public InkStack PopNewest(out Runtime.Object popped) {
            if(Count == 0) {
                popped = new Runtime.Void();
                return this;
            } else {
                var resultStack = new InkStack(this);
                popped = resultStack._values[resultStack.Count - 1];
                resultStack._values.RemoveAt(resultStack.Count - 1);
                return resultStack;
            }
        }

        /// <summary>
        /// Take the least recently added item from the stack (if one exists)
        /// </summary>
        public InkStack PopOldest(out Runtime.Object popped) {
            if(Count == 0) {
                popped = new Runtime.Void();
                return this;
            } else {
                var resultStack = new InkStack(this);
                popped = resultStack._values[0];
                resultStack._values.RemoveAt(0);
                return resultStack;
            }
        }

        /// <summary>
        /// Take the Nth item from the stack (if one exists)
        /// </summary>
        public InkStack PopNth(out Runtime.Object popped, int index) {
            if(Count <= index || index < 0) {
                popped = new Runtime.Void();
                return this;
            } else {
                var resultStack = new InkStack(this);
                popped = resultStack._values[index];
                resultStack._values.RemoveAt(index);
                return resultStack;
            }
        }

        /// <summary>
        /// Return the hashcode for this object, used for comparisons and inserting into dictionaries.
        /// </summary>
        public override int GetHashCode()
        {
            int ownHash = 0;
            foreach (var v in this)
                ownHash += v.GetHashCode();
            return ownHash;
        }

        /// <summary>
        /// Returns a string in the form "a, b, c" with the items in the stack in order.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                sb.Append(_values[i].ToString());
            }

            return sb.ToString();
        }

        List<Value> _values;

        IEnumerator<Value> IEnumerable<Value>.GetEnumerator() {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _values.GetEnumerator();
        }
    }
}
