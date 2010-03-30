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

        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public bool Triggered { get; set; }

        public PrinceInABox()
        {
            Triggered = false;
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
                Screen.Instance.PlayMovie("princeincage", true);
                return false;
            }
            else
            {
                Screen.Instance.PlayMovie("letoutprince", true);
                //End of the game
                Game.Dungeon.PlayerLeavesDungeon();
            }
            
            return true;
        }
    }
}
