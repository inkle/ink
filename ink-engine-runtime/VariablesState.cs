using System.Collections.Generic;

namespace Ink.Runtime
{
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

        public object this[string variableName]
        {
            get {
                Runtime.Object varContents;
                if ( _globalVariables.TryGetValue (variableName, out varContents) )
                    return (varContents as Runtime.Literal).valueObject;
                else
                    return null;
            }
            set {
                var literal = Runtime.Literal.Create(value);
                if (literal == null) {
                    if (value == null) {
                        throw new StoryException ("Cannot pass null to VariableState");
                    } else {
                        throw new StoryException ("Invalid value passed to VariableState: "+value.ToString());
                    }
                }

                SetGlobal (variableName, literal);
            }
        }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _globalVariables.Keys.GetEnumerator();
		}

        internal VariablesState (CallStack callStack)
        {
            _globalVariables = new Dictionary<string, Object> ();
            _callStack = callStack;
        }

        internal void CopyFrom(VariablesState varState)
        {
            _globalVariables = new Dictionary<string, Object> (varState._globalVariables);
            variableChangedEvent = varState.variableChangedEvent;

            if (varState.batchObservingVariableChanges != batchObservingVariableChanges) {

                if (varState.batchObservingVariableChanges) {
                    _batchObservingVariableChanges = true;
                    _changedVariables = new HashSet<string> (varState._changedVariables);
                } else {
                    _batchObservingVariableChanges = false;
                    _changedVariables = null;
                }
            }
        }

        internal Runtime.Object GetVariableWithName(string name)
        {
            return GetVariableWithName (name, -1);
        }

        Runtime.Object GetVariableWithName(string name, int contextIndex)
        {
            Runtime.Object varValue = GetRawVariableWithName (name, contextIndex);

            // Get value from pointer?
            var varPointer = varValue as LiteralVariablePointer;
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
            } 

            // Temporary
            varValue = _callStack.GetTemporaryVariableWithName (name, contextIndex);

            if (varValue == null)
                throw new System.Exception ("Shouldn't ever have a null value where it couldn't be found at all");

            return varValue;
        }

        internal Runtime.Object ValueAtVariablePointer(LiteralVariablePointer pointer)
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
                var varPointer = value as LiteralVariablePointer;
                if (varPointer) {
                    var fullyResolvedVariablePointer = ResolveVariablePointer (varPointer);
                    value = fullyResolvedVariablePointer;
                }

            } 

            // Assign to existing variable pointer?
            // Then assign to the variable that the pointer is pointing to by name.
            else {

                // De-reference variable reference to point to
                LiteralVariablePointer existingPointer = null;
                do {
                    existingPointer = GetRawVariableWithName (name, contextIndex) as LiteralVariablePointer;
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

        void SetGlobal(string variableName, Runtime.Object value)
        {
            Runtime.Object oldValue = null;
            _globalVariables.TryGetValue (variableName, out oldValue);

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
        LiteralVariablePointer ResolveVariablePointer(LiteralVariablePointer varPointer)
        {
            int contextIndex = varPointer.contextIndex;

            if( contextIndex == -1 )
                contextIndex = GetContextIndexOfVariableNamed (varPointer.variableName);

            var valueOfVariablePointedTo = GetRawVariableWithName (varPointer.variableName, contextIndex);

            // Extra layer of indirection:
            // When accessing a pointer to a pointer (e.g. when calling nested or 
            // recursive functions that take a variable references, ensure we don't create
            // a chain of indirection by just returning the final target.
            var doubleRedirectionPointer = valueOfVariablePointedTo as LiteralVariablePointer;
            if (doubleRedirectionPointer) {
                return doubleRedirectionPointer;
            } 

            // Make copy of the variable pointer so we're not using the value direct from
            // the runtime. Temporary must be local to the current scope.
            else {
                return new LiteralVariablePointer (varPointer.variableName, contextIndex);
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

        // Used for accessing temporary variables
        CallStack _callStack;
        HashSet<string> _changedVariables;
    }
}

