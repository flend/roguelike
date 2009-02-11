using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Description of a group of objects, suitable for display to the user and selection
    /// Presumably all these objects will be more or less to the same to be in the same group
    /// </summary>
    public class InventoryListing
    {
        string description;
        int weight;

        List<int> itemIndex;

        public InventoryListing()
        {
            itemIndex = new List<int>();
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
        /// Description suitable for user display (e.g. uncursed short swords)
        /// </summary>
        public string Description
        {
            set
            {
                description = value;
            }
            get
            {
                return description;
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
