using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    /// <summary>
    /// Reveals the location of items on this level
    /// </summary>
    public class ShowItems : Spell
    {
        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            dungeon.RevealItemsOnLevel(player.LocationLevel);

            //Add a message
            Game.MessageQueue.AddMessage("You sense items on this level.");
            LogFile.Log.LogEntryDebug("ShowItems cast.", LogDebugLevel.Medium);

            return true;
        }

        public override int MPCost()
        {
            return 25;
        }

        public override bool NeedsTarget()
        {
            return false;
        }

        public override string SpellName()
        {
            return "Reveal Items";
        }

        public override string Abbreviation()
        {
            return "RI";
        }

        internal override int GetRequiredMagic()
        {
            return 50;
        }

        internal override string MovieRoot()
        {
            return "spellrevealitems";
        }
    }
}
