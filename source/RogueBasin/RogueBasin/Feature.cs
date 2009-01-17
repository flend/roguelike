using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Non-pickupable objects in the dungeon
    /// </summary>
    public abstract class Feature
    {

        /// <summary>
        /// ASCII character
        /// </summary>
        char representation;

        /// <summary>
        /// Level the item is on
        /// </summary>
        int locationLevel;

        /// <summary>
        /// Point on the map on this level that the item is on
        /// </summary>
        Point locationMap;

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

        public Feature()
        {

        }

        /// <summary>
        /// Process a player interacting with this object
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool PlayerInteraction(Player player);
        
    }
}
