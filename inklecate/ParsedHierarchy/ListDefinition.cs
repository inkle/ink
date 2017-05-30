using System;
using System.Collections.Generic;
using System.Linq;

namespace Ink.Parsed
{
    internal class ListDefinition : Parsed.Object
    {
        public string name;
        public List<ListElementDefinition> itemDefinitions;

        public Runtime.ListDefinition runtimeListDefinition {
            get {
                var allItems = new Dictionary<string, int> ();
                foreach (var e in itemDefinitions)
                    allItems.Add (e.name, e.seriesValue);
                return new Runtime.ListDefinition (name, allItems);
            }
        }

        public ListElementDefinition ItemNamed (string itemName)
        {
            if (_elementsByName == null) {
                _elementsByName = new Dictionary<string, ListElementDefinition> ();
                foreach (var el in itemDefinitions) {
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
            this.itemDefinitions = elements;

            int currentValue = 1;
            foreach (var e in this.itemDefinitions) {
                if (e.explicitValue != null)
                    currentValue = e.explicitValue.Value;

                e.seriesValue = currentValue;

                currentValue++;
            }

            AddContent (elements);
        }

        public override Runtime.Object GenerateRuntimeObject ()
        {
            var initialValues = new Runtime.InkList ();
            foreach (var itemDef in itemDefinitions) {
                if (itemDef.inInitialList) {
                    var item = new Runtime.InkListItem (this.name, itemDef.name);
                    initialValues [item] = itemDef.seriesValue;
                }
            }

            // Set origin name, so 
            initialValues.SetInitialOriginName (name);

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
