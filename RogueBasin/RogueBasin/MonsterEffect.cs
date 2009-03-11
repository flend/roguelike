using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a creature event that has a duration in the game.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(MonsterEffects.SlowDown))]
    public abstract class MonsterEffect : CreatureEffect
    {
        protected Monster monster;

        public MonsterEffect(Monster eventReceiver) : base(eventReceiver)
        {
            this.monster = eventReceiver;
        }
    }
}
