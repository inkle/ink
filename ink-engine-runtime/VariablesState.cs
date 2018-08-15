using System.Collections.Generic;

namespace Ink.Runtime
{
    /// <summary>
    /// Encompasses all the global variables in an ink Story, and
    /// allows binding of a VariableChanged event so that that game
    /// code can be notified whenever the global variables change.
    /// </summary>
	public class VariablesState : IEnumerable<string>
    {
        internal delegate void VariableChanged(string variableName, Runtime.Object newValue);
        internal event VariableChanged variableChangedEvent;

        internal bool batchObservingVariableChanges 
        { 
            get {
                return _batchObservingVariableChanges;
            }
            set { 
                _batchObservingVariableChanges = value;
                if (value) {
                    _changedVariables = new HashSet<string> ();
                } 

                // Finished observing variables in a batch - now send 
                // notifications for changed variables all in one go.
                else {
                    if (_changedVariables != null) {
                        foreach (var variableName in _changedVariables) {
                            var currentValue = _globalVariables [variableName];
                            variableChangedEvent (variableName, currentValue);
                        }
                    }

                    _changedVariables = null;
                }
            }
        }
        bool _batchObservingVariableChanges;

        // Allow StoryState to change the current callstack, e.g. for
        // temporary function evaluation.
        internal CallStack callStack {
            get {
                return _callStack;
            }
            set {
                _callStack = value;
            }
        }

        /// <summary>
        /// Get or set the value of a named global ink variable.
        /// The types available are the standard ink types. Certain
        /// types will be implicitly casted when setting.
        /// For example, doubles to floats, longs to ints, and bools
        /// to ints.
        /// </summary>
        public object this[string variableName]
        {
            get {
                Runtime.Object varContents;

                // Search main dictionary first.
                // If it's not found, it might be because the story content has changed,
                // and the original default value hasn't be instantiated.
                // Should really warn somehow, but it's difficult to see how...!
                if ( _globalVariables.TryGetValue (variableName, out varContents) || 
                     _defaultGlobalVariables.TryGetValue(variableName, out varContents) )
                    return (varContents as Runtime.Value).valueObject;
                else {
                    return null;
                }
            }
            set {
                if (!_defaultGlobalVariables.ContainsKey (variableName))
                    throw new StoryException ("Cannot assign to a variable ("+variableName+") that hasn't been declared in the story");
                
                var val = Runtime.Value.Create(value);
                if (val == null) {
                    if (value == null) {
                        throw new StoryException ("Cannot pass null to VariableState");
                    } else {
                        throw new StoryException ("Invalid value passed to VariableState: "+value.ToString());
                    }
                }

                SetGlobal (variableName, val);
            }
        }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

        /// <summary>
        /// Enumerator to allow iteration over all global variables by name.
        /// </summary>
		public IEnumerator<string> GetEnumerator()
		{
			return _globalVariables.Keys.GetEnumerator();
		}

        internal VariablesState (CallStack callStack, ListDefinitionsOrigin listDefsOrigin)
        {
            _globalVariables = new Dictionary<string, Object> ();
            _callStack = callStack;
            _listDefsOrigin = listDefsOrigin;
        }

        internal void CopyFrom (VariablesState toCopy)
        {
            _globalVariables = new Dictionary<string, Object> (toCopy._globalVariables);

            // It's read-only, so no need to create a new copy
            _defaultGlobalVariables = toCopy._defaultGlobalVariables;

            variableChangedEvent = toCopy.variableChangedEvent;

            if (toCopy.batchObservingVariableChanges != batchObservingVariableChanges) {

                if (toCopy.batchObservingVariableChanges) {
                    _batchObservingVariableChanges = true;
                    _changedVariables = new HashSet<string> (toCopy._changedVariables);
                } else {
                    _batchObservingVariableChanges = false;
                    _changedVariables = null;
                }
            }
        }
            
        internal Dictionary<string, object> jsonToken
        {
            get {
                return Json.DictionaryRuntimeObjsToJObject(_globalVariables);
            }
            set {
                _globalVariables = Json.JObjectToDictionaryRuntimeObjs (value);
            }
        }

        internal Runtime.Object GetVariableWithName(string name)
        {
            return GetVariableWithName (name, -1);
        }

        internal Runtime.Object TryGetDefaultVariableValue (string name)
        {
            Runtime.Object val = null;
            _defaultGlobalVariables.TryGetValue (name, out val);
            return val;
        }

		internal bool GlobalVariableExistsWithName(string name)
		{
			return _globalVariables.ContainsKey(name);
		}

        Runtime.Object GetVariableWithName(string name, int contextIndex)
        {
            Runtime.Object varValue = GetRawVariableWithName (name, contextIndex);

            // Get value from pointer?
            var varPointer = varValue as VariablePointerValue;
            if (varPointer) {
                varValue = ValueAtVariablePointer (varPointer);
            }

            return varValue;
        }

