using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class Healing : PlayerEffectInstant
    {
        int healingQuantity;

        public Healing(Player player, int healingQuantity) : base(player)
        {
            this.healingQuantity = healingQuantity;
        }

        public override void OnStart()
        {
            Game.MessageQueue.AddMessage("You feel better!");
            LogFile.Log.LogEntry("Healed " + healingQuantity.ToString());

            player.Hitpoints += healingQuantity;

            if (player.Hitpoints > player.MaxHitpoints)
                player.Hitpoints = player.MaxHitpoints;
        }
    }
}
