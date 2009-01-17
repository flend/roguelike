using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for all types of pickup-able items
    /// </summary>
    public class Item
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
