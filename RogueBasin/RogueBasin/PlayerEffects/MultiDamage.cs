using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    /// <summary>
    /// Hmmm, there's no guarantee that this is called as the last effect... oh well, not a lot I can do about that other than hack in an exception
    /// </summary>
    public class MultiDamage : PlayerEffectSimpleDuration
    {
        int duration;

        int multiplier;

        public MultiDamage(Player player, int duration, int multipler)
            : base(player)
        {
            this.duration = duration;
            this.multiplier = multipler;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnStart()
        {
            LogFile.Log.LogEntry("MultiDamage started");
            Game.MessageQueue.AddMessage("Enemies should fear you today!");

            Game.Dungeon.Player.RecalculateCombatStatsRequired = true;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnEnd()
        {
            LogFile.Log.LogEntry("MultiDamage ended");
            Game.MessageQueue.AddMessage("Phew!");

            Game.Dungeon.Player.RecalculateCombatStatsRequired = true;
        }

        protected override int GetDuration()
        {
            return duration;
        }

        public override int DamageModifier()
        {
            //We multiply the player's current
            int playerDamMod = Game.Dungeon.Player.DamageModifier();

            return playerDamMod * (multiplier - 1);
        }

        public override int DamageBase()
        {
            //We multiply the player's current
            int playerDamMod = Game.Dungeon.Player.DamageBase();

            return playerDamMod * (multiplier);
        }

    }
}
