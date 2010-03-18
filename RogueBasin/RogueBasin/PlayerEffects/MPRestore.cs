using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class MPRestore : PlayerEffectInstant
    {
        int healingQuantity;

        public MPRestore(Player player, int healingQuantity)
            : base(player)
        {
            this.healingQuantity = healingQuantity;
        }

        public override void OnStart()
        {
            Game.MessageQueue.AddMessage("You feel your magical energies return!");
            LogFile.Log.LogEntry("MPUp " + healingQuantity.ToString());

            player.MagicPoints += healingQuantity;

            if (player.MagicPoints > player.MaxMagicPoints)
                player.MagicPoints = player.MaxMagicPoints;
        }
    }
}
