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
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallVault))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.VaultBackstab))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.OpenSpaceAttack))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.Evade))]
    public class SpecialMove
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
        /// </summary>
        /// <param name="isMove"></param>
        /// <param name="isAttack"></param>
        /// <param name="locationAfterMove"></param>
        public virtual void CheckAction(bool isMove, Point locationAfterMove) { 
        
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
        /// Return the root of the movie for this special move
        /// </summary>
        public virtual string MovieRoot() { return ""; }
    }
}
