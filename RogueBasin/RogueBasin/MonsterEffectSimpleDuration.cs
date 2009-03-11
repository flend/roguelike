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
        int currentTicks = 0;

        bool hasEnded = false;

        public MonsterEffectSimpleDuration(Monster monster)
            : base(monster)
        {
            
        }

        /// <summary>
        /// Returns the duration in world ticks. Implement in derived classes
        /// </summary>
        /// <returns></returns>
        protected abstract int GetDuration();

        /// <summary>
        /// Increment time - if we have exceeded the duration, call OnExit() and then mark as finished
        /// </summary>
        public override void IncrementTime()
        {
            currentTicks++;

            if (currentTicks > GetDuration())
            {
                OnEnd();
                hasEnded = true;
            }
        }

        public override bool HasEnded()
        {
            return hasEnded;
        }
    }
}
