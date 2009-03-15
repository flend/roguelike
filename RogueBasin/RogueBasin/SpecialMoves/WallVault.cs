using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    public class WallVault : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; }

        Monster target;
        Point squareToMoveTo;
        bool moveReady;

        public WallVault()
        {
            squareToMoveTo = new Point(0, 0);
            moveReady = false;
        }

        public override void CheckAction(bool isMove, Point locationAfterMove)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //First move is no direction move

            //Not a move or attack = reset
            if (!isMove)
            {
                moveCounter = 0;
                return;
            }

            //First move

            if (moveCounter == 0)
            {
                //Must be no direction
                if (Game.Dungeon.Player.LocationMap != locationAfterMove)
                {
                    return;
                }

                //Otherwise we're on
                moveCounter = 1;
                LogFile.Log.LogEntryDebug("Charge started", LogDebugLevel.Medium);

                return;
            }

            //Second move

            //Any direction without a monster. Subsequent moves needs to be in the same direction

            if (moveCounter == 1)
            {
                //Needs to be no monster in the direction of movement

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

                //Bad terrain
                if (!dungeon.MapSquareCanBeEntered(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return;
                }

                //Monster
                if (squareContents.monster != null)
                {
                    FailNoMonster();
                    return;
                }

                xDelta = locationAfterMove.x - player.LocationMap.x;
                yDelta = locationAfterMove.y - player.LocationMap.y;

                moveCounter++;

                LogFile.Log.LogEntryDebug("Charge move: " + moveCounter, LogDebugLevel.Medium);
                return;
            }

            //Later moves
            if(moveCounter > 1) {
                //Needs to be no monster in the direction of movement

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);


                int thisxDelta = locationAfterMove.x - player.LocationMap.x;
                int thisyDelta = locationAfterMove.y - player.LocationMap.y;

                //Different direction
                if(thisxDelta != xDelta || thisyDelta != yDelta) {
                    FailWrongDirection();
                    return;
                }

                //Bad terrain
                if (!dungeon.MapSquareCanBeEntered(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return;
                }

                //Monster - move is on
                if (squareContents.monster != null)
                {
                    moveReady = true;
                    target = squareContents.monster;
                    return;
                }
                
                //Otherwise keep charging

                moveCounter++;

                LogFile.Log.LogEntryDebug("Charge move: " + moveCounter, LogDebugLevel.Medium);
                return;
            }

            return;

           
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

        private void FailNoMonster()
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

                //Tell the player if there are multiple items in the square
                if (Game.Dungeon.MultipleItemAtSpace(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap))
                {
                    Game.MessageQueue.AddMessage("There are multiple items here.");
                }

                //If there is a feature and an item (feature will be hidden)
                if (Game.Dungeon.FeatureAtSpace(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap) != null &&
                    Game.Dungeon.ItemAtSpace(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap) != null)
                {
                    Game.MessageQueue.AddMessage("There is a staircase here.");
                }
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

        public override string MovieRoot()
        {
            return "wallvault";
        }
    }
}
