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

        //Creature effects can now define these which are used to calculate the creature or player's combat stats

        public int ArmourClassModifier() { return 0; }

        public int DamageBase() { return 0; }

        public int DamageModifier() { return 0; }

        public int HitModifier() { return 0; }

    }
}
