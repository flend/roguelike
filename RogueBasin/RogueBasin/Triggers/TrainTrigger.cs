using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Generic train trigger, provides some useful functionality
    /// </summary>
    public abstract class TrainTrigger : DungeonSquareTrigger
    {
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public TrainTrigger()
        {
            Triggered = false;
        }

        /// <summary>
        /// Name to display
        /// </summary>
        protected abstract string GetTrainingTypeString();

        /// <summary>
        /// Override with appropriate weekday training
        /// </summary>
        protected abstract TrainStats DoWeekdayTraining();

        /// <summary>
        /// Override with appropriate weekend training
        /// </summary>
        protected abstract TrainStats DoWeekendTraining();

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

                Screen.Instance.TrainingTypeString = GetTrainingTypeString();
                Screen.Instance.ClearTrainingStatsRecord();

                for (int i = 0; i < 5; i++)
                {
                    TrainStats train = DoWeekdayTraining();
                    Screen.Instance.AddTrainingStatsRecord(train);
                }
                doesTraining = true;
            }
            else if(dungeon.IsNormalWeekend()) {

                Screen.Instance.TrainingTypeString = GetTrainingTypeString();
                Screen.Instance.ClearTrainingStatsRecord();

                for (int i = 0; i < 2; i++)
                {
                    TrainStats train = DoWeekendTraining();
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
                RunTrainingUI();
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
