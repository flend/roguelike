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
        public bool Triggered { get; set; }

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