        Runtime.Object GetRawVariableWithName(string name, int contextIndex)
        {
            Runtime.Object varValue = null;

            // 0 context = global
            if (contextIndex == 0 || contextIndex == -1) {
                if ( _globalVariables.TryGetValue (name, out varValue) )
                    return varValue;

                var listItemValue = _listDefsOrigin.FindSingleItemListWithName (name);
                if (listItemValue)
                    return listItemValue;
            } 

            // Temporary
            varValue = _callStack.GetTemporaryVariableWithName (name, contextIndex);

            return varValue;
        }

        internal Runtime.Object ValueAtVariablePointer(VariablePointerValue pointer)
        {
            return GetVariableWithName (pointer.variableName, pointer.contextIndex);
        }

        internal void Assign(VariableAssignment varAss, Runtime.Object value)
        {
            var name = varAss.variableName;
            int contextIndex = -1;

            // Are we assigning to a global variable?
            bool setGlobal = false;
            if (varAss.isNewDeclaration) {
                setGlobal = varAss.isGlobal;
            } else {
                setGlobal = _globalVariables.ContainsKey (name);
            }

            // Constructing new variable pointer reference
            if (varAss.isNewDeclaration) {
                var varPointer = value as VariablePointerValue;
                if (varPointer) {
                    var fullyResolvedVariablePointer = ResolveVariablePointer (varPointer);
                    value = fullyResolvedVariablePointer;
                }

            } 

            // Assign to existing variable pointer?
            // Then assign to the variable that the pointer is pointing to by name.
            else {

                // De-reference variable reference to point to
                VariablePointerValue existingPointer = null;
                do {
                    existingPointer = GetRawVariableWithName (name, contextIndex) as VariablePointerValue;
                    if (existingPointer) {
                        name = existingPointer.variableName;
                        contextIndex = existingPointer.contextIndex;
                        setGlobal = (contextIndex == 0);
                    }
                } while(existingPointer);
            }


            if (setGlobal) {
                SetGlobal (name, value);
            } else {
                _callStack.SetTemporaryVariable (name, value, varAss.isNewDeclaration, contextIndex);
            }
        }

        internal void SnapshotDefaultGlobals ()
        {
            _defaultGlobalVariables = new Dictionary<string, Object> (_globalVariables);
        }

        void RetainListOriginsForAssignment (Runtime.Object oldValue, Runtime.Object newValue)
        {
            var oldList = oldValue as ListValue;
            var newList = newValue as ListValue;
            if (oldList && newList && newList.value.Count == 0)
                newList.value.SetInitialOriginNames (oldList.value.originNames);
        }

        internal void SetGlobal(string variableName, Runtime.Object value)
        {
            Runtime.Object oldValue = null;
            _globalVariables.TryGetValue (variableName, out oldValue);

            ListValue.RetainListOriginsForAssignment (oldValue, value);

            _globalVariables [variableName] = value;

            if (variableChangedEvent != null && !value.Equals (oldValue)) {

                if (batchObservingVariableChanges) {
                    _changedVariables.Add (variableName);
                } else {
                    variableChangedEvent (variableName, value);
                }
            }
        }

        // Given a variable pointer with just the name of the target known, resolve to a variable
        // pointer that more specifically points to the exact instance: whether it's global,
        // or the exact position of a temporary on the callstack.
        VariablePointerValue ResolveVariablePointer(VariablePointerValue varPointer)
        {
            int contextIndex = varPointer.contextIndex;

            if( contextIndex == -1 )
                contextIndex = GetContextIndexOfVariableNamed (varPointer.variableName);

            var valueOfVariablePointedTo = GetRawVariableWithName (varPointer.variableName, contextIndex);

            // Extra layer of indirection:
            // When accessing a pointer to a pointer (e.g. when calling nested or 
            // recursive functions that take a variable references, ensure we don't create
            // a chain of indirection by just returning the final target.
            var doubleRedirectionPointer = valueOfVariablePointedTo as VariablePointerValue;
            if (doubleRedirectionPointer) {
                return doubleRedirectionPointer;
            } 

            // Make copy of the variable pointer so we're not using the value direct from
            // the runtime. Temporary must be local to the current scope.
            else {
                return new VariablePointerValue (varPointer.variableName, contextIndex);
            }
        }

        // 0  if named variable is global
        // 1+ if named variable is a temporary in a particular call stack element
        int GetContextIndexOfVariableNamed(string varName)
        {
            if (_globalVariables.ContainsKey (varName))
                return 0;

            return _callStack.currentElementIndex;
        }

        Dictionary<string, Runtime.Object> _globalVariables;

        Dictionary<string, Runtime.Object> _defaultGlobalVariables;

        // Used for accessing temporary variables
        CallStack _callStack;
        HashSet<string> _changedVariables;
        ListDefinitionsOrigin _listDefsOrigin;
    }
}

