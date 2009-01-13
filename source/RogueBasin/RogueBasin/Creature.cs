using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for Creatures. Types will be inherited off this.
    /// </summary>
    public class Creature
    {
        
        /// <summary>
        /// Level the creature is on
        /// </summary>
        int locationLevel;

        /// <summary>
        /// Point on the map on this level that the creature is on
        /// </summary>
        Point locationMap;

        char representation;

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

    }
}
