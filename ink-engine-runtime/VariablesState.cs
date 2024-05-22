using System;
using System.Collections.Generic;
using System.Linq;

namespace Ink.Runtime
{
    /// <summary>
    /// Encompasses all the global variables in an ink Story, and
    /// allows binding of a VariableChanged event so that that game
    /// code can be notified whenever the global variables change.
    /// </summary>
	public class VariablesState : IEnumerable<string>
    {
        public delegate void VariableChanged(string variableName, Runtime.Object newValue);
        public event VariableChanged variableChangedEvent;

        public StatePatch patch;

        public void StartVariableObservation()
        {
            _batchObservingVariableChanges = true;
            _changedVariablesForBatchObs = new HashSet<string> ();
        }

        public Dictionary<string, Object> CompleteVariableObservation()
        {
            _batchObservingVariableChanges = false;

            var changedVars = new Dictionary<string, Object> ();
            if (_changedVariablesForBatchObs != null) {
                foreach (var variableName in _changedVariablesForBatchObs) {
                    var currentValue = _globalVariables [variableName];
                    changedVars[variableName] = currentValue;
                }
            }

            // Patch may still be active - e.g. if we were in the middle of a background save
            if( patch != null ) {
                foreach(var variableName in patch.changedVariables) {
                    if( patch.TryGetGlobal(variableName, out Object patchedVal) ) {
                        changedVars[variableName] = patchedVal;
                    }
                }
            }

            _changedVariablesForBatchObs = null;
            return changedVars;
        }

        public void NotifyObservers(Dictionary<string, Object> changedVars)
        {
            foreach (var varToVal in changedVars) {
                variableChangedEvent (varToVal.Key, varToVal.Value);
            }
        }

        // Allow StoryState to change the current callstack, e.g. for
        // temporary function evaluation.
        public CallStack callStack {
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

                if (patch != null && patch.TryGetGlobal(variableName, out varContents))
                    return (varContents as Runtime.Value).valueObject;

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
                        throw new Exception ("Cannot pass null to VariableState");
                    } else {
                        throw new Exception ("Invalid value passed to VariableState: "+value.ToString());
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

        public VariablesState (CallStack callStack, ListDefinitionsOrigin listDefsOrigin)
        {
            _globalVariables = new Dictionary<string, Object> ();
            _callStack = callStack;
            _listDefsOrigin = listDefsOrigin;
        }

        public void ApplyPatch()
        {
            foreach(var namedVar in patch.globals) {
                _globalVariables[namedVar.Key] = namedVar.Value;
            }

            if(_changedVariablesForBatchObs != null ) {
                foreach (var name in patch.changedVariables)
                    _changedVariablesForBatchObs.Add(name);
            }

            patch = null;
        }

        public void SetJsonToken(Dictionary<string, object> jToken)
        {
            _globalVariables.Clear();

            foreach (var varVal in _defaultGlobalVariables) {
                object loadedToken;
                if( jToken.TryGetValue(varVal.Key, out loadedToken) ) {
                    _globalVariables[varVal.Key] = Json.JTokenToRuntimeObject(loadedToken);
                } else {
                    _globalVariables[varVal.Key] = varVal.Value;
                }
            }
        }

        /// <summary>
        /// When saving out JSON state, we can skip saving global values that
        /// remain equal to the initial values that were declared in ink.
        /// This makes the save file (potentially) much smaller assuming that
        /// at least a portion of the globals haven't changed. However, it
        /// can also take marginally longer to save in the case that the 
        /// majority HAVE changed, since it has to compare all globals.
        /// It may also be useful to turn this off for testing worst case
        /// save timing.
        /// </summary>
        public static bool dontSaveDefaultValues = true;

        public void WriteJson(SimpleJson.Writer writer)
        {
            writer.WriteObjectStart();
            foreach (var keyVal in _globalVariables)
            {
                var name = keyVal.Key;
                var val = keyVal.Value;

                if(dontSaveDefaultValues) {
                    // Don't write out values that are the same as the default global values
                    Runtime.Object defaultVal;
                    if (_defaultGlobalVariables != null && _defaultGlobalVariables.TryGetValue(name, out defaultVal))
                    {
                        if (RuntimeObjectsEqual(val, defaultVal))
                            continue;
                    }
                }


                writer.WritePropertyStart(name);
                Json.WriteRuntimeObject(writer, val);
                writer.WritePropertyEnd();
            }
            writer.WriteObjectEnd();
        }

        public bool RuntimeObjectsEqual(Runtime.Object obj1, Runtime.Object obj2)
        {
            if (obj1.GetType() != obj2.GetType()) return false;

            // Perform equality on int/float/bool manually to avoid boxing
            var boolVal = obj1 as BoolValue;
            if( boolVal != null ) {
                return boolVal.value == ((BoolValue)obj2).value;
            }

            var intVal = obj1 as IntValue;
            if( intVal != null ) {
                return intVal.value == ((IntValue)obj2).value;
            }

            var floatVal = obj1 as FloatValue;
            if (floatVal != null)
            {
                return floatVal.value == ((FloatValue)obj2).value;
            }

            // Other Value type (using proper Equals: list, string, divert path)
            var val1 = obj1 as Value;
            var val2 = obj2 as Value;
            if( val1 != null ) {
                return val1.valueObject.Equals(val2.valueObject);
            }

            throw new System.Exception("FastRoughDefinitelyEquals: Unsupported runtime object type: "+obj1.GetType());
        }

        public Runtime.Object GetVariableWithName(string name)
        {
            return GetVariableWithName (name, -1);
        }

        public Runtime.Object TryGetDefaultVariableValue (string name)
        {
            Runtime.Object val = null;
            _defaultGlobalVariables.TryGetValue (name, out val);
            return val;
        }

		public bool GlobalVariableExistsWithName(string name)
		{
			return _globalVariables.ContainsKey(name) || _defaultGlobalVariables != null && _defaultGlobalVariables.ContainsKey(name);
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
                if (patch != null && patch.TryGetGlobal(name, out varValue))
                    return varValue;

                if ( _globalVariables.TryGetValue (name, out varValue) )
                    return varValue;

                // Getting variables can actually happen during globals set up since you can do
                //  VAR x = A_LIST_ITEM
                // So _defaultGlobalVariables may be null.
                // We need to do this check though in case a new global is added, so we need to
                // revert to the default globals dictionary since an initial value hasn't yet been set.
                if( _defaultGlobalVariables != null && _defaultGlobalVariables.TryGetValue(name, out varValue) ) {
                    return varValue;
                }

                var listItemValue = _listDefsOrigin.FindSingleItemListWithName (name);
                if (listItemValue)
                    return listItemValue;
            } 

            // Temporary
            varValue = _callStack.GetTemporaryVariableWithName (name, contextIndex);

            return varValue;
        }

