using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class TreasureRoom : DungeonSquareTrigger
    {

        public TreasureRoom()
        {

        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }
            //Otherwise in the right place

            if (!Triggered)
            {
                Game.Base.SystemActions.PlayMovie("treasureRoom", true);
                Triggered = true;

                //Teleport the player to the start location on the final level

                //Increment player level
                Player player = Game.Dungeon.Player;

                player.LocationLevel++;

                //Set vision
                player.SightRadius = 100;

                player.LocationMap = Game.Dungeon.Levels[player.LocationLevel].PCStartLocation;
                Game.Dungeon.MovePCAbsolute(player.LocationLevel, player.LocationMap.x, player.LocationMap.y);
            }

            return true;
        }
    }
}
