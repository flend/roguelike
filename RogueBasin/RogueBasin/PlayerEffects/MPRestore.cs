namespace RogueBasin.PlayerEffects
{
    public class MPRestore : PlayerEffectInstant
    {
        //Strictly doesn't need to be serialized, but in just in case
        public int healingQuantity  { get; set; }

        public MPRestore() { }

        public MPRestore(int healingQuantity)
        {
            this.healingQuantity = healingQuantity;
        }

        public override void OnStart(Player player)
        {

            Game.MessageQueue.AddMessage("You feel your magical energies return!");
            LogFile.Log.LogEntry("MPUp " + healingQuantity.ToString());

            player.MagicPoints += healingQuantity;

            if (player.MagicPoints > player.MaxMagicPoints)
                player.MagicPoints = player.MaxMagicPoints;
        }
    }
}
