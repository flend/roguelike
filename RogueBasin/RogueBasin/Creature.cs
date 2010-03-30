using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RogueBasin
{
    /// <summary>
    /// Base class for Creatures.
    /// </summary>
    public abstract class Creature : MapObject
    {
        /// <summary>
        /// The creature's inventory
        /// </summary>
        Inventory inventory;

        /// <summary>
        /// The equipment slots the creature has
        /// </summary>
        List<EquipmentSlotInfo> equipmentSlots;

        /// <summary>
        /// Is the creature still alive?
        /// </summary>
        bool alive;

        public int UniqueID { get; set; }

        public uint TurnCount { get; set; }

        /// <summary>
        /// The creature we were last attacked by
        /// </summary>
        [XmlIgnore]
        public Creature LastAttackedBy { get; set; }

        public int LastAttackedByID { get; set; }


        /// <summary>
        /// Sight radius
        /// </summary>
        public int SightRadius {get; set;}

        public int NormalSightRadius { get; set; }

        /// <summary>
        /// Increment each game turn for the creature's internal clock. Turn at turnClockLimit
        /// </summary>
        protected int speed = 100;

        /// <summary>
        /// Current turn clock value for the creature. When 1000 the creature takes a turn
        /// </summary>
        protected int turnClock = 0;

        /// <summary>
        /// How much the turn clock has to reach to process
        /// </summary>
        protected const int turnClockLimit = 10000;

        /// <summary>
        /// A list of all the equipment slots the creature has
        /// </summary>
        public List<EquipmentSlotInfo> EquipmentSlots
        {
            get
            {
                return equipmentSlots;
            }
        }

        /// <summary>
        /// For serialization
        /// </summary>
        public int TurnClock
        {
            get
            {
                return turnClock;
            }
            set
            {
                turnClock = value;
            }
        }

        public int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        /// <summary>
        /// Set to false to kill the creature
        /// </summary>
        public bool Alive
        {
            get
            {
                return alive;
            }
            set
            {
                alive = value;
            }
        }

        public Creature()
        {
            alive = true;

            LastAttackedBy = null;
            LastAttackedByID = -1;

            NormalSightRadius = 5;

            inventory = new Inventory();

            equipmentSlots = new List<EquipmentSlotInfo>();

            RandomStartTurnClock();
        }

                //Creatures start with a random amount in their turn clock. This stops them all moving simultaneously (looks strange if the player is fast)
        private void RandomStartTurnClock()
        {
            turnClock = Game.Random.Next(turnClockLimit);
        }

        /// <summary>
        /// Increment the internal turn timer and resets if over boundary. Return true if a turn should be had.
        /// </summary>
        internal virtual bool IncrementTurnTime()
        {
            turnClock += speed;

            if (turnClock >= turnClockLimit)
            {
                turnClock -= turnClockLimit;

                TurnCount++;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Pick up an item and put it in the inventory if possible
        /// </summary>
        /// <param name="itemToPickUp"></param>
        /// <returns>False if the item won't fit in the inventory for some reason</returns>
        public virtual bool PickUpItem(Item itemToPickUp)
        {
            //Set the item as found
            itemToPickUp.IsFound = true;

            inventory.AddItem(itemToPickUp);

            return true;
        }

        /// <summary>
        /// Drop an item. Sets the item position to the position of this character
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public virtual bool DropItem(Item itemToDrop)
        {
            inventory.RemoveItem(itemToDrop);

            itemToDrop.LocationLevel = this.LocationLevel;
            itemToDrop.LocationMap = this.LocationMap;
            itemToDrop.InInventory = false;

            return true;
        }

        /// <summary>
        /// Drop all inventory (perhaps you died). Currently drops all items on the floor, could extend by looking at surrounding squares. Use the dig algorithm
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public virtual bool DropAllItems()
        {
            foreach (Item item in inventory.Items)
            {
                item.LocationLevel = this.LocationLevel;
                item.LocationMap = this.LocationMap;
                item.InInventory = false;
            }

            inventory.RemoveAllItems();

            return true;
        }


        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public abstract int ArmourClass();

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public abstract int HitModifier();

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public abstract int DamageBase();

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public abstract int DamageModifier();

        /// <summary>
        /// An effect with combat stat changes has expired, so they need to be recalculated
        /// </summary>
        public bool RecalculateCombatStatsRequired { get; set; }

        /// <summary>
        /// List of inventory items
        /// </summary>
        /// <returns></returns>
        public List<Item> GetInventoryItems()
        {
            return inventory.Items;
        }

        /// <summary>
        /// Inventory - possibly make this protected at some point?
        /// </summary>
        public Inventory Inventory
        {
            get
            {
                return inventory;
            }
            //For serialization
            set
            {
                inventory = value;
            }
        }
    }
}
