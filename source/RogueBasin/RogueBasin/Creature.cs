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

        /// <summary>
        /// Increment each game turn for the creature's internal clock. Turn at turnClockLimit
        /// </summary>
        protected int speed = 10;

        /// <summary>
        /// Current turn clock value for the creature. When 1000 the creature takes a turn
        /// </summary>
        protected int turnClock = 0;

        /// <summary>
        /// How much the turn clock has to reach to process
        /// </summary>
        protected const int turnClockLimit = 1000;
        


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
        /// Increment the internal turn timer. If sufficient, carry out a turn. Perhaps also deal with time-based effects?
        /// Returns true if a turn was had
        /// </summary>
        internal virtual bool IncrementTurnTime()
        {
            turnClock += speed;

            if (turnClock >= turnClockLimit)
            {
                turnClock -= turnClockLimit;

                ProcessTurn();

                return true;
            }
            else return false;
        }

        public virtual void ProcessTurn()
        {
            //Right now we don't do anything
        }
    }
}
