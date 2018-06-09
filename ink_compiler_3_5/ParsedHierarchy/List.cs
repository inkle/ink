using System.Collections.Generic;

namespace Ink.Parsed
{
    using StrList = System.Collections.Generic.List<string>;

    public class List : Parsed.Expression
    {
        public StrList itemNameList;

        public List (StrList itemNameList)
        {
            this.itemNameList = itemNameList;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            var runtimeRawList = new Runtime.InkList ();

            if (itemNameList != null) {
                foreach (var itemName in itemNameList) {
                    var nameParts = itemName.Split ('.');

                    string listName = null;
                    string listItemName = null;
                    if (nameParts.Length > 1) {
                        listName = nameParts [0];
                        listItemName = nameParts [1];
                    } else {
                        listItemName = nameParts [0];
                    }

                    var listItem = story.ResolveListItem (listName, listItemName, this);
                    if (listItem == null) {
                        if (listName == null)
                            Error ("Could not find list definition that contains item '" + itemName + "'");
                        else
                            Error ("Could not find list item " + itemName);
                    } else {
                        if (listName == null)
                            listName = ((ListDefinition)listItem.parent).name;
                        var item = new Runtime.InkListItem (listName, listItem.name);

                        if (runtimeRawList.ContainsKey (item))
                            Warning ("Duplicate of item '"+itemName+"' in list.");
                        else 
                            runtimeRawList [item] = listItem.seriesValue;
                    }
                }
            }

            container.AddContent(new Runtime.ListValue (runtimeRawList));
        }
    }
}
