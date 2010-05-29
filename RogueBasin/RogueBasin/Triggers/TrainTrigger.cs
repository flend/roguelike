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
        public TrainTrigger()
        {

        }

        /// <summary>
        /// First run movie
        /// </summary>
        /// <returns></returns>
        protected virtual string GetIntroMovieName() { return ""; }

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

            //If this is the first time, give some flavour text
            if (Triggered == false)
            {
                string movieName = GetIntroMovieName();

                if (movieName != "")
                    Screen.Instance.PlayMovie(movieName, false);
            }

            Triggered = true;

            //We run the training regime depending on the inherited class

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
            Screen.Instance.UpdateNoMsgQueue();

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
                        Screen.Instance.UpdateNoMsgQueue();
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
