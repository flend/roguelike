using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Represents a creature event that has a duration in the game.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(MonsterEffects.SlowDown))]
    public abstract class MonsterEffect
    {
        
        public MonsterEffect()
        {
        }

        /// <summary>
        /// Carries out the start effects on the target.
        /// </summary>
        public abstract void OnStart(Monster target);

        /// <summary>
        /// Carries out the end effects on the target
        /// </summary>
        public abstract void OnEnd(Monster target);

        /// <summary>
        /// Called every click. If the event duration is over, call OnEnd() and mark as ended
        /// </summary>
        public abstract void IncrementTime(Monster target);

        /// <summary>
        /// Has the event ended and can be deleted from the queue?
        /// </summary>
        public abstract bool HasEnded();

        //Creature effects can now define these which are used to calculate the creature or player's combat stats

        public virtual int ArmourClassModifier() { return 0; }

        public virtual int DamageBase() { return 0; }

        public virtual double DamageModifier() { return 0; }

        public virtual int HitModifier() { return 0; }

        public virtual int SpeedModifier() { return 0; }
    }
}
