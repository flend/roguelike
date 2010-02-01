using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    public class ChargeAttack : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; }

        Monster target;
        Point squareToMoveTo;
        bool moveReady;

        public ChargeAttack()
        {
            squareToMoveTo = new Point(0, 0);
            moveReady = false;
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
                LogFile.Log.LogEntryDebug("Charge started", LogDebugLevel.Medium);

                return true;
            }

            //Second move

            //Any direction without a monster. Subsequent moves needs to be in the same direction

            if (moveCounter == 1)
            {
                //Needs to be no monster in the direction of movement

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

                //Bad terrain
                if (!dungeon.MapSquareIsWalkable(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return false;
                }

                //Monster
                if (squareContents.monster != null)
                {
                    FailBlockingMonster();
                    return false;
                }

                xDelta = locationAfterMove.x - player.LocationMap.x;
                yDelta = locationAfterMove.y - player.LocationMap.y;

                moveCounter++;

                LogFile.Log.LogEntryDebug("Charge move: " + moveCounter, LogDebugLevel.Medium);
                return true;
            }

            //Later moves
            //if(moveCounter > 1) {
            else {
                //Needs to be no monster in the direction of movement

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);


                int thisxDelta = locationAfterMove.x - player.LocationMap.x;
                int thisyDelta = locationAfterMove.y - player.LocationMap.y;

                //Different direction
                if(thisxDelta != xDelta || thisyDelta != yDelta) {
                    FailWrongDirection();
                    return false;
                }

                //Bad terrain
                if (!dungeon.MapSquareIsWalkable(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return false;
                }

                //Monster - move is on
                if (squareContents.monster != null)
                {
                    moveReady = true;
                    target = squareContents.monster;
                    return true;
                }
                
                //Otherwise keep charging

                moveCounter++;

                LogFile.Log.LogEntryDebug("Charge move: " + moveCounter, LogDebugLevel.Medium);
                return true;
            }
          
        }

        private void FailWrongDirection() {

            moveCounter = 0;
            LogFile.Log.LogEntryDebug("Charge failed since wrong direction", LogDebugLevel.Medium);
        }

        private void FailBlocked()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("Charge failed since blocked", LogDebugLevel.Medium);
        }

        private void FailBlockingMonster()
        {
            moveCounter = 0;
            LogFile.Log.LogEntryDebug("Charge failed since monster at stage 1", LogDebugLevel.Medium);
        }

        public override bool MoveComplete()
        {
            return moveReady;
        }

        public override void DoMove(Point locationAfterMove)
        {
            //Bonus to hit and damage
            Game.MessageQueue.AddMessage("Charge attack!");
            int bonus = moveCounter;

            if (moveCounter > 5)
                bonus = 5;

            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, bonus, 0, bonus, 0);

            moveCounter = 0;
            moveReady = false;

            //Standard move into square, copied from PCMove

            bool okToMoveIntoSquare = false;

            if (results == CombatResults.DefenderDied)
            {
                okToMoveIntoSquare = true;
            }


            if (okToMoveIntoSquare)
            {
                Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove.x, locationAfterMove.y);
            }

            //Give the player a small speed up
            //Seems to mean you get a free attack about 1 time in 2
            //Game.Dungeon.Player.AddEffect(new PlayerEffects.SpeedUp(Game.Dungeon.Player, 50, 100));
            
            LogFile.Log.LogEntry("Charge complete");
            //Game.MessageQueue.AddMessage("Wall Vault!");
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MoveName()
        {
            return "Charge Attack";
        }

        public override string MovieRoot()
        {
            return "chargeattack";
        }

        public override string Abbreviation()
        {
            return "Chrg";
        }

        /// <summary>
        /// Effectively infinite, but bonus maxxes at 5
        /// </summary>
        /// <returns></returns>
        public override int TotalStages()
        {
            return 5;
        }

        public override int CurrentStage()
        {
            return moveCounter;
        }
    }
}
