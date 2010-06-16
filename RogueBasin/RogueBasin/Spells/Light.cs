using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    /// <summary>
    /// Teleports to a nearby location
    /// </summary>
    public class Light : Spell
    {
        
        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Do we already have the effect?
            if (player.IsEffectActive(new PlayerEffects.SightRadiusUp(0, 0)))
            {
                Game.MessageQueue.AddMessage("Spell already in effect.");
                LogFile.Log.LogEntryDebug("Light already in effect", LogDebugLevel.Medium);
                return false;
            }

            //Add an armour up effect

            //Apply the armour effect to the player
            //Duration note 100 is normally 1 turn for a non-sped up player
            int duration = 4000 + Game.Random.Next(6000);
            int toLight = (int)Math.Floor(player.MagicStat / 50.0) + 1;

            //Add a message
            Game.MessageQueue.AddMessage("You cast Light.");
            LogFile.Log.LogEntryDebug("Light Cast. Duration: " + duration + " Sight: +" + toLight, LogDebugLevel.Medium);

            player.AddEffect(new PlayerEffects.SightRadiusUp(duration, toLight));
            return true;
        }

        public override int MPCost()
        {
            return 5;
        }

        public override bool NeedsTarget()
        {
            return false;
        }

        public override string SpellName()
        {
            return "Light";
        }

        public override string Abbreviation()
        {
            return "LG";
        }

        internal override int GetRequiredMagic()
        {
            return 30;
        }

        internal override string MovieRoot()
        {
            return "spelllight";
        }
    }
}
