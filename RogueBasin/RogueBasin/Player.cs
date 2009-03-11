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

        /// <summary>
        /// Player level
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// Player armour class. Auto-calculated so not serialized
        /// </summary>
        int armourClass;

        /// <summary>
        /// Player damage base. Auto-calculated so not serialized
        /// </summary>
        int damageBase;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        int damageModifier;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        int hitModifier;

        public Player()
        {
            effects = new List<PlayerEffect>();

            Level = 1;

            //Add default equipment slots
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Body));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));

            //Setup combat parameters
            CalculateCombatStats();
        }

        /// <summary>
        /// Calculate the player's combat stats based on level and equipment
        /// </summary>
        public void CalculateCombatStats()
        {
            //Defaults (not necessary)
            armourClass = 10;
            damageBase = 4;
            damageModifier = 0;
            hitModifier = 0;

            //Check level
            switch (Level)
            {
                case 1:
                    armourClass = 10;
                    damageBase = 4;
                    damageModifier = 0;
                    hitModifier = 0;
                    break;
            }

            //Check equipped items
            
            foreach (Item item in Inventory.Items)
            {
                if (!item.IsEquipped)
                    continue;

                IEquippableItem equipItem = item as IEquippableItem;
                
                //Error if non-equippable item is equipped
                if(equipItem == null) {
                    LogFile.Log.LogEntry("Item " + item.SingleItemDescription + " is non-equippable but is equipped!");
                    //Just skip
                    continue;
                }

                armourClass += equipItem.ArmourClassModifier();
                damageModifier += equipItem.DamageModifier();
                hitModifier += equipItem.HitModifier();

                if (equipItem.DamageBase() > damageBase)
                {
                    damageBase = equipItem.DamageBase();
                }
            }

            //Check effects

            foreach (PlayerEffect effect in effects)
            {
                armourClass += effect.ArmourClassModifier();
                damageModifier += effect.DamageModifier();
                hitModifier += effect.HitModifier();

                if (effect.DamageBase() > damageBase)
                {
                    damageBase = effect.DamageBase();
                }
            }
        }

        /// <summary>
        /// Current HP
        /// </summary>
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

        /// <summary>
        /// Normal maximum hp
        /// </summary>
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
        /// Player can overdrive to 50% of normal hp. This is the max possible with this fact.
        /// </summary>
        public int OverdriveHitpoints { get; set; }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return armourClass;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return damageBase;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return damageModifier;
        }

        public override int HitModifier()
        {
            return hitModifier;
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

        /// <summary>
        /// Work out the damage from an attack with the specified one-time modifiers (could be from a special attack etc.)
        /// Note ACmod positive is a bonus to the monster AC
        /// </summary>
        /// <param name="hitMod"></param>
        /// <param name="damBase"></param>
        /// <param name="damMod"></param>
        /// <param name="ACmod"></param>
        /// <returns></returns>


        int toHitRoll; //just so we can use it in debug

        private int AttackWithModifiers(Monster monster, int hitMod, int damBase, int damMod, int ACmod)
        {
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;
            
            int attackDamageBase;

            if(damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int monsterAC = monster.ArmourClass() + ACmod;
            toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= monsterAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;

                return totalDamage;
            }

            //Miss
            return 0;
        }

         /// <summary>
        /// Normal attack on a monster. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonster(Monster monster)
        {
            return AttackMonsterWithModifiers(monster, 0, 0, 0, 0);
        }

        /// <summary>
        /// Attack a monster with modifiers. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonsterWithModifiers(Monster monster, int hitModifierMod, int damageBaseMod, int damageModifierMod, int enemyACMod)
        {
            //Do we need to recalculate combat stats?
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();

            //Calculate damage from a normal attack
            int damage = AttackWithModifiers(monster, hitModifierMod, damageBaseMod, damageModifierMod, enemyACMod);

            //Do we hit the monster?
            if (damage > 0)
            {
                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                //Fairly evil switch case for special attack types. Sorry, no time to do it well
                SpecialCombatEffectsOnMonster(monster);

                //Is the monster dead, if so kill it?
                if (monster.Hitpoints <= 0)
                {
                    Game.Dungeon.KillMonster(monster);

                    //Debug string
                    string combatResultsMsg = "PvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    Game.MessageQueue.AddMessage(combatResultsMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "PvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                Game.MessageQueue.AddMessage(combatResultsMsg3);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss
            string combatResultsMsg2 = "PvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monster.Hitpoints + " miss";
            Game.MessageQueue.AddMessage(combatResultsMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }

        /// <summary>
        /// List of special combat effects that might happen to a HIT monster
        /// </summary>
        /// <param name="monster"></param>
        private void SpecialCombatEffectsOnMonster(Monster monster)
        {
            //If short sword is equipped, do a slow down effect (EXAMPLE)
            Item shortSword = null;
            foreach (Item item in Inventory.Items)
            {
                if (item as Items.ShortSword != null)
                {
                    shortSword = item as Items.ShortSword;
                    break;
                }
            }

            //If we are using the short sword apply the slow effect
            if (shortSword != null)
            {
                monster.AddEffect(new MonsterEffects.SlowDown(monster, 500, 50));
            }
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
            effects.Add(effect);

            effect.OnStart();

            //Check if it altered our combat stats
            //CalculateCombatStats();
            //Should be done in effect itself or optionally each time we attack
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

            //Update the player's combat stats which may have been affected

            CalculateCombatStats();

            //Update the inventory listing since equipping an item changes its stackability
            //No longer necessary since no equippable items get displayed in inventory
            //Inventory.RefreshInventoryListing();

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + item.SingleItemDescription, LogDebugLevel.Medium);
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
