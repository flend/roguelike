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

        public virtual int ArmourClassModifier() { return 0; }

        public virtual int DamageBase() { return 0; }

        public virtual int DamageModifier() { return 0; }

        public virtual int HitModifier() { return 0; }

        public virtual int SpeedModifier() { return 0; }


    }
}
