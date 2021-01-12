using System.Collections.Generic;

namespace Ink.Parsed
{
    public class List : Parsed.Expression
    {
        public List<Identifier> itemIdentifierList;

        public List (List<Identifier> itemIdentifierList)
        {
            this.itemIdentifierList = itemIdentifierList;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            var runtimeRawList = new Runtime.InkList ();

            if (itemIdentifierList != null) {
                foreach (var itemIdentifier in itemIdentifierList) {
                    var nameParts = itemIdentifier?.name.Split ('.');

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
                            Error ("Could not find list definition that contains item '" + itemIdentifier + "'");
                        else
                            Error ("Could not find list item " + itemIdentifier);
                    } else {
                        if (listName == null)
                            listName = ((ListDefinition)listItem.parent).identifier?.name;
                        var item = new Runtime.InkListItem (listName, listItem.name);

                        if (runtimeRawList.ContainsKey (item))
                            Warning ("Duplicate of item '"+itemIdentifier+"' in list.");
                        else
                            runtimeRawList [item] = listItem.seriesValue;
                    }
                }
            }

            container.AddContent(new Runtime.ListValue (runtimeRawList));
        }
    }
}
