using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// Walk in a square to cause a stun effect or similar
    /// Square directions in a clockwise fashion
    /// </summary>
    public class StunBox : SpecialMove
    {
        int moveCounter = 0;

        int lastDeltaX = 0;
        int lastDeltaY = 0;


        public StunBox()
        {
        }

        public override void CheckAction(bool isMove, Point locationAfterMove)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //No interruptions
            if (!isMove)
            {
                FailInterrupted();
                return;
            }

            //Something in the square we're trying to enter blocks us
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

            //Monster
            if(squareContents.monster != null) {
                FailBlocked();
                return;
            }

            //Bad terrain
            if(!dungeon.MapSquareCanBeEntered(player.LocationLevel, locationAfterMove)) {
                FailBlocked();
                return;
            }

            int thisDeltaX = locationAfterMove.x - player.LocationMap.x;
            int thisDeltaY = locationAfterMove.y - player.LocationMap.y;

            //First move, can be any non diagonal move
            if (moveCounter == 0)
            {
                if (thisDeltaX != 0 && thisDeltaY != 0)
                {
                    FailWrongPattern();
                    return;
                }

                //Otherwise OK
            }
            else
            {
                //Latter moves must be in a clockwise box

                if (lastDeltaX == -1 && lastDeltaY == 0)
                {
                    if (thisDeltaX != 0 || thisDeltaY != -1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                if (lastDeltaX == 0 && lastDeltaY == -1)
                {
                    if (thisDeltaX != 1 || thisDeltaY != 0)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                if (lastDeltaX == 1 && lastDeltaY == 0)
                {
                    if (thisDeltaX != 0 || thisDeltaY != 1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                if (lastDeltaX == 0 && lastDeltaY == 1)
                {
                    if (thisDeltaX != -1 || thisDeltaY != 0)
                    {
                        FailWrongPattern();
                        return;
                    }
                } 
            }

            //OK to increment pattern
            moveCounter++;

            LogFile.Log.LogEntryDebug("StunBox OK - point: " + moveCounter.ToString(), LogDebugLevel.Medium);

            //Save the old deltas
            lastDeltaX = thisDeltaX;
            lastDeltaY = thisDeltaY;
        }

        private void FailWrongPattern()
        {
            LogFile.Log.LogEntryDebug("StunBox failed - wrong pattern", LogDebugLevel.Low);
            moveCounter = 0;
        }

        private void FailBlocked()
        {
            LogFile.Log.LogEntryDebug("StunBox failed - blocked", LogDebugLevel.Low);
            moveCounter = 0;
        }

        private void FailInterrupted()
        {
            LogFile.Log.LogEntryDebug("StunBox failed - interrupted", LogDebugLevel.Low);
            moveCounter = 0;
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override bool MoveComplete()
        {
            if (moveCounter == 4)
                return true;
            return false;
        }

        public override void DoMove(Point locationAfterMove)
        {
            //Move into the destination square like normal
            Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove);

            LogFile.Log.LogEntry("StunBox!");
            Game.MessageQueue.AddMessage("Stun Box!");
        }

        public override string MovieRoot()
        {
            return "stunbox";
        }
    }
}
