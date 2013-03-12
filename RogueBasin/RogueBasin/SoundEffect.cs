using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
