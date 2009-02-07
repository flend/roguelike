using System;
using System.Collections.Generic;
using System.Text;

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
        /// Is the creature still alive?
        /// </summary>
        bool alive;

        /// <summary>
        /// Sight radius
        /// </summary>
        int sightRadius = 5;

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

            inventory = new Inventory();
        }

        public int SightRadius
        {
            get {
                return sightRadius;
            }
            set {
                sightRadius = value;
            }
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
        /// List of inventory items
        /// </summary>
        /// <returns></returns>
        public List<Item> GetInventoryItems()
        {
            return inventory.Items;
        }
    }
}
