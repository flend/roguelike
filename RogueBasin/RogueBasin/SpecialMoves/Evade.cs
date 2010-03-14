using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// Evade . then direction of monster evades rather than attacks
    /// </summary>
    public class Evade : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        Point squareToMoveTo;

        public Evade()
        {
            squareToMoveTo = new Point(0, 0);
        }

        public override bool CheckAction(bool isMove, Point locationAfterMove)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //First move is no direction move

            //Not a move or attack = reset
            if (!isMove)
            {
                moveCounter = 0;
                return false;
            }

            //First move

            if (moveCounter == 0)
            {
                //Must be no direction
                if (Game.Dungeon.Player.LocationMap != locationAfterMove)
                {
                    return false;
                }

                //Otherwise we're on
                moveCounter = 1;
                LogFile.Log.LogEntryDebug("Evade started", LogDebugLevel.Medium);

                return true;
            }

            //Second move

            //Any direction with a monster. We skip past it and then one more square in that direction if we can

            if (moveCounter == 1)
            {
                //Needs to be a monster in the direction of movement
                
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

                //Bad terrain
                if (!dungeon.MapSquareIsWalkable(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return false;
                }

                //No Monster
                if(squareContents.monster == null) {
                    FailNoMonster();
                    return false;
                }

                //OK, so we have a monster to evade
                //Check the 2 squares behind it to find one to jump to (adapted from WallVault)

                int secondXDelta = locationAfterMove.x - player.LocationMap.x;
                int secondYDelta = locationAfterMove.y - player.LocationMap.y;

                Map thisMap = dungeon.Levels[player.LocationLevel];

                //We run forward until we find a square to jump to
                //If we run off the map or can't find a good square, we abort and the move is cancelled

                //Try to jump max of 2 squares, or fall back to one
                //Start square after monster
                int loopCounter = 2;

                //It's not possible to evade 2 monsters in a row, so worse case we stay where we are
                squareToMoveTo = player.LocationMap;

                do
                {
                    int squareX = player.LocationMap.x + secondXDelta * loopCounter;
                    int squareY = player.LocationMap.y + secondYDelta * loopCounter;

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
                    squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(squareX, squareY));

                    //Into a wall
                    if (!thisMap.mapSquares[squareX, squareY].Walkable)
                    {
                        break;
                    }

                    //Is there no monster here? If so, then OK square to jump to
                    if (squareContents.monster != null)
                    {
                        break;
                    }

                    //Empty, set this as OK and carry on looping
                    squareToMoveTo = new Point(squareX, squareY);
 
                    loopCounter++;
                } while (loopCounter < 4);

                //Check the status of the evade
                if (squareToMoveTo == player.LocationMap)
                {
                    FailBlocked();
                    return false;
                }

                //Otherwise we are on and will move in DoMove
                moveCounter = 2;
            }

            return true;
        }

        private void FailBlocked()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("Evade failed since blocked", LogDebugLevel.Medium);
        }

        private void FailNoMonster()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("Evade failed no one to evade!", LogDebugLevel.Medium);
        }

        public override bool MoveComplete()
        {
            if (moveCounter == 2)
                return true;
            return false;
        }

        public override void DoMove(Point locationAfterMove)
        {
            //Move the PC to the new location
            Game.Dungeon.MovePCAbsolute(Game.Dungeon.Player.LocationLevel, squareToMoveTo.x, squareToMoveTo.y);
            moveCounter = 0;

            LogFile.Log.LogEntry("Evade complete");
            Game.MessageQueue.AddMessage("Evade!");
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "evade";
        }

        public override string MoveName()
        {
            return "Evade";
        }

        public override string Abbreviation()
        {
            return "Eva";
        }

        public override int TotalStages()
        {
            return 2;
        }

        public override int CurrentStage()
        {
            return moveCounter;
        }

        public override int GetRequiredCombat()
        {
            return 40;
        }
    }
}
