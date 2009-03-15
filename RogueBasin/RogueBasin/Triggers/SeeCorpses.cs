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

        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public SeeCorpses()
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

            if (!Triggered)
            {
                if (Game.Dungeon.PercentRemembered() > 80)
                {
                    Screen.Instance.PlayMovie("seeCorpses", true);
                }
                else
                {
                    Screen.Instance.PlayMovie("seeCorpsesForgetful", true);
                }
                Triggered = true;
            }

            return true;
        }
    }
}
