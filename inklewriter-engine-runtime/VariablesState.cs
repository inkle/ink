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
            Runtime.Object value = null;
            if (_globalVariables.TryGetValue (name, out value)) {
                return value;
            } else {
                return _callStack.GetTemporaryVariableWithName (name);
            }
        }

        Runtime.Object GetRawVariableWithName(string name)
        {
            Runtime.Object value = null;
            if (_globalVariables.TryGetValue (name, out value)) {
                return value;
            } else {
                return null;
            }
        }

        internal void SetVariable(string name, Runtime.Object value, bool isNewDeclaration)
        {
            // TODO: Do stuff with temporaries?
            //_callStack.SetTemporaryVariable (varAss.variableName, assignedVal, varAss.isNewDeclaration, prioritiseHigherInCallStack);

            if (!isNewDeclaration && !_globalVariables.ContainsKey (name)) {
                throw new StoryException ("Could not find variable to set: " + name);
            }

            if (isNewDeclaration && value is LiteralVariablePointer) {
                var varPointer = value as LiteralVariablePointer;
                if (varPointer) {

                    var varValue = GetRawVariableWithName (varPointer.variableName);
                    if (!varValue) {
                        throw new StoryException ("Could not variable to reference: " + varPointer.variableName);
                    }

                    // Extra layer of indirection:
                    // When accessing a pointer to a pointer (e.g. when calling nested or 
                    // recursive functions that take a variable references, ensure we don't create
                    // a chain of indirection by just returning the final target.
                    if (varValue is LiteralVariablePointer) {
                        value = varValue;
                    } else {
                        
                        // Create new pointer to the value so that we're not attempting to use
                        // a runtime object direct from the story data itself.
                        value = new LiteralVariablePointer (varPointer.variableName);
                    }
                }
            }

            _globalVariables [name] = value;
        }

        Dictionary<string, Runtime.Object> _globalVariables;

        // Used for accessing temporary variables
        CallStack _callStack;
    }
}

