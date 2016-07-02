using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.Features
{
    class Acid : DangerousActiveFeature
    {
        public override bool PlayerInteraction(Player player)
        {
            var damage = (int)Math.Round(Game.Dungeon.Player.MaxHitpoints / 10.0);
            player.ApplyDamageToPlayer(damage);

            LogFile.Log.LogEntryDebug(Description + " does " + damage + " damage to player on interaction", LogDebugLevel.Medium);
            Game.MessageQueue.AddMessage("Bloody hell - acid!");
            SoundPlayer.Instance().EnqueueSound("acid");

            return true;
        }

        public override bool MonsterInteraction(Monster monster)
        {
            var damage = (int)Math.Round(monster.MaxHitpoints / 5.0);
            monster.ApplyDamageToMonster(null, monster, damage);

            LogFile.Log.LogEntryDebug(Description + " does " + damage + " damage to monster on interaction", LogDebugLevel.Medium);

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
            return "acid";
        }
    }
}
