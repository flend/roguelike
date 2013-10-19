using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class TownToWilderness : DungeonSquareTrigger
    {

        public bool ShownMovie = false;

        public TownToWilderness()
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

            //Play intro movie first time
            if (!Triggered)
            {
                if (!ShownMovie && Game.Dungeon.Player.PlayItemMovies)
                {
                    ShownMovie = true;
                    Screen.Instance.PlayMovie("helpwilderness", false);
                }
                Triggered = true;
            }
            //Game.Dungeon.PlayerEnterWilderness();

            return true;
        }
    }
}
