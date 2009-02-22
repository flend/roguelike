using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for all types of pickup-able items
    /// </summary>
    public abstract class Item : MapObject
    {
        public Item()
        {
            inInventory = false;
        }

        /// <summary>
        /// Is this in a creature's inventory
        /// </summary>
        bool inInventory;

        /// <summary>
        /// Is this item in an inventory and therefore should not be rendered on the map?
        /// Policy is that LocationMap and LocationLevel may contain out-of-date data when InInventory is set
        /// </summary>
        public bool InInventory
        {
            get
            {
                return inInventory;
            }

            set
            {
                inInventory = value;
            }
        }

        /// <summary>
        /// Return the weight of the object. Set in derived classes
        /// </summary>
        /// <returns></returns>
        public abstract int GetWeight();

        /// <summary>
        /// Applies the effect of the object
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if the object could be used</returns>
        public abstract bool Use(Creature user);

        /// <summary>
        /// If the object has been used up by a Use()
        /// </summary>
        public abstract bool UsedUp
        {
            get;
        }

        /// <summary>
        /// Single item description, e.g. 'sword'
        /// </summary>
        public abstract string SingleItemDescription
        {
            get;
        }

        /// <summary>
        /// Group item description, e.g. 'swords'
        /// </summary>
        public abstract string GroupItemDescription
        {
            get;
        }
    }
}
