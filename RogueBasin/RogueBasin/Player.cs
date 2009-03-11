using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class Player : Creature
    {
        /// <summary>
        /// Effects that are active on the player
        /// </summary>
        List<PlayerEffect> effects;

        /// <summary>
        /// Current hitpoints
        /// </summary>
        int hitpoints;

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int maxHitpoints;

        public Player()
        {
            effects = new List<PlayerEffect>();

            //Add default equipment slots
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Body));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        public int Hitpoints
        {
            get
            {
                return hitpoints;
            }
            set
            {
                hitpoints = value;
            }
        }

        public int MaxHitpoints
        {
            get
            {
                return maxHitpoints;
            }
            set
            {
                maxHitpoints = value;
            }
        }

        /// <summary>
        /// Will we have a turn if we IncrementTurnTime()
        /// </summary>
        /// <returns></returns>
        public bool CheckReadyForTurn()
        {
            if(turnClock + speed >= turnClockLimit)
            {
                return true;
            }
            return false;
        }

        public CombatResults AttackMonster(Monster monster)
        {
            //The monster always dies
            Game.Dungeon.KillMonster(monster);

            string msg = monster.Representation + " was killed!";
            Game.MessageQueue.AddMessage(msg);
            LogFile.Log.LogEntry(msg);

            return CombatResults.DefenderDied;

        }

        /// <summary>
        /// Increment time on all player events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            foreach (PlayerEffect effect in effects)
            {
                effect.IncrementTime();

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                }
            }

            //Remove finished effects
            foreach (PlayerEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }
        }

        /// <summary>
        /// Increment time on all player events then use the base class to increment time on the player's turn counter
        /// </summary>
        /// <returns></returns>
        internal override bool IncrementTurnTime()
        {
            IncrementEventTime();

            return base.IncrementTurnTime();
        }

        /// <summary>
        /// Run an effect on the player. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(PlayerEffect effect)
        {
            effect.OnStart();

            effects.Add(effect);
        }

        protected override char GetRepresentation()
        {
            return '@';
        }

        /// <summary>
        /// Equip an item. Item is removed from the main inventory.
        /// Returns true if item was used successfully.
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        public bool EquipItem(InventoryListing selectedGroup)
        {
            //Select the first item in the stack
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToUse = Inventory.Items[itemIndex];

            //Check if this item is equippable
            IEquippableItem equippableItem = itemToUse as IEquippableItem;

            if (equippableItem == null)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, not equippable: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);
                return false;
            }

            //Find all matching slots available on the player

            List<EquipmentSlot> itemPossibleSlots = equippableItem.EquipmentSlots;
            List<EquipmentSlotInfo> matchingEquipSlots = new List<EquipmentSlotInfo>();

            foreach (EquipmentSlot slotType in itemPossibleSlots)
            {
                matchingEquipSlots.AddRange(this.EquipmentSlots.FindAll(x => x.slotType == slotType));
            }

            //No suitable slots
            if (matchingEquipSlots.Count == 0)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, no valid slots: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);

                return false;
            }

            //Look for first empty slot

            EquipmentSlotInfo freeSlot = matchingEquipSlots.Find(x => x.equippedItem == null);

            if (freeSlot == null)
            {
                //Not slots free, unequip first slot
                Item oldItem = matchingEquipSlots[0].equippedItem;
                IEquippableItem oldItemEquippable = oldItem as IEquippableItem;

                //Sanity check
                if (oldItemEquippable == null)
                {
                    LogFile.Log.LogEntry("Currently equipped item is not equippable!: " + oldItem.SingleItemDescription);
                    return false;
                }

                //Run unequip routine
                oldItemEquippable.UnEquip(this);
                oldItem.IsEquipped = false;
                
                //Can't do this right now, since not in inventory items appear on the floor

                //This slot is now free
                freeSlot = matchingEquipSlots[0];
            }

            //We now have a free slot to equip in

            //Put new item in first relevant slot and run equipping routine
            matchingEquipSlots[0].equippedItem = itemToUse;
            equippableItem.Equip(this);
            itemToUse.IsEquipped = true;

            //Update the inventory listing since equipping an item changes its stackability
            Inventory.RefreshInventoryListing();

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + itemToUse.SingleItemDescription, LogDebugLevel.Low);
            Game.MessageQueue.AddMessage(itemToUse.SingleItemDescription + " equipped in " + StringEquivalent.EquipmentSlots[matchingEquipSlots[0].slotType]);

            return true;
        }

        /// <summary>
        /// Predicate for matching equipment slot of EquipmentSlot type
        /// </summary>
        private static bool EquipmentSlotMatchesType(EquipmentSlotInfo equipSlot, EquipmentSlot type)
        {
            return (equipSlot.slotType == type);
        }

        /// <summary>
        /// Use the item group. Function is responsible for deleting the item if used up etc. Return true if item was used successfully and time should be advanced.
        /// </summary>
        /// <param name="selectedGroup"></param>
        internal bool UseItem(InventoryListing selectedGroup)
        {
            //For now, we use the first item in any stack only
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToUse = Inventory.Items[itemIndex];

            //Check if this is a useable item
            IUseableItem useableItem = itemToUse as IUseableItem;

            if (useableItem == null)
            {
                Game.MessageQueue.AddMessage("Cannot use this type of item!");
                LogFile.Log.LogEntry("Tried to use non-useable item: " + itemToUse.SingleItemDescription);
                return false;
            }

            bool usedSuccessfully = useableItem.Use(this);

            if (useableItem.UsedUp)
            {
                //Remove item from inventory and don't drop on floor
                Inventory.RemoveItem(itemToUse);
                Game.Dungeon.RemoveItem(itemToUse);
            }

            return usedSuccessfully;
        }

        /// <summary>
        /// Simpler version of equip item, doesn't care about slots
        /// </summary>
        /// <param name="equipItem"></param>
        internal bool EquipItemNoSlots(IEquippableItem equipItem)
        {
            Item item = equipItem as Item;

            if (item == null)
            {
                //Should never happen
                LogFile.Log.LogEntry("Problem with item equip");
                Game.MessageQueue.AddMessage("You can't equip this item (bug)");
                return false;
            }

            //Add the item to our inventory
            item.IsEquipped = true;
            Inventory.AddItem(item);

            //Let the item do its equip action
            //This is probably the only time it gets to do this and won't be refreshed after a load game
            equipItem.Equip(this);



            //Update the inventory listing since equipping an item changes its stackability
            //No longer necessary since no equippable items get displayed in inventory
            //Inventory.RefreshInventoryListing();

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + item.SingleItemDescription, LogDebugLevel.Low);
            Game.MessageQueue.AddMessage(item.SingleItemDescription + " equipped!");

            return true;
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        /// <param name="itemToPickUp"></param>
        internal void AddItemToInventory(Item itemToPickUp)
        {
            Inventory.AddItem(itemToPickUp);
        }
    }
}
