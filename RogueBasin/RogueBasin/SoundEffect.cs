using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// A sound in the dungeon. Sounds are evaluated on distance and recency and magnitude to see if they are interesting 
    /// 
    /// SoundTime is in WorldClock ticks
    /// </summary>
    public class SoundEffect
    {
        //Public for serialization

        public long SoundTime { get; set; }
        public double SoundMagnitude { get; set; }
        public Point MapLocation { get; set; }
        public int LevelLocation { get; set; }

        /// <summary>
        /// For 1.0 magnitude sounds
        /// </summary>
        public const double soundMaxRadius = 15.0;

        public SoundEffect(Dungeon eventReceiver, long soundTime, double soundMagnitude, int soundLevel, Point soundLocation)
        {
            SoundTime = soundTime;
            SoundMagnitude = soundMagnitude;
            MapLocation = soundLocation;
            LevelLocation = soundLevel;
        }

        /// <summary>
        /// Decayed magnitude of sound at WorldClock time passed in
        /// </summary>
        /// <param name="timeNow"></param>
        /// <returns></returns>
        public double DecayedMagnitude(long timeNow)
        {
            return Math.Max(0, (1000 - (timeNow - this.SoundTime)) / 1000.0);
        }

        /// <summary>
        /// Standard function to return the maximum distance where a sound may be heard.
        /// This is useful for drawing purposes
        /// </summary>
        /// <returns></returns>
        public double SoundRadius()
        {
            return soundMaxRadius * SoundMagnitude;
        }

        /// <summary>
        /// Calculate the magnitude of the sound at a particular time, at a particular location
        /// This applies damping for intervening walls
        /// </summary>
        /// <param name="timeNow"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public double DecayedMagnitude(long timeNow, int levelLocation, Point mapLocation)
        {
            double decayedMagnitude = DecayedMagnitude(timeNow);

            //Can't hear sounds across levels
            if (this.LevelLocation != levelLocation)
            {
                return 0.0;
            }

            //Apply damping for distance and no. of walls crossed

            double distance = Math.Sqrt(Math.Pow(mapLocation.x - this.MapLocation.x, 2) + Math.Pow(mapLocation.y - this.MapLocation.y, 2));

            //Try a cliff-cutoff
            if(distance > SoundRadius())
                return 0.0;

            //Draw a direct line between the sound source and the test location

            int xSource = this.MapLocation.x;
            int ySource = this.MapLocation.y;

            TCODLineDrawing.InitLine(xSource, ySource, mapLocation.x, mapLocation.y);

            int noWallsCrossed = 0;

            bool endPoint = false;
            do {
                endPoint = TCODLineDrawing.StepLine(ref xSource, ref ySource);

                //Check if this square is in wall (for now, is non-walkable
                if (!Game.Dungeon.MapSquareIsWalkable(levelLocation, new Point(xSource, ySource)))
                {
                    noWallsCrossed++;
                }
            }
            while(!endPoint);

            //Attenuate by 25% for each wall crossing
            decayedMagnitude -= 0.25 * decayedMagnitude * noWallsCrossed;

            return Math.Max(0.0, decayedMagnitude);
        }

    }
}
