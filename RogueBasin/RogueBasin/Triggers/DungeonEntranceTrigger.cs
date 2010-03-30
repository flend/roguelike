using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class DungeonEntranceTrigger : DungeonSquareTrigger
    {
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public bool Triggered { get; set; }

        public DungeonEntranceTrigger()
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
                //Set vision
                //Game.Dungeon.Player.SightRadius = (int)Math.Ceiling(Game.Dungeon.Player.NormalSightRadius * Game.Dungeon.Levels[0].LightLevel);

                if (Game.Dungeon.Player.PlayItemMovies)
                {
                    Screen.Instance.PlayMovie("helpdungeons", true);
                }
                Triggered = true;
            }
            
            return true;
        }
    }
}
