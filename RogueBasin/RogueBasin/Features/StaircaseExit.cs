using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    /// <summary>
    /// Staircase up. Leave up to the wilderness (i.e. go home)
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public class StaircaseExit : UseableFeature
    {

        int dungeonID;

        public StaircaseExit(int dungeonID)
        {
            this.dungeonID = dungeonID;
        }

        public override bool PlayerInteraction(Player player)
        {
            Dungeon dungeon = Game.Dungeon;

            //Check they really do want to leave
            bool decision = Screen.Instance.YesNoQuestion("Really leave the dungeon?");

            if (decision == false)
            {
                LogFile.Log.LogEntryDebug("Player chose not to leave dungeon at level " + Game.Dungeon.Player.LocationLevel, LogDebugLevel.Low);
                return false;
            }

            //Go back to town
            LogFile.Log.LogEntry("Player leaving dungeon");
            LogFile.Log.LogEntryDebug("Player leaving dungeon at level " + Game.Dungeon.Player.LocationLevel, LogDebugLevel.Medium);

            Game.MessageQueue.AddMessage("You make it safely back to school without being discovered!");

            Game.Dungeon.PlayerLeavesDungeon();

            return true;
        }

        protected override char GetRepresentation()
        {
            return '<';
        }
    }
}
