using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class SeeCorpses : DungeonSquareTrigger
    {

        public SeeCorpses()
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
                if (Game.Dungeon.PercentRemembered() > 80)
                {
                    Game.Base.PlayMovie("seeCorpses", true);
                }
                else
                {
                    Game.Base.PlayMovie("seeCorpsesForgetful", true);
                }
                Triggered = true;
            }

            return true;
        }
    }
}
