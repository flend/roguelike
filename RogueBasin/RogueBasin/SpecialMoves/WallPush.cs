using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// Push off from a wall twice to push a monster a certain number of squares away until it hits an obstacle
    /// This may count as a normal attack, damage the monster, or irritate other monsters
    /// </summary>
    public class WallPush : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta {get; set;}
        public int yDelta { get; set;} 

        Point squareToMoveMonsterTo = new Point(0,0);
        Monster monsterToMove = null;

        public WallPush()
        {
            moveCounter = 0;
            xDelta = 0;
            yDelta = 0;
        }

        public override bool CheckAction(bool isMove, Point deltaMove)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            Point locationAfterMove = player.LocationMap + deltaMove;

            //First move is pushing off against a wall
            //Second move is jumping over 0 or more creatures

            //Not a move or attack = reset
            if (!isMove)
            {
                moveCounter = 0;
                return false;
            }

            //First move
            //Must be push off wall

            if (moveCounter == 0)
            {
                //Must be wall
                MapTerrain pushTerrain = dungeon.Levels[player.LocationLevel].mapSquares[locationAfterMove.x, locationAfterMove.y].Terrain;

                if (pushTerrain != MapTerrain.Wall && pushTerrain != MapTerrain.ClosedDoor)
                {
                    moveCounter = 0;
                    return false;
                }

                //Is wall
                
                //Success
                moveCounter = 1;

                //Need to remember the direction of the first push, since we can only vault opposite this
                xDelta = locationAfterMove.x - player.LocationMap.x;
                yDelta = locationAfterMove.y - player.LocationMap.y;

                LogFile.Log.LogEntryDebug("Wall push stage 1", LogDebugLevel.Medium);

                return true;                   
            }

            //Second move
            //Must be push off same wall
            if (moveCounter == 1)
            {
                int thisDeltaX = locationAfterMove.x - player.LocationMap.x;
                int thisDeltaY = locationAfterMove.y - player.LocationMap.y;

                //Must be exactly the same move as last time
                if (thisDeltaX != xDelta || thisDeltaY != yDelta)
                {
                    FailPattern();
                    return false;
                }

                //Success
                moveCounter = 2;

                LogFile.Log.LogEntryDebug("Wall push stage 2", LogDebugLevel.Medium);

                return true;
            }
            
            //if (moveCounter == 2)
            else
            {
                //Only implementing this for player for now!

                //Check that this direction opposes the initial push

                int secondXDelta = locationAfterMove.x - player.LocationMap.x;
                int secondYDelta = locationAfterMove.y - player.LocationMap.y;

                if (secondXDelta != -xDelta || secondYDelta != -yDelta)
                {
                    //Reset
                    FailPattern();
                    return false;
                }

                //OK, going in right direction

                //Firstly we need to check that there is a monster to push!
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y));

                if (squareContents.monster == null)
                {
                    FailNoMonsterToPush();
                    return false;
                }

                //Need to check what's ahead of the pushed monster

                Map thisMap = dungeon.Levels[player.LocationLevel];

                //We run forward from the monster position until we find a square that will make them stop
                //Default is to leave them where they are
                //(Remember to check the creature path finding / AI)

                //We also set a maximum push-back

                //Default is not to move the creature anywhere

                monsterToMove = squareContents.monster;
                squareToMoveMonsterTo = new Point(locationAfterMove.x, locationAfterMove.y);

                //Start at the square behind the monster
                int loopCounter = 1;

                do
                {
                    int squareX = locationAfterMove.x + secondXDelta * loopCounter;
                    int squareY = locationAfterMove.y + secondYDelta * loopCounter;

                    //Off the map
                    if (squareX < 0 || squareX > thisMap.width)
                    {
                        break;
                    }
                    if (squareY < 0 || squareY > thisMap.height)
                    {
                        break;
                    }

                    MapTerrain squareTerrain = thisMap.mapSquares[squareX, squareY].Terrain;
                    SquareContents thisContents = dungeon.MapSquareContents(player.LocationLevel, new Point(squareX, squareY));

                    //Into something non-walkable
                    if (!thisMap.mapSquares[squareX, squareY].Walkable)
                    {
                        break;
                    }

                    //Is there a monster here
                    if (thisContents.monster != null)
                    {
                        break;
                    }

                    //Nothing here, this would be an OK location for the creature to end up
                    squareToMoveMonsterTo = new Point(squareX, squareY);

                    loopCounter++;
                } while (loopCounter < 6);

                //Complete move
                moveCounter = 3;

                return true;
            }
        }

        private void FailNoMonsterToPush()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("WallVault failed - no monster to push", LogDebugLevel.Low);
        }

        private void FailPattern()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("WallVault failed - pattern error", LogDebugLevel.Low);
        }

        private void NoWhereToJumpFail()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("WallVault failed due to nowhere to jump to", LogDebugLevel.High);
        }

        public override bool MoveComplete()
        {
            if (moveCounter == 3)
                return true;
            return false;
        }

        public override void DoMove(Point deltaMove)
        {
            Point locationAfterMove = Game.Dungeon.Player.LocationMap + deltaMove;

            //Move the creature to the new location if required
            if (squareToMoveMonsterTo.x != locationAfterMove.x || squareToMoveMonsterTo.y != locationAfterMove.y)
            {
                Game.Dungeon.MoveMonsterAbsolute(monsterToMove, monsterToMove.LocationLevel, squareToMoveMonsterTo);
            }

            moveCounter = 0;

            LogFile.Log.LogEntry("Wall push complete");
            Game.MessageQueue.AddMessage("Wall Push!");
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "wallpush";
        }

        public override string MoveName()
        {
            return "Wall Push";
        }

        public override string Abbreviation()
        {
            return "WlPs";
        }

        public override int TotalStages()
        {
            return 3;
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
