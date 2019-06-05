using System.Collections.Generic;

namespace Ink.Runtime
{
    internal class StatePatch
    {
        public Dictionary<string, Runtime.Object> globals { get { return _globals;  } }
        public HashSet<string> changedVariables { get { return _changedVariables;  } }
        public Dictionary<Container, int> visitCounts { get { return _visitCounts;  } }
        public Dictionary<Container, int> turnIndices { get { return _turnIndices;  } }

        public StatePatch(StatePatch toCopy)
        {
            if( toCopy != null ) {
                _globals = new Dictionary<string, Object>(toCopy._globals);
                _changedVariables = new HashSet<string>(toCopy._changedVariables);
                _visitCounts = new Dictionary<Container, int>(toCopy._visitCounts);
                _turnIndices = new Dictionary<Container, int>(toCopy._turnIndices);
            } else {
                _globals = new Dictionary<string, Object>();
                _changedVariables = new HashSet<string>();
                _visitCounts = new Dictionary<Container, int>();
                _turnIndices = new Dictionary<Container, int>();
            }
        }

        public bool TryGetGlobal(string name, out Runtime.Object value)
        {
            return _globals.TryGetValue(name, out value);
        }

        public void SetGlobal(string name, Runtime.Object value){
            _globals[name] = value;
        }

        public void AddChangedVariable(string name)
        {
            _changedVariables.Add(name);
        }

        public bool TryGetVisitCount(Container container, out int count)
        {
            return _visitCounts.TryGetValue(container, out count);
        }

        public void SetVisitCount(Container container, int count)
        {
            _visitCounts[container] = count;
        }

        public void SetTurnIndex(Container container, int index)
        {
            _turnIndices[container] = index;
        }

        public bool TryGetTurnIndex(Container container, out int index)
        {
            return _turnIndices.TryGetValue(container, out index);
        }

        Dictionary<string, Runtime.Object> _globals;
        HashSet<string> _changedVariables = new HashSet<string>();
        Dictionary<Container, int> _visitCounts = new Dictionary<Container, int>();
        Dictionary<Container, int> _turnIndices = new Dictionary<Container, int>();
    }
}
