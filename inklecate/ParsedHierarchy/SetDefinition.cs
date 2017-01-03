using System;
using System.Collections.Generic;
using System.Linq;

namespace Ink.Parsed
{
    internal class SetDefinition : Parsed.Object
    {
        public string name;
        public List<SetElementDefinition> elements;

        public Runtime.Set runtimeSetDefinition {
            get {
                var allItems = new Dictionary<string, int> ();
                foreach (var e in elements)
                    allItems.Add (e.name, e.seriesValue);
                return new Runtime.Set (name, allItems);
            }
        }

        public SetElementDefinition ItemNamed (string itemName)
        {
            if (_elementsByName == null) {
                _elementsByName = new Dictionary<string, SetElementDefinition> ();
                foreach (var el in elements) {
                    _elementsByName [el.name] = el;
                }
            }

            SetElementDefinition foundElement;
            if (_elementsByName.TryGetValue (itemName, out foundElement))
                return foundElement;

            return null;
        }

        public SetDefinition (List<SetElementDefinition> elements)
        {
            this.elements = elements;

            int currentValue = 1;
            bool hasDefinedInitialValue = false;
            foreach (var e in this.elements) {
                if (e.explicitValue != null) {
                    currentValue = e.explicitValue.Value;
                }

                e.seriesValue = currentValue;

                if (e.inInitialSet)
                    hasDefinedInitialValue = true;

                currentValue++;
            }

            // If no particular element is assigned to the initial set,
            // make it the first one.
            if (!hasDefinedInitialValue)
                elements [0].inInitialSet = true;

            AddContent (elements);
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var initialValues = new Dictionary<string, int> ();
            foreach (var e in elements) {
                if (e.inInitialSet)
                    initialValues [this.name + "." + e.name] = e.seriesValue;
            }

            return new Runtime.ListValue (initialValues);
        }

        Dictionary<string, SetElementDefinition> _elementsByName;
    }

    internal class SetElementDefinition : Parsed.Object
    {
        public string name;
        public int? explicitValue;
        public int seriesValue;
        public bool inInitialSet;

        public string fullName {
            get {
                var parentSet = parent as SetDefinition;
                if (parentSet == null)
                    throw new System.Exception ("Can't get full name without a parent set");

                return parentSet.name + "." + name;
            }
        }

        public SetElementDefinition (string name, bool inInitialSet, int? explicitValue = null)
        {
            this.name = name;
            this.inInitialSet = inInitialSet;
            this.explicitValue = explicitValue;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            throw new System.NotImplementedException ();
        }
    }
}
