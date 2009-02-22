using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a creature event that has a duration in the game.
    /// </summary>
    public abstract class MonsterEffect : CreatureEffect
    {
        Monster monster;

        public MonsterEffect(Monster eventReceiver) : base(eventReceiver)
        {
            this.monster = eventReceiver;
        }
    }
}
