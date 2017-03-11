﻿using System;
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

            if (Triggered || Game.Dungeon.DungeonInfo.Dungeons[0].PlayerLeftDock)
                return false;

            //Initial game entry

           // Game.Base.SystemActions.PlayMovie("enterflatline", true);

            if (Game.Dungeon.Player.PlayItemMovies)
            {
               //// Game.Base.SystemActions.PlayMovie("helpkeys", true);
               // Game.Base.SystemActions.PlayMovie("helpnewuser", true);
            }

            //Game.Base.SystemActions.PlayMovie("mission0", true);

            Triggered = true;

            return true;
        }
    }
}
