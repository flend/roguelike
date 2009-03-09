using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public abstract class SpecialMove
    {

        /// <summary>
        /// Check the player's last action, does it fit with the move?
        /// </summary>
        /// <param name="isMove"></param>
        /// <param name="isAttack"></param>
        /// <param name="locationAfterMove"></param>
        public abstract void CheckAction(bool isMove, Point locationAfterMove);

        /// <summary>
        /// Is the move complete and will fire?
        /// </summary>
        /// <returns></returns>
        public abstract bool MoveComplete();

        /// <summary>
        /// Carry out the move (instead of normal move / attack)
        /// </summary>
        public abstract void DoMove(Point locationAfterMove);

        /// <summary>
        /// Clear the counter - used when another move has triggered
        /// </summary>
        public abstract void ClearMove();

        /// <summary>
        /// Return the root of the movie for this special move
        /// </summary>
        public abstract string MovieRoot();
    }
}
