using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class Healing : PlayerEffectInstant
    {
        public int healingQuantity { get; set; }

        public Healing() { }

        public Healing(int healingQuantity)
        {
            this.healingQuantity = healingQuantity;
        }

        public override void OnStart(Player player)
        {
            //Game.MessageQueue.AddMessage("You feel better!");
            LogFile.Log.LogEntry("Healed " + healingQuantity.ToString());

            player.Hitpoints += healingQuantity;

            if (player.Hitpoints > player.MaxHitpoints)
                player.Hitpoints = player.MaxHitpoints;
        }
    }
}
