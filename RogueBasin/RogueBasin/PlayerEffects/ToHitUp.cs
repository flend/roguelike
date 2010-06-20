using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class ToHitUp : PlayerEffectSimpleDuration
    {
        public int duration  { get; set; }

        public int toHitUpAmount  { get; set; }

        public ToHitUp() { }

        public ToHitUp(int duration, int toHitUpAmount)
        {

            this.duration = duration;
            this.toHitUpAmount = toHitUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player target)
        {
            LogFile.Log.LogEntry("ToHitUp start");
            Game.MessageQueue.AddMessage("Your aim feels true.");
            Game.Dungeon.Player.CalculateCombatStats();
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player target)
        {
            LogFile.Log.LogEntry("ToHitUp ended");
            Game.MessageQueue.AddMessage("You aim returns to normal.");
            Game.Dungeon.Player.RecalculateCombatStatsRequired = true;
        }

        protected override int GetDuration()
        {
            return duration;
        }

        public override int HitModifier() { return toHitUpAmount; }
    }
}
