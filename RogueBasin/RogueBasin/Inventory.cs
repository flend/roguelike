using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{


    /// <summary>
    /// An object store. Can be on a container, creature or player
    /// </summary>
    public class Inventory
    {
        List<Item> items;

        List<InventoryListing> inventoryListing;

        List<InventoryListing> equipmentListing;

        int totalWeight = 0;

        public Inventory()
        {
            items = new List<Item>();

            inventoryListing = new List<InventoryListing>();
            equipmentListing = new List<InventoryListing>();
        }

        /// <summary>
        /// Add an item to the inventory. The item will be marked as 'in inventory' so not displayed on the world map.
        /// Now removes from the dungeon master item list (to stop replication when serializing)
        /// </summary>
        /// <param name="itemToAdd"></param>
        public void AddItem(Item itemToAdd) {
            
            itemToAdd.InInventory = true;

            //Add to inventory
            items.Add(itemToAdd);

            totalWeight += itemToAdd.GetWeight();

            //Remove from dungeon list
            Game.Dungeon.RemoveItem(itemToAdd);

            //Refresh the listing
            RefreshInventoryListing();
        }

        /// <summary>
        /// Removes an item from the inventory. Does NOT set InInventory = false. This should be done by the object that possesses the inventory (so it can update the position correctly)
        /// Now adds from the dungeon master item list (to stop replication when serializing). If the caller forgets to set InInventory = false the item will not be displayed
        /// </summary>
        /// <param name="itemToRemove"></param>
        public void RemoveItem(Item itemToRemove)
        {
            //Remove from inventory
            items.Remove(itemToRemove);

            totalWeight -= itemToRemove.GetWeight();

            //Add to dungeon list
            Game.Dungeon.Items.Add(itemToRemove);

            //Refresh the listing
            RefreshInventoryListing();
        }

        /// <summary>
        /// Removes an item from the inventory. Does NOT set InInventory = false. This should be done by the object that possesses the inventory (so it can update the position correctly)
        /// Now adds from the dungeon master item list (to stop replication when serializing). If the caller forgets to set InInventory = false the item will not be displayed
        /// </summary>
        /// <param name="itemToRemove"></param>
        public void RemoveAllItems()
        {
            //Add all items to dungeon list
            foreach (Item item in items)
            {
                Game.Dungeon.Items.Add(item);
            }

            totalWeight = 0;

            //Remove all items from inventory
            items.Clear();

            //Refresh the listing
            RefreshInventoryListing();
        }

        /// <summary>
        /// Do we contain an item of the same type (i.e. class)
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public bool ContainsItem(Item itemName)
        {
            foreach (Item item in items)
            {
                if (Object.ReferenceEquals(itemName.GetType(), item.GetType()))
                {
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// Update the listing groups
        /// </summary>
        public void RefreshInventoryListing()
        {
            //INVENTORY (non-equippable)

            //List of groups of similar items
            inventoryListing.Clear();

            //Group similar items (based on type) into categories (InventoryListing)
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];

                //Equipped items are not displayed
                if (item.IsEquipped)
                {
                    continue;
                }
                
                //Check if we have a similar item group already. If so, add the index of this item to that group
                bool foundGroup = false;

                    foreach (InventoryListing group in inventoryListing)
                    {
                        //Check that we are the same type (and therefore sort of item)
                        Type itemType = item.GetType();

                        //Look only at the first item in the group (stored by index). All the items in this group must have the same type
                        if (items[group.ItemIndex[0]].GetType() == item.GetType() && !items[group.ItemIndex[0]].IsEquipped)
                        {
                            group.ItemIndex.Add(i);
                            foundGroup = true;
                            break;
                        }
                    }


                //If there is no group, create a new one
                if (!foundGroup)
                {
                    InventoryListing newGroup = new InventoryListing(this);
                    newGroup.ItemIndex.Add(i);
                    inventoryListing.Add(newGroup);
                }
            }

            //Sort the inventory listing alphabetically. This keeps the list roughly in the same order for the player

            inventoryListing.Sort();

            //EQUIPPABLE (non-removable)

            equipmentListing.Clear();
                
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];

                //Non-equipped items are not displayed
                if (!item.IsEquipped)
                {
                    continue;
                }

                //No stacking for equipment

                InventoryListing newGroup = new InventoryListing(this);
                newGroup.ItemIndex.Add(i);
                equipmentListing.Add(newGroup);
            }
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
            //For serialization
            set
            {
                items = value;
            }
        }

        /// <summary>
        /// Listing of the inventory, suitable for the user
        /// </summary>
        public List<InventoryListing> InventoryListing
        {
            get
            {
                return inventoryListing;
            }
            set
            {
                inventoryListing = value;
            }
        }

        /// <summary>
        /// Listing of the equipment, suitable for the user
        /// </summary>
        public List<InventoryListing> EquipmentListing
        {
            get
            {
                return equipmentListing;
            }
            set
            {
                equipmentListing = value;
            }
        }


        public int TotalWeight
        {
            get
            {
                return totalWeight;
            }
            //For serialization
            set
            {
                totalWeight = value;
            }
        }
    }
}
