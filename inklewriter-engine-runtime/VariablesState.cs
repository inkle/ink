using System.Collections.Generic;

namespace Inklewriter.Runtime
{
    internal class VariablesState
    {
        internal VariablesState (CallStack callStack)
        {
            _globalVariables = new Dictionary<string, Object> ();
            _callStack = callStack;
        }

        internal Runtime.Object GetVariableWithName(string name)
        {
            Runtime.Object varValue = GetRawVariableWithName (name);

            // Get value from pointer?
            var varPointer = varValue as LiteralVariablePointer;
            if (varPointer) {
                varValue = GetVariableWithName (varPointer.value);
            }

            return varValue;
        }

        Runtime.Object GetRawVariableWithName(string name)
        {
            Runtime.Object value = null;
            if (_globalVariables.TryGetValue (name, out value)) {
                return value;
            } else {
                return _callStack.GetTemporaryVariableWithName (name);
            }
        }

        internal void Assign(VariableAssignment varAss, Runtime.Object value)
        {
            var name = varAss.variableName;

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

                    var valueOfVariablePointedTo = GetRawVariableWithName (varPointer.variableName);

                    // Extra layer of indirection:
                    // When accessing a pointer to a pointer (e.g. when calling nested or 
                    // recursive functions that take a variable references, ensure we don't create
                    // a chain of indirection by just returning the final target.
                    var doubleRedirectionPointer = valueOfVariablePointedTo as LiteralVariablePointer;
                    if (doubleRedirectionPointer) {
                        varPointer = doubleRedirectionPointer;
                    } 

                    // Make copy of the variable pointer so we're not using the value direct from
                    // the runtime.
                    else {
                        varPointer = new LiteralVariablePointer (varPointer.variableName);
                    }

                    value = varPointer;
                }

            } 

            // Assign to existing variable pointer?
            // Then assign to the variable that the pointer is pointing to by name.
            else {

                // De-reference variable reference to point to
                LiteralVariablePointer existingPointer = null;
                do {
                    existingPointer = GetRawVariableWithName (name) as LiteralVariablePointer;
                    if (existingPointer) {
                        name = existingPointer.variableName;
                        setGlobal = true;
                    }
                } while(existingPointer);
            }


            if (setGlobal) {
                _globalVariables [name] = value;
            } else {
                _callStack.SetTemporaryVariable (name, value, varAss.isNewDeclaration);
            }
        }

        Dictionary<string, Runtime.Object> _globalVariables;

        // Used for accessing temporary variables
        CallStack _callStack;
    }
}

