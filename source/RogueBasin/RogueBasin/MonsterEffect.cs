using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a creature event that has a duration in the game.
    /// </summary>
    public abstract class MonsterEffect : Effect
    {
        Monster monster;

        public MonsterEffect(Monster eventReceiver)
        {
            this.monster = eventReceiver;
        }
    }
}
