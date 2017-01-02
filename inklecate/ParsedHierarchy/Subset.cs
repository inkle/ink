using System.Collections.Generic;

namespace Ink.Parsed
{
    internal class Subset : Parsed.Expression
    {
        public List<string> itemNameList;

        public Subset (List<string> itemNameList)
        {
            this.itemNameList = itemNameList;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            var runtimeSetDict = new Runtime.SetDictionary ();

            if (itemNameList != null) {
                foreach (var itemName in itemNameList) {
                    var nameParts = itemName.Split ('.');

                    string setName = null;
                    string setItemName = null;
                    if (nameParts.Length > 1) {
                        setName = nameParts [0];
                        setItemName = nameParts [1];
                    } else {
                        setItemName = nameParts [0];
                    }

                    var setItem = story.ResolveSetItem (setName, setItemName, this);
                    if (setItem == null) {
                        if (setName == null)
                            Error ("Could not find SET definition that contains item '" + itemName + "'");
                        else
                            Error ("Could not find SET item " + itemName);
                    } else {
                        runtimeSetDict.Add (setItem.fullName, setItem.seriesValue);
                    }
                }
            }

            container.AddContent(new Runtime.SetValue (runtimeSetDict));
        }

        //public override void ResolveReferences (Story context)
        //{
        //    base.ResolveReferences (context);

        //    throw new System.NotImplementedException ();
        //}
    }
}
