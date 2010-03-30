using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    public class BurstOfSpeed : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        Point squareToMoveTo;

        public BurstOfSpeed()
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
                LogFile.Log.LogEntryDebug("Burst of Speed started", LogDebugLevel.Medium);

                return true;
            }

            //Second move

            if (moveCounter == 1)
            {
                //Must be no direction
                if (Game.Dungeon.Player.LocationMap != locationAfterMove)
                {
                    moveCounter = 0;
                    LogFile.Log.LogEntryDebug("Burst of Speed failed, move on 2", LogDebugLevel.Medium);
                    return false;
                }

                //Otherwise we're on
                moveCounter = 2;
                LogFile.Log.LogEntryDebug("Burst of Speed 2", LogDebugLevel.Medium);

                return true;
            }


            //Second move

            //Any direction. We skip up to 3 squares unless we get blocked

            if (moveCounter == 2)
            {
                int secondXDelta = locationAfterMove.x - player.LocationMap.x;
                int secondYDelta = locationAfterMove.y - player.LocationMap.y;

                Map thisMap = dungeon.Levels[player.LocationLevel];

                //We run forward until we find a square to jump to
                //If we run off the map or can't find a good square, we abort and the move is cancelled

                int loopCounter = 1;

                //Worst case we stay where we are
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
                    SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(squareX, squareY));

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
                } while (loopCounter < 3);

                //Check the status of the evade
                if (squareToMoveTo == player.LocationMap)
                {
                    FailBlocked();
                    return false;
                }

                //Otherwise we are on and will move in DoMove
                moveCounter = 3;
                return true;
            }

            LogFile.Log.LogEntryDebug("Burst of speed: moveCounter wrong", LogDebugLevel.Medium);
            return false;
        }

        private void FailBlocked()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("Burst of speed failed since blocked", LogDebugLevel.Medium);
        }

        public override bool MoveComplete()
        {
            if (moveCounter == 3)
                return true;
            return false;
        }

        public override void DoMove(Point locationAfterMove)
        {
            //Move the PC to the new location
            Game.Dungeon.MovePCAbsolute(Game.Dungeon.Player.LocationLevel, squareToMoveTo.x, squareToMoveTo.y);
            moveCounter = 0;

            //Give the player a small speed up
            //Seems to mean you get a free attack about 1 time in 2
            int duration = 150 + Game.Random.Next(400);
            Game.Dungeon.Player.AddEffect(new PlayerEffects.SpeedUp(Game.Dungeon.Player, 150, 150));
            Game.Dungeon.Player.RecalculateCombatStatsRequired = true;

            LogFile.Log.LogEntry("Burst of Speed complete");
            Game.MessageQueue.AddMessage("Burst of Speed!");
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "burstofspeed";
        }

        public override string MoveName()
        {
            return "Burst of Speed";
        }

        public override string Abbreviation()
        {
            return "BoS";
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
            return 80;
        }
    }
}
