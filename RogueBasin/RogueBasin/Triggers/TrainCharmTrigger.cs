using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Athletics training
    /// </summary>
    public class TrainCharmTrigger : TrainTrigger
    {
        protected override string GetIntroMovieName()
        {
            return "traincharm";
        }

        protected override string GetTrainingTypeString()
        {
            return "Training: Etiquette and dance class";
        }

        protected override TrainStats DoWeekdayTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekdayTrainCharm(Game.Dungeon.Player);
            return train;
        }

        protected override TrainStats DoWeekendTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekendTrainCharm(Game.Dungeon.Player);
            return train;
        }
    }
}
