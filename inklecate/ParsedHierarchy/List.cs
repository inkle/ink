using System.Collections.Generic;

namespace Ink.Parsed
{
    using StrList = System.Collections.Generic.List<string>;

    internal class List : Parsed.Expression
    {
        public StrList itemNameList;

        public List (StrList itemNameList)
        {
            this.itemNameList = itemNameList;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            var runtimeRawList = new Runtime.RawList ();

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

                    var setItem = story.ResolveListItem (setName, setItemName, this);
                    if (setItem == null) {
                        if (setName == null)
                            Error ("Could not find SET definition that contains item '" + itemName + "'");
                        else
                            Error ("Could not find SET item " + itemName);
                    } else {
                        runtimeRawList.Add (setItem.fullName, setItem.seriesValue);
                    }
                }
            }

            container.AddContent(new Runtime.ListValue (runtimeRawList));
        }
    }
}
