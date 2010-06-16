using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a global event on the dungeon
    /// </summary>
    public abstract class DungeonEffect
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
        /// Carries out the start effects on the event receiver
        /// </summary>
        public abstract void OnStart();

        /// <summary>
        /// Carries out the end effects on the event receiver
        /// </summary>
        public abstract void OnEnd();

        /// <summary>
        /// Increment time - if we have exceeded the duration, call OnExit() and then mark as finished
        /// </summary>
        public virtual void IncrementTime()
        {
            currentTicks++;

            if (currentTicks > GetDuration())
            {
                OnEnd();
                hasEnded = true;
            }
        }

        public virtual bool HasEnded()
        {
            return hasEnded;
        }
    }
}
