using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// This move is learnt with VaultBackstab and provides the initial move before the backstab
    /// </summary>
    public class WallVault : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; } 

        Point squareToMoveTo;

        public WallVault()
        {
            squareToMoveTo = new Point(0, 0);
        }

        public override bool CheckAction(bool isMove, Point deltaMove)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //First move is pushing off against a wall
            //Second move is jumping over 0 or more creatures

            Point locationAfterMove = player.LocationMap + deltaMove;

            //Not a move or attack = reset
            if (!isMove)
            {
                moveCounter = 0;
                return false;
            }

            //First move

            if (moveCounter == 0)
            {
                //Must be non-walkable to push off
                bool pushTerrainWalkable = dungeon.Levels[player.LocationLevel].mapSquares[locationAfterMove.x, locationAfterMove.y].Walkable;

                if (pushTerrainWalkable)
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

                LogFile.Log.LogEntryDebug("Wall vault stage 1", LogDebugLevel.Medium);

                return true;                   
            }

            //Second move
            //Now require a monster to be adjacent to us in order to leap over
            //This means you can't just leap 1 space off the wall but makes it more sensible for following up a WallLeap

            if (moveCounter == 1)
            {
                //Only implementing this for player for now!

                //Check that this direction opposes the initial push

                int secondXDelta = locationAfterMove.x - player.LocationMap.x;
                int secondYDelta = locationAfterMove.y - player.LocationMap.y;

                if (secondXDelta != -xDelta || secondYDelta != -yDelta)
                {
                    //Reset

                    moveCounter = 0;
                    return false;
                }

                //OK, going in right direction

                //Need to check what's ahead of the player

                //Empty squares, can jump 2
                Map thisMap = dungeon.Levels[player.LocationLevel];

                //We run forward until we find a square to jump to
                //If we run off the map or can't find a good square, we abort and the move is cancelled

                //First empty square
                int loopCounter = 1;

                do
                {
                    int squareX = player.LocationMap.x + secondXDelta * loopCounter;
                    int squareY = player.LocationMap.y + secondYDelta * loopCounter;

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

                    //Adjacent square, must be a monster
                    if (loopCounter == 1 && squareContents.monster == null)
                    {
                        NoMonsterToVaultFail();
                        return false;
                    }

                    //Is there no monster here? If so, this is our destination
                    if (squareContents.monster == null)
                    {
                        squareToMoveTo = new Point(squareX, squareY);
                        moveCounter = 2;
                        break;
                    }

                    //Monster here? Keep looping until we hit an empty or something bad

                    loopCounter++;
                } while (true);
            }

            return true;
        }

        private void NoWhereToJumpFail()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("WallVault failed due to nowhere to jump to", LogDebugLevel.Low);
        }

        private void NoMonsterToVaultFail()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("WallVault failed due to no monster to leap", LogDebugLevel.Low);
        }

        public override bool MoveComplete()
        {
            if (moveCounter == 2)
                return true;
            return false;
        }

        public override void DoMove(Point deltaMove)
        {
            Point locationAfterMove = Game.Dungeon.Player.LocationMap + deltaMove;

            //Move the PC to the new location
            Game.Dungeon.MovePCAbsolute(Game.Dungeon.Player.LocationLevel, squareToMoveTo.x, squareToMoveTo.y);
            moveCounter = 0;

            //Give the player a small speed up
            //Seems to mean you get a free attack about 1 time in 2
            Game.Dungeon.Player.AddEffect(new PlayerEffects.SpeedUp(Game.Dungeon.Player, 50, 150));

            LogFile.Log.LogEntry("Wall vault complete");
            Game.MessageQueue.AddMessage("Wall Vault!");
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "wallvault";
        }

        public override string MoveName()
        {
            return "Wall Vault";
        }

        public override string Abbreviation()
        {
            return "WlVt";
        }

        public override int TotalStages()
        {
            return 2;
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
            //Effective move is in the opposite direction to the push off
            return new Point(-xDelta, -yDelta);
        }


        public override int GetRequiredCombat()
        {
            return 9999;
        }
    }
}
