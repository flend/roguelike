using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class DamageUp  : PlayerEffectSimpleDuration
    {
        int duration;

        int damageUpAmount;

        public DamageUp(Player player, int duration, int damageUpAmount)
            : base(player)
        {

            this.duration = duration;
            this.damageUpAmount = damageUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart()
        {
            LogFile.Log.LogEntry("ToHitUp start");
            Game.MessageQueue.AddMessage("Your hands ache with pent-up power");
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

        public override int DamageModifier() { return damageUpAmount; }
    }
}