using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.MonsterEffects
{
    class SlowDown : MonsterEffectSimpleDuration
    {
        int duration;

        int speedEffect;

        public SlowDown(Monster monster, int duration, int speedEffect)
            : base(monster)
        {
            this.duration = duration;
            this.speedEffect = speedEffect;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnStart()
        {
            LogFile.Log.LogEntry("SlowDown started");
            Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " looks stunned!");

            monster.Speed -= speedEffect;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnEnd()
        {
            LogFile.Log.LogEntry("SlowDown ended");
            Game.MessageQueue.AddMessage("The " + monster.SingleDescription + "recovers.");

            monster.Speed += speedEffect;
        }

        protected override int GetDuration()
        {
            return duration;
        }
    }
}
