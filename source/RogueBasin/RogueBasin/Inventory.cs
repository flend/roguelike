using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// An object store. Can be on a container, creature or player
    /// </summary>
    class Inventory
    {
        List<Item> items;

        int totalWeight = 0;

        public Inventory()
        {
            items = new List<Item>();
        }

        /// <summary>
        /// Add an item to the inventory. The item will be marked as 'in inventory' so not displayed on the world map
        /// </summary>
        /// <param name="itemToAdd"></param>
        public void AddItem(Item itemToAdd) {
            
            itemToAdd.InInventory = true;

            items.Add(itemToAdd);

            totalWeight += itemToAdd.GetWeight();
        }

        /// <summary>
        /// Removes an item from the inventory. Does NOT set InInventory = false. This should be done by the object that possesses the inventory (so it can update the position correctly)
        /// </summary>
        /// <param name="itemToRemove"></param>
        public void RemoveItem(Item itemToRemove)
        {
            items.Remove(itemToRemove);

            totalWeight -= itemToRemove.GetWeight();
        }

        /// <summary>
        /// Items in the inventory
        /// </summary>
        public List<Item> Items
        {
            get
            {
                return items;
            }
        }

        public int TotalWeight
        {
            get
            {
                return totalWeight;
            }
        }
    }
}
