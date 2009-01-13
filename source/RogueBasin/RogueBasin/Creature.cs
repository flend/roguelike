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
        /// Increment each game turn for the creature's internal clock. Turn at 1000
        /// </summary>
        int speed;

        /// <summary>
        /// Speed added each game turn. When 1000 the creature takes a turn
        /// </summary>
        int turnClock = 0;

        /// <summary>
        /// How much the turn clock has to reach to process
        /// </summary>
        const int turnClockLimit = 1000;
        


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
        /// </summary>
        internal void IncrementTurnTime()
        {
            turnClock += speed;

            if (turnClock >= turnClockLimit)
            {
                turnClock -= turnClockLimit;

                ProcessTurn();
            }
        }

        private void ProcessTurn()
        {
            //Right now we don't do anything
        }
    }
}
