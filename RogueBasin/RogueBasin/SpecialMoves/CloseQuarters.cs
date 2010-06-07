using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    public class CloseQuarters : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; }

        public int firstDeltaX { get; set; }
        public int firstDeltaY { get; set; }

        public int lastDeltaX { get; set; }
        public int lastDeltaY { get; set; }

        Monster target = null; //Doesn't last long enough to need serialization

        /// <summary>
        /// No squares we are next to, use for damage and hit bonus
        /// </summary>
        int noCardinals;

        public CloseQuarters()
        {
            moveCounter = 0;
        }

        public override bool CheckAction(bool isMove, Point deltaMove, bool otherMoveSuccess)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //No interruptions
            if (!isMove)
            {
                //FailInterrupted();
                return false;
            }

            //Close quarters as part of other attacks are taken care of in their functions, only process if this is not a special move
            if (otherMoveSuccess)
            {
                return false;
            }


            //1 step attack, when single enemy in contact and 3 cardinal directions of target are walls or other unwalkable things
            //and attack is in cardinal direction


            firstDeltaX = deltaMove.x;
            firstDeltaY = deltaMove.y;

            Point locationAfterMove = player.LocationMap + deltaMove;

            //Any non-diagonal move
            //if (firstDeltaX != 0 && firstDeltaY != 0)
            //{
                //FailWrongPattern();
                //return;
           // }

            //Check it is an attack
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y));

            //Is there a monster here?
            if (squareContents.monster != null)
            {
                //Check for charmed
                if (squareContents.monster.Charmed)
                {
                    return false;
                }

                //Check for already dead (maybe from a leap)
                if (!squareContents.monster.Alive)
                {
                    return false;
                }

                //Are 3 cardinal directions around the monster unwalkable?
                Point monsterLoc = squareContents.monster.LocationMap;

                noCardinals = 0;

                if (!dungeon.MapSquareIsWalkable(squareContents.monster.LocationLevel, new Point(monsterLoc.x - 1, monsterLoc.y)))
                    noCardinals++;

                if (!dungeon.MapSquareIsWalkable(squareContents.monster.LocationLevel, new Point(monsterLoc.x + 1, monsterLoc.y)))
                    noCardinals++;

                if (!dungeon.MapSquareIsWalkable(squareContents.monster.LocationLevel, new Point(monsterLoc.x, monsterLoc.y + 1)))
                    noCardinals++;

                if (!dungeon.MapSquareIsWalkable(squareContents.monster.LocationLevel, new Point(monsterLoc.x, monsterLoc.y - 1)))
                    noCardinals++;

                if (noCardinals > 1)
                {
                    target = squareContents.monster;
                    moveCounter = 1;
                    LogFile.Log.LogEntryDebug("CloseQuarters OK. No cards: " + noCardinals, LogDebugLevel.Low);
                    return true;
                }
                LogFile.Log.LogEntryDebug("CloseQuarters: not enough cardinals", LogDebugLevel.Low);
            }

            //Otherwise it's not a close quarters attack
            return false;
        }

        public override bool MoveComplete()
        {
            //Carry out any bar the 1st move (which is handled by the normal code)
            if (moveCounter > 0)
                return true;
            return false;
        }

        public override void DoMove(Point deltaMove, bool noMove)
        {
            Point locationAfterMove = Game.Dungeon.Player.LocationMap + deltaMove;

            //Attack the monster in its square with bonuses

            //Bonus to hit and damage
            Game.MessageQueue.AddMessage("CloseQuarters!");
            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, noCardinals, 0, noCardinals, 0, true);

            moveCounter = 0;

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
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "closequarters";
        }

        public override string Abbreviation()
        {
            return "ClQs";
        }

        public override string MoveName()
        {
            return "Close Quarters";
        }

        public override int TotalStages()
        {
            return 1;
        }

        public override int CurrentStage()
        {
            return moveCounter;
        }

        public override int GetRequiredCombat()
        {
            return 20;
        }

        public override bool NotSimultaneous()
        {
            return true;
        }
    }
}
