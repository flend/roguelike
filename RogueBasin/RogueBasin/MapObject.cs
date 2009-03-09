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
        char representation = '\0';

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

        /// <summary>
        /// Map char. Stored in derived classes but can also be overridden by setting with this
        /// </summary>
        public char Representation
        {
            get
            {
                if (representation == '\0')
                {
                    return GetRepresentation();
                }
                else
                {
                    return representation;
                }
            }
            set
            {
                representation = value;
            }
        }

        /// <summary>
        /// Get the representation from the derived class
        /// </summary>
        /// <returns></returns>
        protected virtual char GetRepresentation()
        {
            return 'X';
        }

        /// <summary>
        /// Return true if this object and other are in the same place (level and square)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool InSameSpace(MapObject other)
        {
            if (this.LocationLevel == other.LocationLevel &&
                this.LocationMap == other.LocationMap)
            {
                return true;
            }
            else 
                return false;
        }
        
        /// <summary>
        /// Return true if the object is at the position specified
        /// </summary>
        /// <param name="level"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        public bool IsLocatedAt(int level, Point locationMap)
        {
            if (this.LocationLevel == level && this.LocationMap == locationMap)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
