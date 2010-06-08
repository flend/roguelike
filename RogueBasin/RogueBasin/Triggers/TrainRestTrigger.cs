using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class TrainRestTrigger : TrainTrigger
    {
        protected override string GetIntroMovieName()
        {
            return "trainrest";
        }

        protected override string GetTrainingTypeString()
        {
            return "Training: Day off!";
        }

        protected override TrainStats DoWeekdayTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekdayTrainRest(Game.Dungeon.Player);
            return train;
        }

        protected override TrainStats DoWeekendTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekendTrainRest(Game.Dungeon.Player);
            return train;
        }

        //Override again for the store message
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
            List<TrainStats> trainingRecord = new List<TrainStats>();

            if (dungeon.IsWeekday())
            {

                //Carry out training and load up the UI

                Screen.Instance.TrainingTypeString = GetTrainingTypeString();
                Screen.Instance.ClearTrainingStatsRecord();

                for (int i = 0; i < 5; i++)
                {
                    TrainStats train = DoWeekdayTraining();
                    trainingRecord.Add(train);
                    Screen.Instance.AddTrainingStatsRecord(train);
                }
                doesTraining = true;
            }
            else if (dungeon.IsNormalWeekend())
            {

                Screen.Instance.TrainingTypeString = GetTrainingTypeString();
                Screen.Instance.ClearTrainingStatsRecord();

                for (int i = 0; i < 2; i++)
                {
                    TrainStats train = DoWeekendTraining();
                    trainingRecord.Add(train);
                    Screen.Instance.AddTrainingStatsRecord(train);
                }
                doesTraining = true;
            }
            else
            {

                //Adventure weekend
                Game.MessageQueue.AddMessage("You can go to get your stuff from the store.");
            }

            //Run the UI then update the player's stats

            if (doesTraining)
            {
                RunTrainingUI();

                foreach (TrainStats stats in trainingRecord)
                {
                    stats.ApplyDeltasToPlayer();
                }
            }

            return true;
        }

    }
}
