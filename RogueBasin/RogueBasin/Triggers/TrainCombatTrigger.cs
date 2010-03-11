using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Combat training
    /// </summary>
    public class TrainCombatTrigger : TrainTrigger
    {
        protected override string GetIntroMovieName()
        {
            return "traincombat";
        }

        protected override string GetTrainingTypeString()
        {
            return "Training: Combat practice";
        }

        protected override TrainStats DoWeekdayTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekdayTrainCombat(Game.Dungeon.Player);
            return train;
        }

        protected override TrainStats DoWeekendTraining()
        {
            TrainStats train = new TrainStats();
            train.WeekendTrainCombat(Game.Dungeon.Player);
            return train;
        }
    }
}
