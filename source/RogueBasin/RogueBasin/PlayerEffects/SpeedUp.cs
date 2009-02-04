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
            player.Speed += speedUpAmount;
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd()
        {
            player.Speed -= speedUpAmount;
        }

        protected override int GetDuration()
        {
            return duration;
        }

    }
}
