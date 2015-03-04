using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class PrinceInABox : DungeonSquareTrigger
    {

        public PrinceInABox()
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

            if (!Game.Dungeon.DungeonInfo.DragonDead)
            {
                Game.Base.PlayMovie("princeincage", true);
                return false;
            }
            else
            {
                Game.Base.PlayMovie("letoutprince", true);
                //End of the game
                Game.Dungeon.PlayerLeavesDungeon();
            }
            
            return true;
        }
    }
}
