﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// This is a follow-up move to a vault involving an attack as the last move. It therefore replicates wall vault and adds a new last move (may be a nicer way to do this)
    /// </summary>
    class VaultBackstab : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; } 

        Point squareToMoveTo;

        Monster target = null; //doesn't need to be serialized

        public VaultBackstab()
        {
            squareToMoveTo = new Point(0, 0);
        }

        public override void CheckAction(bool isMove, Point locationAfterMove)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //First move is pushing off against a wall
            //Second move is jumping over 0 or more creatures

            //Not a move or attack = reset
            if (!isMove)
            {
                moveCounter = 0;
                return;
            }

            //First move

            if (moveCounter == 0)
            {
                //Must be wall
                MapTerrain pushTerrain = dungeon.Levels[player.LocationLevel].mapSquares[locationAfterMove.x, locationAfterMove.y].Terrain;

                if (pushTerrain != MapTerrain.Wall && pushTerrain != MapTerrain.ClosedDoor)
                {
                    moveCounter = 0;
                    return;
                }

                //Is wall
                
                //Success
                moveCounter = 1;

                //Need to remember the direction of the first push, since we can only vault opposite this
                xDelta = locationAfterMove.x - player.LocationMap.x;
                yDelta = locationAfterMove.y - player.LocationMap.y;

                LogFile.Log.LogEntryDebug("Vault backstab stage 1", LogDebugLevel.Medium);

                return;                   
            }

            //Second move

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
                    return;
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
                        return;
                    }
                    if (squareY < 0 || squareY > thisMap.height)
                    {
                        NoWhereToJumpFail();
                        return;
                    }


                    MapTerrain squareTerrain = thisMap.mapSquares[squareX, squareY].Terrain;
                    SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(squareX, squareY));

                    //Into a wall
                    if (!thisMap.mapSquares[squareX, squareY].Walkable)
                    {
                        NoWhereToJumpFail();
                        return;
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

                LogFile.Log.LogEntryDebug("Vault backstab stage 2", LogDebugLevel.Medium);
                return;
            }

            //Third move, has to be attack in the opposite direction to the vault (i.e. same as original push)
            if (moveCounter == 2)
            {
                //Check direction is correct
                int secondXDelta = locationAfterMove.x - player.LocationMap.x;
                int secondYDelta = locationAfterMove.y - player.LocationMap.y;

                if (secondXDelta != xDelta || secondYDelta != yDelta)
                {
                    //Reset

                    moveCounter = 0;
                    return;
                }

                //Check there is a monster to attack
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y));

                //Is there a monster here? If so, we will attack it
                if (squareContents.monster != null)
                {
                    target = squareContents.monster;
                    moveCounter = 3;
                }
                else
                {
                    //This implies the monster is really fast and going elsewhere which I guess is possible
                    LogFile.Log.LogEntry("VaultBackstab failed due to no-one to stab!");
                    moveCounter = 0;
                }
                return;
            }
        }

        private void NoWhereToJumpFail()
        {
            moveCounter = 0;
            //Vault already tells us this
            //LogFile.Log.LogEntry("VaultBack failed due to nowhere to jump to");
        }

        public override bool MoveComplete()
        {
            if (moveCounter == 3)
                return true;
            return false;
        }

        public override void DoMove(Point locationAfterMove)
        {
            //Attack the monster with bonuses
            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, 3, 0, 4, -2);
            
            //Move into their square if the monster dies as normal
            bool okToMoveIntoSquare = false;
            if (results == CombatResults.DefenderDied)
                {
                    okToMoveIntoSquare = true;
                }
            

            if (okToMoveIntoSquare)
            {
                Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove.x, locationAfterMove.y);
            }
            
            moveCounter = 0;

            LogFile.Log.LogEntry("Wall backstab complete");
            Game.MessageQueue.AddMessage("Wall Backstab!");
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "wallvault";
        }
    }
}