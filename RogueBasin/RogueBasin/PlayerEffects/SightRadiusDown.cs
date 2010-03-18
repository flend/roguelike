using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    class SightRadiusDown : PlayerEffectSimpleDuration
    {
        int duration;

        int sightDownAmount;

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
            player.SightRadius -= sightDownAmount;
            Game.MessageQueue.AddMessage("The shadows come closer.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd()
        {
            LogFile.Log.LogEntry("SightDown ended");
            player.SightRadius += sightDownAmount;
            Game.MessageQueue.AddMessage("Everything is clear again.");
        }

        protected override int GetDuration()
        {
            return duration;
        }
    }
}