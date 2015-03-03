using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    public class Exit : Spell
    {
        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Are we in the dungeons
            if (player.LocationLevel < 2)
            {
                Game.MessageQueue.AddMessage("Exit only works in the dungeon.");
                LogFile.Log.LogEntryDebug("Exit used when not in dungeon", LogDebugLevel.Medium);
            }

            //Check if we actually want to do this
            /*
            bool actuallyLeave = Screen.Instance.YesNoQuestion("Do you want to leave the dungeon and return to school?");

            //Exit from current dungeon

            if (actuallyLeave)
            {
                Game.MessageQueue.AddMessage("You teleport out of the dungeon and return to school.");
                dungeon.PlayerLeavesDungeon();
            }
            else
            {
                Game.MessageQueue.AddMessage("You cancel the spell and the energies dissipate.");
            }*/

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
            return "Exit";
        }

        public override string Abbreviation()
        {
            return "XT";
        }

        internal override int GetRequiredMagic()
        {
            return 15;
        }

        internal override string MovieRoot()
        {
            return "spellexit";
        }
    }
}
