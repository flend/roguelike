using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// A player event with a simple ticks duration
    /// </summary>
    public abstract class MonsterEffectSimpleDuration : MonsterEffect
    {
        public int currentTicks { get; set; }

        public bool hasEnded { get; set; }

        public MonsterEffectSimpleDuration()
        {
            this.currentTicks = 0;
            this.hasEnded = false;
        }

        /// <summary>
        /// Returns the duration in world ticks. Implement in derived classes
        /// </summary>
        /// <returns></returns>
        protected abstract int GetDuration();

        /// <summary>
        /// Increment time - if we have exceeded the duration, call OnExit() and then mark as finished
        /// </summary>
        public override void IncrementTime(Monster target)
        {
            currentTicks++;

            if (currentTicks > GetDuration())
            {
                OnEnd(target);
                hasEnded = true;
                target.CalculateCombatStats();
            }
        }

        public override bool HasEnded()
        {
            return hasEnded;
        }
    }
}
