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
        public bool Triggered { get; set; } //Needs to be serialized

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
                Screen.Instance.PlayMovie("enterDungeon", true);
                Triggered = true;
            }
            else
            {
                Screen.Instance.PlayMovie("enterDungeonAgain", true);
            }

            return true;
        }
    }
}
