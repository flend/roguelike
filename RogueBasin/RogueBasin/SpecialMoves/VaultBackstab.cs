using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// This is a follow-up move to a vault involving an attack as the last move. It therefore replicates wall vault and adds a new last move (may be a nicer way to do this)
    /// Note that it is not a special-movement move since the attack is in the direction of the keypress
    /// </summary>
    public class VaultBackstab : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; } 

        Point squareToMoveTo;

        Monster target = null; //Doesn't last long enough to need serialization

        public VaultBackstab()
        {
            squareToMoveTo = new Point(0, 0);
        }

        public override bool CheckAction(bool isMove, Point deltaMove, bool otherMoveSuccess)
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

                LogFile.Log.LogEntryDebug("Vault backstab stage 1", LogDebugLevel.Medium);

                return true;                   
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

                LogFile.Log.LogEntryDebug("Vault backstab stage 2", LogDebugLevel.Medium);
                return true;
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
                    return false;
                }

                //Check there is a monster to attack
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y));

                //Is there a monster here? If so, we will attack it
                if (squareContents.monster != null && !squareContents.monster.Charmed)
                {
                    target = squareContents.monster;
                    moveCounter = 3;

                    return true;
                }
                else
                {
                    //This implies the monster is really fast and going elsewhere which I guess is possible
                    LogFile.Log.LogEntry("VaultBackstab failed due to no-one to stab!");
                    moveCounter = 0;

                    return false;
                }
            }

            LogFile.Log.LogEntryDebug("Vault backstab move counter wrong", LogDebugLevel.Medium);
            return false;
        }

        private void NoMonsterToVaultFail()
        {
            moveCounter = 0;
            //LogFile.Log.LogEntryDebug("WallVault failed due to no monster to leap", LogDebugLevel.Low);
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

        public override void DoMove(Point deltaMove, bool noMove)
        {
            Point locationAfterMove = Game.Dungeon.Player.LocationMap + deltaMove;

            //Attack the monster with bonuses
            Game.MessageQueue.AddMessage("Wall Backstab!");

            int bonus = 0;
            int noCardinals = FindNumberOfCardinals(target);
            if (noCardinals > 1)
            {
                bonus = noCardinals;
                Game.MessageQueue.AddMessage("Close Quarters!");
            }

            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, 5, 0, 5 + bonus, -2 - bonus, true);
            
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
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "vaultbackstab";
        }

        public override string MoveName()
        {
            return "Vault Backstab";
        }

        public override string Abbreviation()
        {
            return "VtBS";
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
            return 50;
        }
    }
}
