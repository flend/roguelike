using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents an event that has a duration in the game.
    /// Has OnStart(), OnEnd() events that act on objects
    /// </summary>
    public abstract class Effect
    {
        /// <summary>
        /// Carries out the start effects on the event receiver
        /// </summary>
        public abstract void OnStart();

        /// <summary>
        /// Carries out the end effects on the event receiver
        /// </summary>
        public abstract void OnEnd();

        /// <summary>
        /// Called every click. If the event duration is over, call OnEnd() and mark as ended
        /// </summary>
        public abstract void IncrementTime();

        /// <summary>
        /// Has the event ended and can be deleted from the queue?
        /// </summary>
        public abstract bool HasEnded();

    }
}
