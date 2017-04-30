﻿namespace RogueBasin.SpecialMoves
{
    public class ChargeAttack : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; }

        Monster target; //Doesn't last long enough to need serialization
        Point squareToMoveTo;
        bool moveReady;

        public ChargeAttack()
        {
            squareToMoveTo = new Point(0, 0);
            moveReady = false;
        }

        public override bool CheckAction(bool isMove, Point deltaMove, bool otherMoveSuccess)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Not a move or attack = reset
            if (!isMove)
            {
                moveCounter = 0;
                return false;
            }

            
            //First move
            /*
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
            }*/

            //Second move

            //Any direction without a monster. Subsequent moves needs to be in the same direction

            Point locationAfterMove = player.LocationMap + deltaMove;

            if (moveCounter == 0)
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

                if (xDelta == 0 && yDelta == 0)
                {
                    FailWrongDirection();
                    return false;
                }

                moveCounter++;
                
                LogFile.Log.LogEntryDebug("Charge started", LogDebugLevel.Medium);
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
                    //If it's a charmed monster don't attack
                    if (squareContents.monster.Charmed)
                    {
                        FailBlockingMonster();
                        return false;
                    }

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

        public override void DoMove(Point deltaMove, bool noMove)
        {
            Point locationAfterMove = Game.Dungeon.Player.LocationMap + deltaMove;
            
            //Bonus to hit and damage
            Game.MessageQueue.AddMessage("Charge attack!");
            int bonus = moveCounter;

            if (moveCounter > 5)
                bonus = 5;

            //Add a bonus for close quarters if applicable
            int noCardinals = FindNumberOfCardinals(target);
            if (noCardinals > 1)
            {
                bonus += noCardinals;
                Game.MessageQueue.AddMessage("Close Quarters!");
            }

            CombatResults results = Game.Dungeon.Combat.PlayerMeleeAttackMonsterWithModifiers(target, bonus, 0, bonus, 0, true);
            Screen.Instance.DrawMeleeAttack(Game.Dungeon.Player, target, results);

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
                Game.Dungeon.Movement.MovePCAbsoluteNoInteractions(Game.Dungeon.Player.LocationLevel, locationAfterMove);
            }

            //Give the player a small speed up
            //Seems to mean you get a free attack about 1 time in 2
            //Game.Dungeon.Player.AddEffect(new PlayerEffects.SpeedUp(Game.Dungeon.Player, 50, 100));
            
            LogFile.Log.LogEntryDebug("Charge complete", LogDebugLevel.Medium);
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

        public override int GetRequiredCombat()
        {
            return 30;
        }
    }
}
