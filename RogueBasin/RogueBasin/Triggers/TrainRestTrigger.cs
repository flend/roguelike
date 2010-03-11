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
    }
}
