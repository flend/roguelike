using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class SpotFriend : DungeonSquareTrigger
    {

        public SpotFriend()
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
                Game.Base.PlayMovie("spotFriend", true);
                Triggered = true;
            }

            return true;
        }
    }
}
