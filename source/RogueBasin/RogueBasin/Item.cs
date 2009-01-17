using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for all types of pickup-able items
    /// </summary>
    public abstract class Item
    {
        public Item() {
            inInventory = false;
        }

        /// <summary>
        /// ASCII character
        /// </summary>
        char representation;

        /// <summary>
        /// Level the creature is on
        /// </summary>
        int locationLevel;

        /// <summary>
        /// Point on the map on this level that the creature is on
        /// </summary>
        Point locationMap;

        /// <summary>
        /// Is this in a creature's inventory
        /// </summary>
        bool inInventory;

        public char Representation
        {
            get
            {
                return representation;
            }
            set
            {
                representation = value;
            }
        }

        /// <summary>
        /// Level the item is on
        /// </summary>
        public int LocationLevel
        {
            get
            {
                return locationLevel;
            }
            set
            {
                locationLevel = value;
            }
        }

        /// <summary>
        /// Point within the level the item is at
        /// </summary>
        public Point LocationMap
        {
            get
            {
                return locationMap;
            }
            set
            {
                locationMap = value;
            }
        }

        public bool InInventory
        {
            get
            {
                return inInventory;
            }
        }
    }
}
