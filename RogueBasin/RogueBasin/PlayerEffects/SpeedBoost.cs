using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.PlayerEffects
{
    public class SpeedBoost : PlayerEffectNoDuration
    {
        public int duration { get; set; }

        //public bool sightZeroCase  { get; set; }
        int level;

        public SpeedBoost(int level)
        {
            this.level = level;
        }

        public int Level { get { return level; } }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntry("Boost start");

            //Sight radius is already maximum so don't do anything
            /*if (player.SightRadius == 0)
            {
                sightZeroCase = true;
            }
            else
            {
                player.SightRadius += sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("The world becomes a blur.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player player)
        {
            LogFile.Log.LogEntry("Boost end");
            /*
            if (!sightZeroCase)
            {
                player.SightRadius -= sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("The world slow back down.");
        }

        public override int SightModifier()
        {
            return 0;
        }

        public override int SpeedModifier()
        {
            if (level == 1)
                return +100;

            if (level == 2)
                return +200;

            if (level == 3)
                return +300;

            return +100;
        }

        public override string GetName()
        {
            return "Speed";
        }

        internal override libtcodWrapper.Color GetColor()
        {
            return ColorPresets.BlanchedAlmond;
        }
    }
}