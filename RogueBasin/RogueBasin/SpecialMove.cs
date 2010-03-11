using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for SpecialMoves. Shouldn't be instantiated. Instantiate a child.
    /// Contains checks for whether we know a move or not.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.StunBox))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallPush))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.ChargeAttack))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallVault))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.VaultBackstab))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.OpenSpaceAttack))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.Evade))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.MultiAttack))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.BurstOfSpeed))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.CloseQuarters))]
    public abstract class SpecialMove
    {
        /// <summary>
        /// Has the player learnt this move yet?
        /// </summary>
        public bool Known { get; set; }

        public SpecialMove() {
           Known = false;
        }

        /// <summary>
        /// Check the player's last action, does it fit with the move?
        /// Return code: true is fits, false if it caused a failure.
        /// We run this function twice if it fails at first. This is because a move may cause a failure at second stage, but be valid as a new first stage. The first run clears sta
        /// </summary>
        /// <param name="isMove"></param>
        /// <param name="isAttack"></param>
        /// <param name="locationAfterMove"></param>
        public virtual bool CheckAction(bool isMove, Point locationAfterMove) {
            return true;
        }

        /// <summary>
        /// Is the move complete and will fire?
        /// </summary>
        /// <returns></returns>
        public virtual bool MoveComplete() { return false; }

        /// <summary>
        /// Carry out the move (instead of normal move / attack)
        /// </summary>
        public virtual void DoMove(Point locationAfterMove) { }

        /// <summary>
        /// Clear the counter - used when another move has triggered
        /// </summary>
        public virtual void ClearMove() { }

        /// <summary>
        /// Return a 4 character abbrevation for the move
        /// </summary>
        /// <returns></returns>
        public virtual string Abbreviation() { return ""; }

        /// <summary>
        /// Return the root of the movie for this special move
        /// </summary>
        public virtual string MovieRoot() { return ""; }

        /// <summary>
        /// Return a presentable string for the name
        /// </summary>
        public virtual string MoveName() { return ""; }

        /// <summary>
        /// Total stages in move
        /// </summary>
        /// <returns></returns>
        public virtual int TotalStages() { return 0; }

        /// <summary>
        /// Current stage in move
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentStage() { return 0; }

        /// <summary>
        /// Current stage in move
        /// </summary>
        /// <returns></returns>
        public abstract int GetRequiredCombat();
    }
}
