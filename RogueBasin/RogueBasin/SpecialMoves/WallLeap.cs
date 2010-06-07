using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// This move is learnt with VaultBackstab and provides the initial move before the backstab
    /// </summary>
    public class WallLeap : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; } 

        Point squareToMoveTo;

        Monster target = null; //Doesn't last long enough to need serialization
        int leapDistance = 0;

        public WallLeap()
        {
            squareToMoveTo = new Point(0, 0);
        }

        public override bool CheckAction(bool isMove, Point deltaMove, bool otherMoveSuccess)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //First move is pushing off against a wall
            //Second move is jumping over 0 or more creatures

            //Not a move or attack = reset
            if (!isMove)
            {
                moveCounter = 0;
                return false;
            }

            Point locationAfterMove = player.LocationMap + deltaMove;

            //First move

            if (moveCounter == 0)
            {
                //Must be non-walkable to push off
                bool pushTerrainWalkable = dungeon.Levels[player.LocationLevel].mapSquares[locationAfterMove.x, locationAfterMove.y].Walkable;

                if (pushTerrainWalkable)
                {
                    moveCounter = 0;
                    LogFile.Log.LogEntryDebug("Wall leap: No wall to push off", LogDebugLevel.Medium);
                    return false;
                }

                //Is wall

                xDelta = locationAfterMove.x - player.LocationMap.x;
                yDelta = locationAfterMove.y - player.LocationMap.y;

                int xReverseDelta = -xDelta;
                int yReverseDelta = -yDelta;

                //Need to check what's ahead of the player

                //Monster 1 square away is no good, monster several squares away is good

                Map thisMap = dungeon.Levels[player.LocationLevel];

                //We run forward until we find a square to jump to
                //If we run off the map or can't find a good square, we abort and the move is cancelled

                //First empty square
                int loopCounter = 1;

                do
                {
                    int squareX = player.LocationMap.x + xReverseDelta * loopCounter;
                    int squareY = player.LocationMap.y + yReverseDelta * loopCounter;

                    //Off the map
                    if (squareX < 0 || squareX > thisMap.width)
                    {
                        NoWhereToJumpFail();
                        return false;
                    }
                    if (squareY < 0 || squareY > thisMap.height)
                    {
                        NoWhereToJumpFail();
                        return false;
                    }

                    MapTerrain squareTerrain = thisMap.mapSquares[squareX, squareY].Terrain;
                    SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(squareX, squareY));

                    //Into a wall
                    if (!thisMap.mapSquares[squareX, squareY].Walkable)
                    {
                        NoWhereToJumpFail();
                        return false;
                    }

                    //Too far
                    if (loopCounter == 6)
                    {
                        LogFile.Log.LogEntryDebug("Wall leap: too far to monster - fail", LogDebugLevel.Medium);
                        return false;

                    }

                    //First loop, a monster is a problem

                    if (loopCounter == 1 && squareContents.monster != null)
                    {
                        LogFile.Log.LogEntryDebug("Wall leap: Monster in first position - fail", LogDebugLevel.Medium);
                        return false;
                    }

                    if (loopCounter > 1)
                    {
                        //Find monster to attack
                        if (squareContents.monster != null)
                        {
                            target = squareContents.monster;

                            if (squareContents.monster.Charmed)
                            {
                                return false;
                            }

                            //Distance jumped is the bonus
                            leapDistance = loopCounter - 1;

                            //Square to move to is the penultimate square
                            squareToMoveTo = new Point(player.LocationMap.x + xReverseDelta * (loopCounter - 1), player.LocationMap.y + yReverseDelta * (loopCounter - 1));
                            break;
                        }
                    }
                    //No monster? Keep looping until we find one

                    loopCounter++;
                } while (true);

                //Success
                moveCounter = 1;

                LogFile.Log.LogEntryDebug("Wall leap: stage 1 complete", LogDebugLevel.Medium);

                return true;                   
            }

            return true;
        }

        private void NoWhereToJumpFail()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("Wall Leap: failed due to nowhere to jump to", LogDebugLevel.Medium);
        }

        public override bool MoveComplete()
        {
            if (moveCounter == 1)
                return true;
            return false;
        }

        public override void DoMove(Point deltaMove, bool noMove)
        {
            //Attack the monster with bonuses
            Game.MessageQueue.AddMessage("Wall Leap!");

            int noCardinals = FindNumberOfCardinals(target);

            int bonus = 0;
            if (noCardinals > 1)
            {
                bonus = noCardinals;
                Game.MessageQueue.AddMessage("Close Quarters!");
            }


            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, leapDistance + 1 + bonus, 0, leapDistance + 1 + bonus, 0, true);

            //Move the PC to the new location
            Game.Dungeon.MovePCAbsolute(Game.Dungeon.Player.LocationLevel, squareToMoveTo.x, squareToMoveTo.y);
            /*
            //Move into their square if the monster dies as normal
            
            bool okToMoveIntoSquare = false;
            if (results == CombatResults.DefenderDied)
            {
                okToMoveIntoSquare = true;
            }


            if (okToMoveIntoSquare)
            {
                Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove.x, locationAfterMove.y);
            }*/
            
            moveCounter = 0;

            LogFile.Log.LogEntryDebug("Wall leap complete", LogDebugLevel.Medium);
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "wallleap";
        }

        public override string MoveName()
        {
            return "Wall Leap";
        }

        public override string Abbreviation()
        {
            return "WlLp";
        }

        public override int TotalStages()
        {
            return 1;
        }

        public override int CurrentStage()
        {
            return moveCounter;
        }

        public override bool CausesMovement()
        {
            return true;
        }

        public override Point RelativeMoveAfterMovement()
        {
            //Effective move is in the opposite direction to the push off, towards the monster
            return new Point(-xDelta, -yDelta);
        }

        public override int GetRequiredCombat()
        {
            return 60;
        }
    }
}
