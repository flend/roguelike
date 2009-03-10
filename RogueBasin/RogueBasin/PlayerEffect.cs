using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a temporary effect on the player
    /// </summary>

    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.Healing))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SpeedUp))]
    public abstract class PlayerEffect : CreatureEffect
    {
        protected Player player;

        public PlayerEffect(Player eventReceiver) : base(eventReceiver)
        {
            player = eventReceiver;
        }
    }
}
