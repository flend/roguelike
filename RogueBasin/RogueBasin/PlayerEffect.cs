using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Represents a temporary effect on the player
    /// </summary>

    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.Healing))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.MultiDamage))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.ToHitUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.DamageUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SightRadiusDown))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SightRadiusUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SpeedDown))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.MPRestore))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SeeFOV))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.StealthField))]
    public abstract class PlayerEffect
    {
        public PlayerEffect()
        {
            
        }

        /// <summary>
        /// Carries out the start effects on the target. Many standard effects just use the stat modifier virtual functions.
        /// </summary>
        public abstract void OnStart(Player target);

        /// <summary>
        /// Carries out the end effects on the target. Many standard effects just use the stat modifier virtual functions.
        /// </summary>
        public abstract void OnEnd(Player target);

        /// <summary>
        /// Called every click. If the event duration is over, call OnEnd() and mark as ended
        /// </summary>
        public abstract void IncrementTime(Player target);

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

        /// <summary>
        /// Modifier for normal sight radius
        /// </summary>
        /// <returns></returns>
        public virtual int SightModifier() { return 0; }

        public virtual string GetName() { return ""; }


        internal virtual System.Drawing.Color GetColor()
        {
            return System.Drawing.Color.White;
        }
    }
}
