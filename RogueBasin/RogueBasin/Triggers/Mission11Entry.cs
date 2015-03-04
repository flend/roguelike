﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class Mission11Entry : DungeonSquareTrigger
    {

        public Mission11Entry()
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

            //Mission 2 tutorial
            //Always play the late ones
            Game.Base.PlayMovie("mission11", true);

            Triggered = true;

            return true;
        }
    }
}
