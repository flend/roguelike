using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    /// <summary>
    /// Speed up the player for a duration
    /// </summary>
    public class SpeedDown : PlayerEffectSimpleDuration
    {
        int duration;

        int speedUpAmount;

        public SpeedDown(Player player, int duration, int speedUpAmount) : base(player) {

            this.duration = duration;
            this.speedUpAmount = speedUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart()
        {
            LogFile.Log.LogEntry("Speed Down started");
            Game.MessageQueue.AddMessage("You slow up!");

            player.Speed -= speedUpAmount;
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd()
        {
            LogFile.Log.LogEntry("Speed Down ended");
            Game.MessageQueue.AddMessage("You speed up!");

            player.Speed += speedUpAmount;
        }

        protected override int GetDuration()
        {
            return duration;
        }

    }
}
