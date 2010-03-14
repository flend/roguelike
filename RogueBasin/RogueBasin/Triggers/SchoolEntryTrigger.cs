using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the school for the first time
    /// </summary>
    public class SchoolEntryTrigger : DungeonSquareTrigger
    {
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public SchoolEntryTrigger()
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
                Screen.Instance.PlayMovie("enterschool", true);

                if (Game.Dungeon.Player.PlayItemMovies)
                {
                    Screen.Instance.PlayMovie("helpkeys", true);
                }
                Triggered = true;
            }

            return true;
        }
    }
}
