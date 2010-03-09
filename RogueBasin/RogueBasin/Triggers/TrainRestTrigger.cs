using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class TrainRestTrigger : DungeonSquareTrigger
    {
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public TrainRestTrigger()
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

            //We run the training regime for Rest

            Dungeon dungeon = Game.Dungeon;

            bool doesTraining = false;
            

            if(dungeon.IsWeekday()) {

                //Carry out training and load up the UI

                Screen.Instance.TrainingTypeString = "Training: Rest & Relaxation";
                Screen.Instance.ClearTrainingStatsRecord();

                for (int i = 0; i < 5; i++)
                {
                    TrainStats train = new TrainStats();
                    train.WeekdayTrainRest(Game.Dungeon.Player);
                    Screen.Instance.AddTrainingStatsRecord(train);
                }
                doesTraining = true;
            }
            else if(dungeon.IsNormalWeekend()) {

                Screen.Instance.TrainingTypeString = "Training: Rest & Relaxation";
                Screen.Instance.ClearTrainingStatsRecord();

                for (int i = 0; i < 2; i++)
                {
                    TrainStats train = new TrainStats();
                    train.WeekendTrainRest(Game.Dungeon.Player);
                    Screen.Instance.AddTrainingStatsRecord(train);
                }
                doesTraining = true;
            }
            else {

                //Adventure weekend
                Game.MessageQueue.AddMessage("Surely there's something better to do this weekend!");

            }

            if (doesTraining)
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
                dungeon.MoveToNextDate();

                //Teleport the user back to the start location
                dungeon.PlayerBackToTown();
            }

            return true;
        }
    }
}
