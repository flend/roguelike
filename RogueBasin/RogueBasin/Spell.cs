using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for SpecialMoves. Shouldn't be instantiated. Instantiate a child.
    /// Contains checks for whether we know a move or not.
    /// </summary>
    
    //I suspect we can't be abstract because of these - need to check

    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.StunBox))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallPush))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.ChargeAttack))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallVault))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.VaultBackstab))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.OpenSpaceAttack))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.Evade))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.MultiAttack))]
    //[System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.BurstOfSpeed))]
    [System.Xml.Serialization.XmlInclude(typeof(Spells.MagicMissile))]
    [System.Xml.Serialization.XmlInclude(typeof(Spells.FireLance))]
    [System.Xml.Serialization.XmlInclude(typeof(Spells.SlowMonster))]
    [System.Xml.Serialization.XmlInclude(typeof(Spells.Blink))]
    [System.Xml.Serialization.XmlInclude(typeof(Spells.FireBall))]
    public abstract class Spell
    {
        /// <summary>
        /// Has the player learnt this spell yet?
        /// </summary>
        public bool Known { get; set; }

        public Spell()
        {
           Known = false;
        }

        /// <summary>
        /// Is the spell targetable?
        /// </summary>
        /// <returns></returns>
        public virtual bool NeedsTarget() { return false; }

        /// <summary>
        /// Cost in MPs
        /// </summary>
        /// <returns></returns>
        public virtual int MPCost() { return 1; }

        /// <summary>
        /// Carry out the move (instead of normal move / attack)
        /// </summary>
        public virtual bool DoSpell(Point target) { return false; }

        /// <summary>
        /// Return a 4 character abbrevation for the spell
        /// </summary>
        /// <returns></returns>
        public virtual string Abbreviation() { return ""; }

        /// <summary>
        /// Return a presentable string for the name
        /// </summary>
        public virtual string SpellName() { return ""; }

        /// <summary>
        /// Name of the movie associated
        /// </summary>
        /// <returns></returns>
        internal virtual string MovieRoot()
        {
            return "";
        }

        /// <summary>
        /// Magic required for this spell
        /// </summary>
        /// <returns></returns>
        internal abstract int GetRequiredMagic();
        
    }
}
