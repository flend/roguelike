using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Effects are stored on the creature. They carry out their effects via the OnStart() and OnEnd() methods
    /// The storing creature should pass OnStart(this) OnEnd(this)
    /// This saves storing the creature in the effect which, although nice, is hard to serialize
    /// </summary>
    public abstract class CreatureEffect
    {

        public CreatureEffect()
        {
        }

        /// <summary>
        /// Carries out the start effects on the target.
        /// </summary>
        public abstract void OnStart(Creature target);

        /// <summary>
        /// Carries out the end effects on the target
        /// </summary>
        public abstract void OnEnd(Creature target);

        /// <summary>
        /// Called every click. If the event duration is over, call OnEnd() and mark as ended
        /// </summary>
        public abstract void IncrementTime(Creature target);

        /// <summary>
        /// Has the event ended and can be deleted from the queue?
        /// </summary>
        public abstract bool HasEnded();

        //Creature effects can now define these which are used to calculate the creature or player's combat stats

        public virtual int ArmourClassModifier() { return 0; }

        public virtual int DamageBase() { return 0; }

        public virtual int DamageModifier() { return 0; }

        public virtual int HitModifier() { return 0; }

        public virtual int SpeedModifier() { return 0; }


    }
}
