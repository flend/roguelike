using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class ToHitUp : PlayerEffectSimpleDuration
    {
        int duration;

        int toHitUpAmount;

        public ToHitUp(Player player, int duration, int toHitUpAmount)
            : base(player)
        {

            this.duration = duration;
            this.toHitUpAmount = toHitUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart()
        {
            LogFile.Log.LogEntry("ToHitUp start");
            Game.MessageQueue.AddMessage("Your aim feels true");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd()
        {
            LogFile.Log.LogEntry("ToHitUp ended");
            Game.MessageQueue.AddMessage("You return to normal");
        }

        protected override int GetDuration()
        {
            return duration;
        }

        public override int HitModifier() { return toHitUpAmount; }
    }
}
