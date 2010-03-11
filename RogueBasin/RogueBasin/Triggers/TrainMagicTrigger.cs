using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Athletics training
    /// </summary>
    public class TrainMagicTrigger : TrainTrigger
    {
        protected override string GetIntroMovieName()
        {
            return "trainmagic";
        }

        protected override string GetTrainingTypeString()
        {
            return "Training: Magic practice";
        }

        protected override TrainStats DoWeekdayTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekdayTrainMagic(Game.Dungeon.Player);
            return train;
        }

        protected override TrainStats DoWeekendTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekendTrainMagic(Game.Dungeon.Player);
            return train;
        }
    }
}
