using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Athletics training
    /// </summary>
    public class TrainAthleticsTrigger : TrainTrigger
    {
        protected override string GetIntroMovieName()
        {
            return "trainathletics";
        }

        protected override string GetTrainingTypeString()
        {
            return "Training: Athletics Workout!";
        }

        protected override TrainStats DoWeekdayTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekdayTrainAthletics(Game.Dungeon.Player);
            return train;
        }

        protected override TrainStats DoWeekendTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekendTrainAthletics(Game.Dungeon.Player);
            return train;
        }
    }
}
