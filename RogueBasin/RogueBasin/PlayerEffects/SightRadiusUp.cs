using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    class SightRadiusUp : PlayerEffectSimpleDuration
    {
        int duration;

        int sightUpAmount;

        public SightRadiusUp(Player player, int duration, int sightUpAmount)
            : base(player)
        {

            this.duration = duration;
            this.sightUpAmount = sightUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart()
        {
            LogFile.Log.LogEntry("SightUp start");
            player.SightRadius += sightUpAmount;
            Game.MessageQueue.AddMessage("The darkness falls away in a glance.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd()
        {
            LogFile.Log.LogEntry("SightUp ended");
            player.SightRadius -= sightUpAmount;
            Game.MessageQueue.AddMessage("The shadows grow longer.");
        }

        protected override int GetDuration()
        {
            return duration;
        }
    }
}