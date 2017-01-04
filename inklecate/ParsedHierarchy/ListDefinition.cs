using System;
using System.Collections.Generic;
using System.Linq;

namespace Ink.Parsed
{
    internal class ListDefinition : Parsed.Object
    {
        public string name;
        public List<ListElementDefinition> elements;

        public Runtime.ListDefinition runtimeListDefinition {
            get {
                var allItems = new Dictionary<string, int> ();
                foreach (var e in elements)
                    allItems.Add (e.name, e.seriesValue);
                return new Runtime.ListDefinition (name, allItems);
            }
        }

        public ListElementDefinition ItemNamed (string itemName)
        {
            if (_elementsByName == null) {
                _elementsByName = new Dictionary<string, ListElementDefinition> ();
                foreach (var el in elements) {
                    _elementsByName [el.name] = el;
                }
            }

            ListElementDefinition foundElement;
            if (_elementsByName.TryGetValue (itemName, out foundElement))
                return foundElement;

            return null;
        }

        public ListDefinition (List<ListElementDefinition> elements)
        {
            this.elements = elements;

            int currentValue = 1;
            foreach (var e in this.elements) {
                if (e.explicitValue != null)
                    currentValue = e.explicitValue.Value;

                e.seriesValue = currentValue;

                currentValue++;
            }

            AddContent (elements);
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var initialValues = new Dictionary<string, int> ();
            foreach (var e in elements) {
                if (e.inInitialList)
                    initialValues [this.name + "." + e.name] = e.seriesValue;
            }

            return new Runtime.ListValue (initialValues);
        }

        Dictionary<string, ListElementDefinition> _elementsByName;
    }

    internal class ListElementDefinition : Parsed.Object
    {
        public string name;
        public int? explicitValue;
        public int seriesValue;
        public bool inInitialList;

        public string fullName {
            get {
                var parentList = parent as ListDefinition;
                if (parentList == null)
                    throw new System.Exception ("Can't get full name without a parent list");

                return parentList.name + "." + name;
            }
        }

        public ListElementDefinition (string name, bool inInitialList, int? explicitValue = null)
        {
            this.name = name;
            this.inInitialList = inInitialList;
            this.explicitValue = explicitValue;
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            throw new System.NotImplementedException ();
        }
    }
}
