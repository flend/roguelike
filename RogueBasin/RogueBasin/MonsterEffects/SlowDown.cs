using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.MonsterEffects
{
    public class SlowDown : MonsterEffectSimpleDuration
    {
        public int duration { get; set; }

        public int speedEffect  { get; set; }

        public SlowDown() { }

        public SlowDown(int duration, int speedEffect)
        {
            this.duration = duration;
            this.speedEffect = speedEffect;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnStart(Creature target)
        {
            Monster monster = target as Monster;

            LogFile.Log.LogEntry("SlowDown started");
            Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " slows!");

            target.Speed -= speedEffect;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnEnd(Creature target)
        {
            Monster monster = target as Monster;

            LogFile.Log.LogEntry("SlowDown ended");
            Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " recovers.");

            target.Speed += speedEffect;
        }

        protected override int GetDuration()
        {
            return duration;
        }
    }
}
