using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class TrainLeaveSchoolTrigger : DungeonSquareTrigger
    {
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public TrainLeaveSchoolTrigger()
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

            //If this is the first time, give some flavour text - to do
            string movieName = "";
            
            if(movieName != "")
                Screen.Instance.PlayMovie(movieName, false);

            Triggered = true;

            Dungeon dungeon = Game.Dungeon;

            if(dungeon.IsWeekday()) {

                //Normal weekday
                Game.MessageQueue.AddMessage("You're expected in classes during the week. No way to sneak out.");
                Game.Dungeon.PlayerBackToTown();
            }
            else if(dungeon.IsNormalWeekend()) {

                //Normal weekday
                Game.MessageQueue.AddMessage("The masters are still around. No chance to sneak out today.");
                Game.Dungeon.PlayerBackToTown();
            }
            else {

                //Adventure weekend
                Game.MessageQueue.AddMessage("The masters have retired to their rooms - now's your chance!");
            }

            return true;
        }

        protected void RunTrainingUI()
        {
            //Show training UI
            Screen.Instance.DisplayTrainingUI = true;
            Screen.Instance.DrawAndFlush();

            bool continueLooking = true;

            while (continueLooking)
            {
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {
                    char keyCode = (char)userKey.Character;

                    //Exit out of inventory
                    if (keyCode == 'x')
                    {
                        Screen.Instance.DisplayTrainingUI = false;
                        Screen.Instance.DrawAndFlush();
                        continueLooking = false;
                    }
                }
            }

            //Increment calendar time
            Game.Dungeon.MoveToNextDate();

            //Teleport the user back to the start location
            Game.Dungeon.PlayerBackToTown();
        }
    }
}
