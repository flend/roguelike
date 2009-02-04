using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a global event on the dungeon
    /// </summary>
    public abstract class DungeonEffect : Effect
    {
        Dungeon dungeon;

        int currentTicks = 0;

        bool hasEnded = false;
        
        public DungeonEffect(Dungeon eventReceiver)
        {
            this.dungeon = eventReceiver;
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
