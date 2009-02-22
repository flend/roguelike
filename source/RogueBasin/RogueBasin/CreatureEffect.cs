using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public abstract class CreatureEffect : Effect
    {
        protected Creature creature;

        public CreatureEffect(Creature eventReceiver)
        {
            this.creature = eventReceiver;
        }
    }
}
