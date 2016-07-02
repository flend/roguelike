﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class ArenaEntry : DungeonSquareTrigger
    {

        public ArenaEntry()
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

            Game.MessageQueue.AddMessage("The door slams shut behind you");

            Game.Dungeon.ShutDoor(level);

            Triggered = true;

            return true;
        }
    }
}
