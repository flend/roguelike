using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    class SightRadiusDown : PlayerEffectSimpleDuration
    {
        int duration;

        int sightDownAmount;

        int effectiveSightDownAmount;
        bool sightZeroCase = false;

        public SightRadiusDown(Player player, int duration, int sightUpAmount)
            : base(player)
        {

            this.duration = duration;
            this.sightDownAmount = sightUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart()
        {
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
        public override void OnEnd()
        {
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