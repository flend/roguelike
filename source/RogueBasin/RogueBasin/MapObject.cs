using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for any object that can be represented on the map by level & position
    /// </summary>
    public class MapObject
    {
        /// <summary>
        /// ASCII character
        /// </summary>
        char representation;

        /// <summary>
        /// Level the object is on
        /// </summary>
        int locationLevel;

        /// <summary>
        /// Point on the map on this level that the object is on
        /// </summary>
        Point locationMap;

        /// <summary>
        /// Level the object is on
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
        /// Point within the level the object is at
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
    }
}
