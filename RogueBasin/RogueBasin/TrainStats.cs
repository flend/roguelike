﻿namespace RogueBasin
{
    /// <summary>
    /// Class for training stats. Works like a dialog, run the function then query the state for what happened. Does however make changes to player
    /// </summary>
    public class TrainStats
    {
        /// <summary>
        /// Should be set before any stat raising takes place
        /// </summary>
        private Player player;

        public int HitpointsStatDelta { get; set; }
        public int MaxHitpointsStatDelta { get; set; }
        public int SpeedStatDelta { get; set; }
        public int AttackStatDelta { get; set; }
        public int CharmStatDelta { get; set; }
        public int MagicStatDelta { get; set; }

        public const int minimumHitpointStat = 1;

        /// <summary>
        /// Apply the deltas we have calculated to the player. Do this after the dialogue so the screen redraw looks reasonable
        /// </summary>
        public void ApplyDeltasToPlayer() {

            player.HitpointsStat += HitpointsStatDelta;
            player.MaxHitpointsStat += MaxHitpointsStatDelta;
            player.SpeedStat += SpeedStatDelta;
            player.AttackStat += AttackStatDelta;
            player.CharmStat += CharmStatDelta;
            player.MagicStat += MagicStatDelta;
        }

        public void TrainHitpointStat(int chance, int amount)
        {
            if (Game.Random.Next(100) < chance)
            {
                if(player.HitpointsStat + amount > player.MaxHitpointsStat) {
                    amount = player.MaxHitpointsStat - player.HitpointsStat;
                }

                if (player.HitpointsStat + amount < minimumHitpointStat)
                    amount = minimumHitpointStat + -1 * player.HitpointsStat;

                //player.HitpointsStat += amount;
                HitpointsStatDelta += amount;
            }
        }

        public void TrainMaxHitpointStat(int chance, int amount)
        {
            if (Game.Random.Next(100) < chance)
            {
                if (player.MaxHitpointsStat + amount < minimumHitpointStat)
                    amount = minimumHitpointStat + -1 * player.MaxHitpointsStat;

                //player.MaxHitpointsStat += amount;
                MaxHitpointsStatDelta += amount;

                //Check that max hitpoints isn't now below hitpoints
                if (player.HitpointsStat > player.MaxHitpointsStat)
                {
                    amount = player.HitpointsStat - player.MaxHitpointsStat;
                }
                //player.HitpointsStat += amount;
                HitpointsStatDelta += amount;
            }
        }

        public void TrainAttackStat(int chance, int amount)
        {
            if (Game.Random.Next(100) < chance)
            {
                if (player.AttackStat + amount < 0)
                    amount = -1 * player.AttackStat;

                //player.AttackStat += amount;
                AttackStatDelta += amount;
            }
        }

        public void TrainSpeedStat(int chance, int amount)
        {
            if (Game.Random.Next(100) < chance)
            {
                if (player.SpeedStat + amount < 0)
                    amount = -1 * player.SpeedStat;

                //player.SpeedStat += amount;
                SpeedStatDelta += amount;
            }
        }

        public void TrainCharmStat(int chance, int amount)
        {
            if (Game.Random.Next(100) < chance)
            {
                if (player.CharmStat + amount < 0)
                    amount = -1 * player.CharmStat;

                //player.CharmStat += amount;
                CharmStatDelta += amount;
            }
        }

        public void TrainMagicStat(int chance, int amount)
        {
            if (Game.Random.Next(100) < chance)
            {
                if (player.MagicStat + amount < 0)
                    amount = -1 * player.MagicStat;

                //player.MagicStat += amount;
                MagicStatDelta += amount;
            }
        }

        /// <summary>
        /// Train REST 1 day at weekday level
        /// </summary>
        /// <param name="player"></param>
        public void WeekdayTrainRest(Player player)
        {

            //Set player
            this.player = player;

            TrainHitpointStat(75, 2);
            TrainMaxHitpointStat(0, 0);
            TrainSpeedStat(5, -1);
            TrainAttackStat(5, -1);
            TrainCharmStat(5, 1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train ATHLETICS 1 day at weekday level
        /// </summary>
        /// <param name="player"></param>
        public void WeekdayTrainAthletics(Player player)
        {
            this.player = player;


            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(50, -1);
            TrainMaxHitpointStat(50, 1);
            TrainSpeedStat(5, 1);
            TrainAttackStat(10, 1);
            TrainCharmStat(5, -1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train COMBAT 1 day at weekday level
        /// </summary>
        /// <param name="player"></param>
        public void WeekdayTrainCombat(Player player)
        {
            this.player = player;

            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(40, -1);
            TrainMaxHitpointStat(10, 1);
            TrainSpeedStat(5, 1);
            TrainAttackStat(50, 1);
            TrainCharmStat(5, -1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train CHARM 1 day at weekday level
        /// </summary>
        /// <param name="player"></param>
        public void WeekdayTrainCharm(Player player)
        {
            this.player = player;

            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(20, -1);
            TrainMaxHitpointStat(5, -1);
            TrainSpeedStat(0, 0);
            TrainAttackStat(10, -1);
            TrainCharmStat(50, 1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train MAGIC 1 day at weekday level
        /// </summary>
        /// <param name="player"></param>
        public void WeekdayTrainMagic(Player player)
        {
            this.player = player;

            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(20, -1);
            TrainMaxHitpointStat(5, -1);
            TrainSpeedStat(0, 0);
            TrainAttackStat(5, -1);
            TrainCharmStat(5, -1);
            TrainMagicStat(50, 1);
        }

        // <summary>
        /// Train REST 1 day at weekend level
        /// </summary>
        /// <param name="player"></param>
        public void WeekendTrainRest(Player player)
        {
            //Set player
            this.player = player;

            TrainHitpointStat(75, 4);
            TrainMaxHitpointStat(0, 0);
            TrainSpeedStat(5, -1);
            TrainAttackStat(5, -1);
            TrainCharmStat(5, 1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train ATHLETICS 1 day at weekend level
        /// </summary>
        /// <param name="player"></param>
        public void WeekendTrainAthletics(Player player)
        {
            this.player = player;

            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(50, -1);
            TrainMaxHitpointStat(50, 1);
            TrainSpeedStat(5, 1);
            TrainAttackStat(10, 1);
            TrainCharmStat(5, -1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train COMBAT 1 day at weekend level
        /// </summary>
        /// <param name="player"></param>
        public void WeekendTrainCombat(Player player)
        {
            this.player = player;

            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(40, -1);
            TrainMaxHitpointStat(10, 1);
            TrainSpeedStat(5, 1);
            TrainAttackStat(50, 1);
            TrainCharmStat(5, -1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train CHARM 1 day at weekday level
        /// </summary>
        /// <param name="player"></param>
        public void WeekendTrainCharm(Player player)
        {
            this.player = player;

            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(20, -1);
            TrainMaxHitpointStat(5, -1);
            TrainSpeedStat(0, 0);
            TrainAttackStat(5, -1);
            TrainCharmStat(50, 1);
            TrainMagicStat(5, -1);
        }

        /// <summary>
        /// Train MAGIC 1 day at weekday level
        /// </summary>
        /// <param name="player"></param>
        public void WeekendTrainMagic(Player player)
        {
            this.player = player;

            if (player.HitpointsStat == minimumHitpointStat)
            {
                Game.MessageQueue.AddMessage("You are so tired you can't get anything done. Time for rest?");
                return;
            }

            TrainHitpointStat(20, -1);
            TrainMaxHitpointStat(5, -1);
            TrainSpeedStat(0, 0);
            TrainAttackStat(5, -1);
            TrainCharmStat(5, -1);
            TrainMagicStat(50, 1);
        }
    }
}
