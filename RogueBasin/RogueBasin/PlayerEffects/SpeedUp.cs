using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    /// <summary>
    /// Speed up the player for a duration
    /// </summary>
    public class SpeedUp : PlayerEffectSimpleDuration
    {
        int duration;

        int speedUpAmount;

        public SpeedUp(Player player, int duration, int speedUpAmount) : base(player) {

            this.duration = duration;
            this.speedUpAmount = speedUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart()
        {
            LogFile.Log.LogEntry("Speed Up started");
            Game.MessageQueue.AddMessage("You speed up!");

            //player.Speed += speedUpAmount;
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd()
        {
            LogFile.Log.LogEntry("Speed Up ended");
            Game.MessageQueue.AddMessage("You slow down!");

            //player.Speed -= speedUpAmount;
        }

        public override int SpeedModifier()
        {
            return speedUpAmount;
        }

        protected override int GetDuration()
        {
            return duration;
        }

    }
}
