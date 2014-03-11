using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a creature event that has a duration in the game.
    /// </summary>
    public abstract class ItemEffect
    {

        public ItemEffect()
        {
        }

        /// <summary>
        /// Carries out the start effects on the target.
        /// </summary>
        public abstract void OnStart(Item target);

        /// <summary>
        /// Carries out the end effects on the target
        /// </summary>
        public abstract void OnEnd(Item target);

        /// <summary>
        /// Called every click. If the event duration is over, call OnEnd() and mark as ended
        /// </summary>
        public abstract void IncrementTime(Item target);

        /// <summary>
        /// Has the event ended and can be deleted from the queue?
        /// </summary>
        public abstract bool HasEnded();
    }
}
