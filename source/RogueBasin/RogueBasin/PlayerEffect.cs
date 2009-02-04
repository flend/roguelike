using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a temporary effect on the player
    /// </summary>
    public abstract class PlayerEffect : Effect
    {
        protected Player player;

        public PlayerEffect(Player eventReceiver)
        {
            player = eventReceiver;
        }
    }
}
