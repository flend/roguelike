using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class SightRadiusDown : PlayerEffectSimpleDuration
    {
        int duration  { get; set; }

        int sightDownAmount  { get; set; }

        int effectiveSightDownAmount  { get; set; }
        public bool sightZeroCase { get; set; }

        public SightRadiusDown() { }

        public SightRadiusDown(int duration, int sightUpAmount)
        {
            this.sightZeroCase = false;
            this.duration = duration;
            this.sightDownAmount = sightUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Creature target)
        {
            Player player = target as Player;

            LogFile.Log.LogEntry("SightDown start");

            //Bit of a hack, but for 0 sight dungeons, restrict to 1 square. This is what the blinding effects normally do
            if (player.SightRadius == 0)
            {
                sightZeroCase = true;
                player.SightRadius = 1;
            }
            else
            {
                if (sightDownAmount > player.SightRadius)
                {
                    effectiveSightDownAmount = player.SightRadius;
                }
                else
                {
                    effectiveSightDownAmount = sightDownAmount;
                }
            }
            player.SightRadius -= effectiveSightDownAmount;
            Game.MessageQueue.AddMessage("The shadows come closer.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Creature target)
        {
            Player player = target as Player;

            LogFile.Log.LogEntry("SightDown ended");

            if (sightZeroCase)
            {
                player.SightRadius = 0;
            }
            else
            {
                player.SightRadius += effectiveSightDownAmount;
            }
            Game.MessageQueue.AddMessage("Everything is clear again.");
        }

        protected override int GetDuration()
        {
            return duration;
        }
    }
}