        public Runtime.Object ValueAtVariablePointer(VariablePointerValue pointer)
        {
            return GetVariableWithName (pointer.variableName, pointer.contextIndex);
        }

        public void Assign(VariableAssignment varAss, Runtime.Object value)
        {
            var name = varAss.variableName;
            int contextIndex = -1;

            // Are we assigning to a global variable?
            bool setGlobal = false;
            if (varAss.isNewDeclaration) {
                setGlobal = varAss.isGlobal;
            } else {
                setGlobal = GlobalVariableExistsWithName (name);
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

        public void SnapshotDefaultGlobals ()
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

        public void SetGlobal(string variableName, Runtime.Object value)
        {
            Runtime.Object oldValue = null;
            if( patch == null || !patch.TryGetGlobal(variableName, out oldValue) )
                _globalVariables.TryGetValue (variableName, out oldValue);

            ListValue.RetainListOriginsForAssignment (oldValue, value);

            if (patch != null)
                patch.SetGlobal(variableName, value);
            else
                _globalVariables [variableName] = value;

            if (variableChangedEvent != null && !value.Equals (oldValue)) {

                if (_batchObservingVariableChanges) {
                    if (patch != null)
                        patch.AddChangedVariable(variableName);
                    else if(_changedVariablesForBatchObs != null)
                        _changedVariablesForBatchObs.Add (variableName);
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
            if (GlobalVariableExistsWithName(varName))
                return 0;

            return _callStack.currentElementIndex;
        }

        Dictionary<string, Runtime.Object> _globalVariables;

        Dictionary<string, Runtime.Object> _defaultGlobalVariables;

        // Used for accessing temporary variables
        CallStack _callStack;
        HashSet<string> _changedVariablesForBatchObs;
        ListDefinitionsOrigin _listDefsOrigin;
        bool _batchObservingVariableChanges;
    }
}

