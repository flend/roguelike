using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    public class DockDoor : DungeonSquareTrigger
    {

        public DockDoor()
        {

        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place - should be in the base I think
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }

            //(Don't check on triggered, since it's global for these events

            //Ensure that this level's entered flat is set

            Game.Dungeon.DungeonInfo.Dungeons[Game.Dungeon.Player.LocationLevel].PlayerLeftDock = true;

            return true;
        }
    }
}
