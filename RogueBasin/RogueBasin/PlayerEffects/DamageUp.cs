using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class DamageUp : PlayerEffectSimpleDuration
    {
        public int duration { get; set; }

        public int damageUpAmount { get; set; }

        public DamageUp() { }

        public DamageUp(int duration, int damageUpAmount)
        {

            this.duration = duration;
            this.damageUpAmount = damageUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Creature target)
        {
            LogFile.Log.LogEntry("ToHitUp start");
            Game.MessageQueue.AddMessage("Your hands ache with pent-up power.");
            Game.Dungeon.Player.CalculateCombatStats();
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Creature target)
        {
            LogFile.Log.LogEntry("ToHitUp ended");
            Game.MessageQueue.AddMessage("Your hands stop aching.");
            Game.Dungeon.Player.RecalculateCombatStatsRequired = true;
        }

        protected override int GetDuration()
        {
            return duration;
        }

        public override int DamageModifier() { return damageUpAmount; }
    }
}