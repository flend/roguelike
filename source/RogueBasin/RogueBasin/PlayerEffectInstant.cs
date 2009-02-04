using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// An effect that has an immediate effect (like a healing potion) but no duration
    /// </summary>
    public abstract class PlayerEffectInstant : PlayerEffect
    {
        public PlayerEffectInstant(Player player)
            : base(player)
        {

        }

        /// <summary>
        /// Instant events have no OnEnd
        /// </summary>
        public override void OnEnd()
        {
            
        }

        /// <summary>
        /// This event will be removed immediately
        /// </summary>
        /// <returns></returns>
        public override bool HasEnded()
        {
            return true;
        }

        /// <summary>
        /// Do nothing here, HasEnded() is always set
        /// </summary>
        public override void IncrementTime()
        {
            
        }
    }
}
