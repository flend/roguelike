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
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.MultiDamage))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.ToHitUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.DamageUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SightRadiusDown))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SightRadiusUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SpeedDown))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.SpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerEffects.MPRestore))]
    public abstract class PlayerEffect : CreatureEffect
    {
        public PlayerEffect()
        {
            
        }
    }
}
