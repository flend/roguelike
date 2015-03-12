using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.Features
{
    class Acid : ActiveFeature
    {
        public override bool PlayerInteraction(Player player)
        {
            var damage = (int)Math.Round(Game.Dungeon.Player.MaxHitpoints / 10.0);
            player.ApplyDamageToPlayer(damage);

            LogFile.Log.LogEntryDebug(Description + " does " + damage + " damage to player on interaction", LogDebugLevel.Medium);
            Game.MessageQueue.AddMessage("Bloody hell - acid!");

            return true;
        }

        public override string Description
        {
            get
            {
                return "Acid - nasty!";
            }
        }

        protected override string GetGameSprite()
        {
            return "";
        }
    }
}
