namespace RogueBasin.PlayerEffects
{
    /// <summary>
    /// Hmmm, there's no guarantee that this is called as the last effect... oh well, not a lot I can do about that other than hack in an exception
    /// </summary>
    public class MultiDamage : PlayerEffectSimpleDuration
    {
        public int duration  { get; set; }

        public int multiplier  { get; set; }

        public MultiDamage() { }

        public MultiDamage(int duration, int multipler)
        {
            this.duration = duration;
            this.multiplier = multipler;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntry("MultiDamage started");
            Game.MessageQueue.AddMessage("Enemies should fear you today!");

            player.RecalculateCombatStatsRequired = true;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnEnd(Player player)
        {
            LogFile.Log.LogEntry("MultiDamage ended");
            Game.MessageQueue.AddMessage("Phew!");

            player.RecalculateCombatStatsRequired = true;
        }

        public override int GetDuration()
        {
            return duration;
        }

        public override double DamageModifier()
        {
            //We multiply the player's current
            var playerDamMod = Game.Dungeon.Player.DamageModifier();

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
