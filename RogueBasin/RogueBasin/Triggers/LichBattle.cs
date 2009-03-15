using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class LichBattle : DungeonSquareTrigger
    {

        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public LichBattle()
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
                Screen.Instance.PlayMovie("lichBattle", true);

                //Wake up the lich
                Creatures.Lich lich = null;
                foreach (Monster m in Game.Dungeon.Monsters)
                {
                    if(m is Creatures.Lich) {
                        lich = m as Creatures.Lich;
                    }
                }

                if (lich == null)
                {
                    LogFile.Log.LogEntry("Can't find the lich to start the final battle");
                }
                else
                {
                    lich.Sleeping = false;
                }



                Triggered = true;
            }
            
            return true;
        }
    }
}
