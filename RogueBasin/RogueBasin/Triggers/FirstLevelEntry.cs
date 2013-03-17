using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class FirstLevelEntry : DungeonSquareTrigger
    {

        public FirstLevelEntry()
        {

        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place - should be in the base I think
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }

            //Have we triggered already?

            if (Triggered)
                return false;

            //Initial game entry

            Screen.Instance.PlayMovie("enterflatline", true);

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                Screen.Instance.PlayMovie("helpkeys", true);
                Screen.Instance.PlayMovie("helpnewuser", true);
            }

            //Screen.Instance.PlayMovie("mission0", true);

            Triggered = true;

            return true;
        }
    }
}
