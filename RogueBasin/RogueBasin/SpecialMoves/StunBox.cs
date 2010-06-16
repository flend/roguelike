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
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int lastDeltaX { get; set; }
        public int lastDeltaY { get; set; }

        double stunRadius = 3.05;

        public StunBox()
        {
        }

        public override bool CheckAction(bool isMove, Point deltaMove, bool otherMoveSuccess)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            Point locationAfterMove = player.LocationMap + deltaMove;

            //No interruptions
            if (!isMove)
            {
                FailInterrupted();
                return false;
            }

            //Something in the square we're trying to enter blocks us
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

            //Monster
            if(squareContents.monster != null) {
                FailBlocked();
                return false;
            }

            //Bad terrain
            if(!dungeon.MapSquareIsWalkable(player.LocationLevel, locationAfterMove)) {
                FailBlocked();
                return false;
            }

            int thisDeltaX = locationAfterMove.x - player.LocationMap.x;
            int thisDeltaY = locationAfterMove.y - player.LocationMap.y;

            //First move, can be any non diagonal move
            if (moveCounter == 0)
            {
                if (thisDeltaX != 0 && thisDeltaY != 0)
                {
                    FailWrongPattern();
                    return false;
                }

                if (thisDeltaX == 0 && thisDeltaY == 0)
                {
                    FailWrongPattern();
                    return false;
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
                        return false;
                    }
                }

                if (lastDeltaX == 0 && lastDeltaY == -1)
                {
                    if (thisDeltaX != 1 || thisDeltaY != 0)
                    {
                        FailWrongPattern();
                        return false;
                    }
                }

                if (lastDeltaX == 1 && lastDeltaY == 0)
                {
                    if (thisDeltaX != 0 || thisDeltaY != 1)
                    {
                        FailWrongPattern();
                        return false;
                    }
                }

                if (lastDeltaX == 0 && lastDeltaY == 1)
                {
                    if (thisDeltaX != -1 || thisDeltaY != 0)
                    {
                        FailWrongPattern();
                        return false;
                    }
                } 
            }

            //OK to increment pattern
            moveCounter++;

            LogFile.Log.LogEntryDebug("StunBox OK - point: " + moveCounter.ToString(), LogDebugLevel.Medium);

            //Save the old deltas
            lastDeltaX = thisDeltaX;
            lastDeltaY = thisDeltaY;

            return true;
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

        public override void DoMove(Point deltaMove, bool noMove)
        {

            Point locationAfterMove = Game.Dungeon.Player.LocationMap + deltaMove;

            ClearMove();
            
            //Move into the destination square like normal
            Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove);

            //Stun everyone within the radius

            List<Monster> targets = new List<Monster>();
            foreach (Monster monster in Game.Dungeon.Monsters)
            {
                if (monster.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                if (Game.Dungeon.GetDistanceBetween(monster, Game.Dungeon.Player) < stunRadius)
                {
                    targets.Add(monster);
                }
            }

            //Stun these monsters
            foreach (Monster target in targets)
            {
                int duration = 250 + Game.Random.Next(500);
                target.AddEffect(new MonsterEffects.SlowDown(duration, target.Speed / 2));
            }

            LogFile.Log.LogEntryDebug("StunBox!", LogDebugLevel.Medium);
            Game.MessageQueue.AddMessage("Stunning Sphere!");
        }

        public override string MovieRoot()
        {
            return "stunbox";
        }

        public override string MoveName()
        {
            return "Stun Box";
        }

        public override string Abbreviation()
        {
            return "Stun";
        }

        public override int TotalStages()
        {
            return 4;
        }

        public override int CurrentStage()
        {
            return moveCounter;
        }

        public override int GetRequiredCombat()
        {
            return 100;
        }
    }
}
