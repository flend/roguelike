using System;
using System.Collections.Generic;

namespace RogueBasin
{
    /// <summary>
    /// Description of a group of objects, suitable for display to the user and selection
    /// Presumably all these objects will be more or less to the same to be in the same group
    /// </summary>
    public class InventoryListing : IComparable
    {
        /// <summary>
        /// Parent inventory
        /// </summary>
        Inventory inventory;

        int weight;

        List<int> itemIndex;

        /// <summary>
        /// For serialization only (nb: we rebuild inventory after serialization anyway)
        /// </summary>
        public InventoryListing()
        {

        }

        public int CompareTo(object obj)
        {
            InventoryListing otherListing = obj as InventoryListing;

            if(otherListing != null && itemIndex.Count > 0 && otherListing.itemIndex.Count > 0) {

                //Find name of this type of object
                string name = inventory.Items[itemIndex[0]].SingleItemDescription;
                //And for the other group
                string otherName = inventory.Items[otherListing.itemIndex[0]].SingleItemDescription;

                if (otherName == null)
                    return 1;
                if (name == null)
                    return -1;

                return name.CompareTo(otherName);
            }
            else
                throw new ArgumentException("Object is not an InventoryListing");
        }

        public InventoryListing(Inventory myInventory)
        {
            itemIndex = new List<int>();
            inventory = myInventory;
        }

        /// <summary>
        /// Indices of all the items in Inventory.Items that this group corresponds to
        /// </summary>
        public List<int> ItemIndex
        {
            set
            {
                itemIndex = value;
            }
            get
            {
                return itemIndex;
            }
        }

        /// <summary>
        /// Description suitable for user display (e.g. 2 uncursed short swords)
        /// </summary>
        public string Description
        {
            get
            {
                //Build the description from the number of objects
                //and the single or group item description (taken from the Item reference)

                string descString = Number.ToString() + " ";

                string nameString;

                Item item = inventory.Items[itemIndex[0]];

                if (item.UseHiddenName)
                    nameString = Game.Dungeon.GetHiddenName(item);
                else
                    nameString = item.SingleItemDescription;

                if (Number == 1)
                {
                    descString += nameString;
                }
                else
                {
                    descString += nameString;
                }
                return descString;
            }
        }

        /// <summary>
        /// Number of the objects in the group
        /// </summary>
        public int Number
        {
            get
            {
                return itemIndex.Count;
            }
        }

        /// <summary>
        /// Total weight of the objects in the group
        /// </summary>
        public int Weight
        {
            get
            {
                return weight;
            }
            set
            {
                weight = value;
            }
        }
    }
}
