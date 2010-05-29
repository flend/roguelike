using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class ApproachingTheDragon : DungeonSquareTrigger
    {

        public ApproachingTheDragon()
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

            if (!Triggered && !Game.Dungeon.DungeonInfo.DragonDead)
            {
                Screen.Instance.PlayMovie("dragonapproach", true);
            }

            return true;
        }
    }
}
