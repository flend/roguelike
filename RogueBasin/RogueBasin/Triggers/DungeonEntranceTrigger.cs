using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square.
    /// </summary>
    public class DungeonEntranceTrigger : DungeonSquareTrigger
    {
        public DungeonEntranceTrigger()
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
